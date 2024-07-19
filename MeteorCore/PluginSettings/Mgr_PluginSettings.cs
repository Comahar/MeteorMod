using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using MeteorCore.Setting.Interfaces;
using MeteorCore.Setting.Interfaces.Internal;

/// DO NOT USE MeteorCore.Setting.AbstractClasses
/// This class must work with any setting type that implements IPluginSetting

namespace MeteorCore.Setting;
public class Mgr_PluginSettings : MonoBehaviour {
    private static Mgr_PluginSettings _instance;

    public static Mgr_PluginSettings Instance {
        get {
            if(_instance == null)
                _instance = FindObjectOfType<Mgr_PluginSettings>();
            return _instance;
        }
    }
    private void Awake() {
        if(_instance == null)
            _instance = this;
        if(_instance != this)
            Destroy(this);
        SceneManager.sceneLoaded += this.SceneLoaded;
    }

    public static GameObject Create() {
        // Mgr_PluginSettings gameobject
        GameObject gameObject = new GameObject("Mgr_PluginSettings");
        gameObject.AddComponent<Mgr_PluginSettings>();
        gameObject.transform.SetParent(Plugin.PluginManager);
        GameObject prefabs = SettingsUIPrefabs.Create();
        prefabs.transform.SetParent(gameObject.transform);
        return gameObject;
    }

    public static bool SceneHasSettingsMenu {
        // title, gameplay, and lnl
        get {
            return SceneHelper.IsTitleScene || SceneHelper.IsGameScene || SceneHelper.IsLNLScene;
        }
    }
    public static Transform settingPagesHolder;

    Dictionary<string, PluginSettingPageContainer> settingPages = new Dictionary<string, PluginSettingPageContainer>();
    List<PluginSettingContainer> settings = new List<PluginSettingContainer>();
    Dictionary<BepInPlugin, ConfigFile> configFiles = new Dictionary<BepInPlugin, ConfigFile>();
    class PluginSettingContainer {
        public BepInPlugin ownerMetadata;
        public ConfigEntryBase configEntryBase;
        public string pageName;
        public IPluginSetting setting;
        public Type settingUIItemType;
    }

    class PluginSettingPageContainer {
        public string pageGameObjectName;
        public string pageName;
        public string dictionaryName;
        public Type pageType;
        public IPluginSettingsPage pageInstance;
    }

    public static void AddPage<T>(string pageName, string dictionaryName, string pageGameObjectName) where T : IPluginSettingsPage {
        if(Instance.settingPages.ContainsKey(pageName)) {
            Plugin.Logger.LogError($"Setting page {pageName} already exists");
            return;
        }
        Plugin.Logger.LogInfo("Adding setting page " + pageName);
        Instance.settingPages.Add(pageName, new PluginSettingPageContainer() {
            pageGameObjectName = pageGameObjectName,
            dictionaryName = dictionaryName,
            pageName = pageName,
            pageType = typeof(T),
            pageInstance = null
        });
    }


