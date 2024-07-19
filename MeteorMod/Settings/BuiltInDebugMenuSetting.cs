using UnityEngine;
using UnityEngine.SceneManagement;
using MeteorCore.Setting;
using MeteorCore.Setting.Interfaces;

namespace MeteorMod.Settings;

public static class BuiltInDebugMenuSetting {
    public static PluginSettingBool enableDebugSetting;

    public static DebugMenuToggle debugMenuToggle;
    public static Mgr_DebugTools debugTools;


    public static void Init() {
        enableDebugSetting = new PluginSettingBool(
            settingKey: "EnableDebugMenu",
            settingName: "Enable Debug Menu",
            tooltip: "Enables debugTools and debug menu\n\nAccess the menu with CTRL+SHIFT+D",
            configSection: MyPluginInfo.PLUGIN_NAME,
            owner: Plugin.metadata,
            defaultValue: false
        );

        Mgr_PluginSettings.AddSetting<bool, PluginSettingToggleUIItem>(enableDebugSetting, Plugin.settingPageName, Plugin.metadata);
        enableDebugSetting.OnValueChanged += DebugSettingChanged;

        SceneManager.sceneLoaded += SceneLoaded;
    }

    private static void DebugSettingChanged(IPluginSetting<bool> setting) {
        bool settingValue = setting.Value;
        SetDebugMenuState(settingValue);
    }

    private static bool firstSceneLoad = true;
    public static void SceneLoaded(Scene scene, LoadSceneMode mode) {
        // run only once on the title scene
        // early return if not title scene
        if(!MeteorCore.SceneHelper.IsTitleScene || !firstSceneLoad)
            return;
        Plugin.Logger.LogWarning($"BuiltInDebugMenuSetting SceneLoaded");

        firstSceneLoad = false;

        // get debug gameobject, early return if not found
        string gameObjectPath = "MANAGER_MASTER/Debug";
        GameObject debugToolsGameObject = GameObject.Find(gameObjectPath);
        if(debugToolsGameObject == null) {
            Plugin.Logger.LogError("Could not get debug object," + gameObjectPath + "GameObject not found");
            return;
        }
        debugTools = debugToolsGameObject.GetComponent<Mgr_DebugTools>();
        if(debugTools == null) {
            Plugin.Logger.LogError("Could not get debug, Mgr_DebugTools component not in GameObject" + gameObjectPath);
            return;
        }

        // get debug button gameobject
        debugMenuToggle = UnityEngine.Object.FindObjectOfType<DebugMenuToggle>(true);
        if(debugMenuToggle == null) {
            Plugin.Logger.LogError("Could not get DebugMenuToggle");
            return;
        }

        // set debug menu state to setting value
        SetDebugMenuState(enableDebugSetting.Value);
    }

    public static void SetDebugMenuState(bool state) {
        if(debugTools == null || debugMenuToggle == null) {
            Plugin.Logger.LogWarning("BuiltInDebugMenuSetting errored at Init, cannot apply debug setting");
            return;
        }

        if(IsMenuActive()) {
            debugMenuToggle.ToggleAllDebugBoxes();
        }

        if(state) {
            debugTools.DisableAllDebug = false;
            Plugin.Logger.LogInfo("Enabled GVH debug");

            debugMenuToggle.gameObject.SetActive(true);
            Plugin.Logger.LogInfo("Enabled GVH debug button");
        } else {
            debugTools.DisableAllDebug = true;
            Plugin.Logger.LogInfo("Disabled GVH debug");

            debugMenuToggle.gameObject.SetActive(false);
            Plugin.Logger.LogInfo("Disabled GVH debug button");

        }
    }

    public static bool IsMenuActive() {
        if(debugMenuToggle == null) {
            Plugin.Logger.LogWarning("DebugMenuToggle is null");
            return false;
        }
        if(debugMenuToggle.debugGameobjects.Length == 0) {
            Plugin.Logger.LogWarning("debugGameobjects is empty");
            return false;
        }
        return debugMenuToggle.debugGameobjects[0].activeInHierarchy;
    }
}
