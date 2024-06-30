using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using BepInEx;
using BepInEx.Configuration;
using System.IO;
using UnityEngine.SceneManagement;
using MeteorMod.ModSettings.ModSettingItems;

namespace MeteorMod.ModSettings {
    public class Mgr_ModSettings : MonoBehaviour {
        private static Mgr_ModSettings _instance;

        public static Mgr_ModSettings Instance {
            get {
                if(_instance == null)
                    _instance = FindObjectOfType<Mgr_ModSettings>();
                return _instance;
            }
        }
        private void Awake() {
            if (_instance == null)
                _instance = this;
            if (_instance != this)
                Destroy(this);
            SceneManager.sceneLoaded += SceneLoaded;
        }

        private ModSettingsPage defaultPage;

        // Mod Settings
        /*public Dictionary<BepInPlugin, List<Setting>> modSettings {
            get {
                return _modSettings;
            }
        }*/

        //private static Dictionary<BepInPlugin, List<Setting>> _modSettings = new Dictionary<BepInPlugin, List<Setting>>();
        //private static Dictionary<BepInPlugin, ConfigFile> configFiles = new Dictionary<BepInPlugin, ConfigFile>();
        //private static Dictionary<BepInPlugin, List<ConfigEntryBase>> configEntries = new Dictionary<BepInPlugin, List<ConfigEntryBase>>();
        
        public Dictionary<string, ModSettingsPage> settingPages = new Dictionary<string, ModSettingsPage>();
        public Dictionary<ModSettingsPage, IModSetting> settingPageToSettingMap = new Dictionary<ModSettingsPage, IModSetting>();
        public Dictionary<BepInPlugin, ModSettingContainer> modSettingsContainer = new Dictionary<BepInPlugin, ModSettingContainer>();

        
        public void SceneLoaded(Scene scene, LoadSceneMode mode) {
            Plugin.LOG.LogWarning($"Mgr_ModSettings SceneLoaded isTitleScene: {SceneHelper.IsTitleScene} IsGameScene: {SceneHelper.IsGameScene} IsLNLScene: {SceneHelper.IsLNLScene}");
            // only run when scene has settings menu
            // title, gameplay, and lnl
            if(SceneHelper.IsTitleScene || SceneHelper.IsGameScene || SceneHelper.IsLNLScene) {
                // Create default mod settings page
                // TODO create a hook for creating multiple pages
                defaultPage = ModSettingsPage.Create("ModsSettingPage", "Mods");
                foreach(KeyValuePair<BepInPlugin, ModSettingContainer> container in modSettingsContainer) {
                    foreach(ModSetting<object> setting in container.Value.settings) {
                        SettingsItem settingsItemUI = setting.CreateUIElement();
                        if(settingsItemUI != null) {
                            defaultPage.AddSettingUIItem(settingsItemUI);
                        } else {
                            Plugin.LOG.LogWarning($"Could not create UI element for {setting.settingKey} in {container.Key.Name}");
                        }
                    }
                }
            }
            Plugin.LOG.LogWarning($"Mgr_ModSettings SceneLoaded finish");
        }

        public static void AddSetting<T>(BepInPlugin owner, MeteorMod.ModSettings.ModSettingItems.ModSetting<T> setting, ModSettingsPage page) {
            if(!Instance.modSettingsContainer.ContainsKey(owner)) {
                Instance.modSettingsContainer.Add(owner, new ModSettingContainer());
            }
            var settingObj = setting as ModSetting<object>;
            if (settingObj == null) {
                Plugin.LOG.LogError($"Could not cast {setting.GetType()} to ModSetting<object> with name {setting.settingKey} required for {owner.Name}");
                return;
            }
            Instance.modSettingsContainer[owner].settings.Add(settingObj);
        }