    /*
    * Could not implement as TUIItem : MonoBehaviour, IPluginSettingUIItem<IPluginSetting<TSettingValue>, TSettingValue> {
    * because covariance is needed when for IPluginSetting supplying IPluginSettingUIItem<IPluginSetting<TSettingValue>, TSettingValue>
    * however it can't be used as methods cant take covariant types in IPluginSettingUIItem.SetSetting
    * this will loosen the user restrictions on the IPluginSettingUIItem generic which may result in a runtime error
    * if the user tries to use a IPluginSettingUIItem with a different IPluginSetting type
    */
    public static void AddSetting<TSettingValue, TUIItem>(IPluginSetting<TSettingValue> setting, string pageName, BepInPlugin owner)
    where TUIItem : MonoBehaviour, IPluginSettingUIItem {
        Plugin.Logger.LogInfo($"Adding setting {setting.settingKey} to page {pageName}");
        if(!Instance.settingPages.ContainsKey(pageName)) {
            Plugin.Logger.LogError($"Setting is not added as the page {pageName}, it does not exist. Make sure to call AddPage first");
            return;
        }

        try {
            GameObject testGameObject = new GameObject($"Generic Test {typeof(TUIItem)}");
            Component testComponent = testGameObject.AddComponent<TUIItem>();
            IPluginSettingUIItem testItem = testComponent as IPluginSettingUIItem;
            if(testItem == null) {
                Plugin.Logger.LogError($"Could not add setting ui item {typeof(TUIItem)}, it may have a generic type problem. Make sure to pass a concrete type.");
                return;
            }
            Destroy(testGameObject);
        } catch(Exception e) {
            Plugin.Logger.LogError($"Could not add setting ui item {typeof(TUIItem)}, it may have a generic type problem. Make sure to pass a concrete type.");
            Plugin.Logger.LogError(e);
            throw e;
        }


        // create config file
        if(!Instance.configFiles.ContainsKey(owner)) {
            Instance.CreateConfigFile(owner);
        }
        // Create config entry
        var configEntry = Instance.configFiles[owner].Bind<TSettingValue>(
            section: pageName,
            key: setting.settingKey,
            defaultValue: setting.DefaultValue,
            description: setting.tooltip
        );

        Instance.settings.Add(new PluginSettingContainer() {
            ownerMetadata = owner,
            configEntryBase = configEntry,
            pageName = pageName,
            setting = setting,
            settingUIItemType = typeof(TUIItem)
        });

        // set current setting value to config entry value
        setting.SetValue(configEntry.Value, save: false, pending: false, notify: false);
    }

    private void SceneLoaded(Scene scene, LoadSceneMode mode) {
        // call SettingsUIPrefabs.SceneLoaded to initialize ui prefabs
        SettingsUIPrefabs.Instance.SceneLoaded(scene, mode);
        // only run when scene has settings menu
        if(SceneHasSettingsMenu) {
            // create new pages
            foreach(KeyValuePair<string, PluginSettingPageContainer> page in this.settingPages) {
                Plugin.Logger.LogInfo("Creating page " + page.Key);
                var pageContainer = page.Value;
                var pageInstance = this.CreatePageInstance(pageContainer);
                pageContainer.pageInstance = pageInstance;
            }
            // create settings
            foreach(PluginSettingContainer PluginSettings in this.settings) {
                Plugin.Logger.LogInfo("Creating setting UI item for" + PluginSettings.setting.settingKey);
                var pageName = PluginSettings.pageName;
                if(!this.settingPages.ContainsKey(pageName)) {
                    return;
                }
                var pageInstance = this.settingPages[pageName].pageInstance;
                var uiItem = this.CreateUIItem(PluginSettings.settingUIItemType, PluginSettings.setting, pageInstance.settingsHolder);
                pageInstance.AddSettingUIItem(uiItem);
                uiItem.SetSettingInternal(PluginSettings.setting);
            }
            // call OnInitialized on all pages
            foreach(PluginSettingPageContainer pageContainer in this.settingPages.Values) {
                pageContainer.pageInstance.OnInitialized();
            }
        }
    }

    // settingType has to be a IPluginSettingUIItem
    private IPluginSettingUIItem CreateUIItem(Type settingType, IPluginSetting setting, Transform parent) {
        try {
            var uiItemGameObject = new GameObject(settingType.Name);
            Component uiItemComponent = uiItemGameObject.AddComponent(settingType);
            IPluginSettingUIItem uiItem = uiItemComponent as IPluginSettingUIItem;
            if(uiItem == null) {
                Plugin.Logger.LogError("Could not create ui item instance of type " + settingType.Name + " does it implement IPluginSettingUIItem?");
                return null;
            }
            // not using previous uiItemGameObject or uiItem is important, Initialize method may behave lika a static method
            // it may return a different gameobject than itself
            var newUIItemGameObject = uiItem.Initialize(parent, setting);

            return newUIItemGameObject.GetComponent<IPluginSettingUIItem>();
        } catch(Exception e) {
            Plugin.Logger.LogError($"Error creating ui item instance of type {settingType.Name}");
            Plugin.Logger.LogError(e);
            throw e;
        }
    }