        /*
        public static bool RemoveSetting<T, D>(BepInPlugin ownerMetadata, T setting, ModSettingsPage page) where T : Setting, IModSetting<D> {
            if(!_modSettings.ContainsKey(ownerMetadata)) {
                return false;
            }
            return _modSettings[ownerMetadata].Remove(setting);
        }

        public static void AddSetting<T, D>(BepInPlugin ownerMetadata, T setting, string settingPageName, bool createUIItem) where T : Setting<D>, IModSetting<D> {
            
            if (!configFiles.ContainsKey(ownerMetadata)) {
                var path = Path.Combine(Paths.ConfigPath, ownerMetadata.Name + ".cfg");
                var config = new ConfigFile(path, true, ownerMetadata);
                config.SaveOnConfigSet = false;
                configFiles.Add(ownerMetadata, config);
            }
            
            if (!configEntries.ContainsKey(ownerMetadata)) {
                configEntries.Add(ownerMetadata, new List<ConfigEntryBase>());
            }

            var configFile = configFiles[ownerMetadata];
            var configDefinition = new ConfigDefinition(ownerMetadata.Name, setting.settingKey);

            var configEntry = configFile.Bind(configDefinition, setting.defaultValue);
            configEntries[ownerMetadata].Add(configEntry);


            if(!_modSettings.ContainsKey(ownerMetadata)) {
                _modSettings.Add(ownerMetadata, new List<Setting>());
            }
            _modSettings[ownerMetadata].Add(setting);


            //settingPageMap.Add(settingPageName, setting);

            // set using SetValue with pending: false to not make it dirty
            setting.SetValue(configEntry.Value, pending: false, notify: false);
        }

        public static void AddSetting<T, D>(BepInPlugin ownerMetadata, T setting, string settingPageName) where T : Setting<D>, IModSetting<D> {
            AddSetting<T, D>(ownerMetadata, setting, settingPageName, true);
        }

        public static void AddSetting<T, D>(BepInPlugin ownerMetadata, T setting, bool createUIItem) where T : Setting<D>, IModSetting<D> {
            AddSetting<T, D>(ownerMetadata, setting, "default", createUIItem);
        }

        private void AddSettingItem<T>(T setting, ModSettingsPage page) where T : Setting, IModSetting {
            page.CreateSettingUIItem(setting);
        }

        private void AddSettingItem<T, D>(T setting, ModSettingsPage page) where T : Setting<D>, IModSetting {
            page.CreateSettingUIItem<T, D>(setting);
        }*/

        // normally this is in SettingsPage class however this is better handedled here
        // as a bonus this class is singleton so accessing inside static methods are easier
        public bool HaveSettingsChanged() {
            foreach(KeyValuePair<BepInPlugin, ModSettingContainer> container in modSettingsContainer) {
                foreach(ModSetting<object> setting in container.Value.settings) {
                    if(setting.isDirty)
                        return true;
                }
            }
            return false;
        }

        public void SaveSettings() {
            foreach(KeyValuePair<BepInPlugin, ModSettingContainer> container in modSettingsContainer) {
                var configFile = container.Value.configFile;
                var configEntries = container.Value.configEntries;
                foreach(ModSetting<object> setting in container.Value.settings) {
                    var configEntry = configEntries.Find(
                        entry => entry.Definition.Key == setting.settingKey
                    );
                    configEntry.BoxedValue = setting.GetValue(true);
                }
                configFile.Save();
            }
        }
    }



    public class ModSettingContainer {
        public ConfigFile configFile;
        public List<ConfigEntryBase> configEntries;
        public List<ModSetting<object>> settings;
        public BepInPlugin ownerMetadata;
    }


    // Patch for checking if mod settings have changed
    [HarmonyPatch(typeof(SettingsMenu), nameof(SettingsMenu.HaveSettingsChangedWhileOpen))]
    public class SettingsPage_HaveSettingsChangedWhileOpen_Patch {
        public static void Postfix(ref bool __result) {
            __result = __result || Mgr_ModSettings.Instance.HaveSettingsChanged();
        }
    }

    // Patch for saving mod settings
    [HarmonyPatch(typeof(Mgr_Settings), nameof(Mgr_Settings.ApplySettings))]
    public class Mgr_Settings_ApplySettings_Patch {
        public static void Postfix() {
            Plugin.LOG.LogInfo("Saving");
            try {
                foreach(KeyValuePair<BepInPlugin, ModSettingContainer> container in Mgr_ModSettings.Instance.modSettingsContainer) {
                    foreach(ModSetting<object> setting in container.Value.settings) {
                        setting.ApplyPendingValue();
                    }
                }
                Mgr_ModSettings.Instance.SaveSettings();
                Plugin.LOG.LogInfo("Mod settings saved");
            } catch(Exception e) {
                Plugin.LOG.LogError("Error saving mod settings: " + e);
            }
        }
    }

    // Patch for reverting mod settings
    [HarmonyPatch(typeof(SettingsMenu), nameof(SettingsMenu.RevertAllPendingSettingsChanges))]
    public class SettingsMenu_RevertAllPendingSettingsChanges_Patch {
        public static void Postfix() {
            foreach(KeyValuePair<BepInPlugin, ModSettingContainer> container in Mgr_ModSettings.Instance.modSettingsContainer) {
                foreach(ModSetting<object> setting in container.Value.settings) {
                    setting.RevertPendingValue();
                }
            }
        }
    }
}