    // pageType has to be a IPluginSettingsPage
    private IPluginSettingsPage CreatePageInstance(PluginSettingPageContainer pageContainer) {
        var pageGameObject = new GameObject(pageContainer.pageGameObjectName);
        pageGameObject.transform.SetParent(settingPagesHolder);
        IPluginSettingsPage pageInstance = pageGameObject.AddComponent(pageContainer.pageType) as IPluginSettingsPage;
        if(pageInstance == null) {
            Plugin.Logger.LogError($"Could not create page instance of type {pageContainer.pageType?.Name} with name {pageContainer.pageGameObjectName} does it implement IPluginSettingsPage?");
        }
        try {
            // not using previous pageGameObject is important, Initialize method may behave lika a static method
            pageGameObject = pageInstance.Initialize(pageContainer.pageName, this.GetPageSettingsByPageName(pageContainer.pageName));
        } catch(Exception e) {
            Plugin.Logger.LogError($"Could not initialize page instance of type {pageContainer.pageType?.Name} with name {pageContainer.pageGameObjectName}");
            Plugin.Logger.LogError(e);
            throw e;
        }
        return pageGameObject.GetComponent<IPluginSettingsPage>();
    }

    private List<IPluginSetting> GetPageSettingsByPageName(string pageName) {
        var pageSettings = new List<IPluginSetting>();
        foreach(PluginSettingContainer PluginSetting in this.settings) {
            if(PluginSetting.pageName == pageName) {
                pageSettings.Add(PluginSetting.setting);
            }
        }
        return pageSettings;
    }

    private void CreateConfigFile(BepInPlugin ownerMetadata) {
        var path = System.IO.Path.Combine(Paths.ConfigPath, ownerMetadata.Name + ".cfg");
        var config = new ConfigFile(path, true, ownerMetadata);
        config.SaveOnConfigSet = false;
        this.configFiles.Add(ownerMetadata, config);
    }

    public void ApplySettings() {
        foreach(PluginSettingContainer PluginSetting in this.settings) {
            PluginSetting.setting.ApplyPendingValue();
        }
    }

    public void RevertSettings() {
        foreach(PluginSettingContainer PluginSetting in this.settings) {
            PluginSetting.setting.RevertPendingValue();
        }
    }

    public void SaveSettings() {
        foreach(PluginSettingContainer PluginSettings in this.settings) {
            var configEntry = PluginSettings.configEntryBase;
            configEntry.BoxedValue = PluginSettings.setting.BoxedValue;
        }
        foreach(ConfigFile configFile in this.configFiles.Values) {
            configFile.Save();
        }
        Plugin.Logger.LogInfo("Saved settings");
    }
    public bool HaveSettingsChangedWhileOpen() {
        foreach(PluginSettingContainer PluginSetting in this.settings) {
            if(PluginSetting.setting.isDirty)
                return true;
        }
        return false;
    }
}


[HarmonyPatch(typeof(SettingsMenu), nameof(SettingsMenu.HaveSettingsChangedWhileOpen))]
public class SettingsPage_HaveSettingsChangedWhileOpen_Patch {
    public static void Postfix(ref bool __result) {
        __result = __result || Mgr_PluginSettings.Instance.HaveSettingsChangedWhileOpen();
    }
}

// Patch for saving plugin settings
[HarmonyPatch(typeof(Mgr_Settings), nameof(Mgr_Settings.ApplySettings))]
public class Mgr_Settings_ApplySettings_Patch {
    public static void Postfix() {
        Mgr_PluginSettings.Instance.ApplySettings();
        Mgr_PluginSettings.Instance.SaveSettings();
    }
}

// Patch for reverting plugin settings
[HarmonyPatch(typeof(SettingsMenu), nameof(SettingsMenu.RevertAllPendingSettingsChanges))]
public class SettingsMenu_RevertAllPendingSettingsChanges_Patch {
    public static void Postfix() {
        Mgr_PluginSettings.Instance.RevertSettings();
    }
}