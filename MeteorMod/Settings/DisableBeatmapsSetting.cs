using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using MeteorCore.Setting;
using MeteorCore.Setting.Interfaces;

namespace MeteorMod.Settings;

public static class DisableBeatmapsSetting {
    public static PluginSettingBool disableBeatmapsSetting;

    private static GameObject GameplayUI;
    public static List<KeyCode> KeyCombos = new List<KeyCode> { KeyCode.LeftControl, KeyCode.LeftShift, KeyCode.F };
    private static bool comboKeyDown = false;

    private static bool IsPressingCombo {
        get {
            foreach(KeyCode key in KeyCombos) {
                if(!Input.GetKey(key)) {
                    return false;
                }
            }
            return true;
        }
    }

    public static void Init() {
        disableBeatmapsSetting = new PluginSettingBool(
            settingKey: "DisableBeatmaps",
            settingName: "Disable Beatmaps",
            tooltip: "Disables rhythm game segments' interactivity and UI.\nWhile in a performance toggle with CTRL+SHIFT+F",
            configSection: MyPluginInfo.PLUGIN_NAME,
            owner: Plugin.metadata,
            defaultValue: false
        );

        Mgr_PluginSettings.AddSetting<bool, PluginSettingToggleUIItem>(disableBeatmapsSetting, Plugin.settingPageName, Plugin.metadata);

        disableBeatmapsSetting.OnValueChanged += DisableBeatmapsSettingChanged;

        SceneManager.sceneLoaded += SceneLoaded;
        Plugin.OnUpdate += OnUpdate;
    }

    public static void SceneLoaded(Scene scene, LoadSceneMode mode) {
        if(!MeteorCore.SceneHelper.IsPerformenceScene)
            return;
        GameplayUI = GameObject.Find("GameplayUI Root");
        if(GameplayUI == null) {
            Plugin.Logger.LogWarning("Could not find GameplayUI Root");
            return;
        }
        DisableBeatmapsSettingChanged(disableBeatmapsSetting);
    }

    public static void OnUpdate() {
        if(GameplayUI == null) {
            return;
        }
        if(IsPressingCombo) {
            if(!comboKeyDown) {
                comboKeyDown = true;
                disableBeatmapsSetting.SetValue(value: !disableBeatmapsSetting.Value, save: true, pending: false, notify: true);
            }
        } else {
            comboKeyDown = false;
        }
    }

    public static void DisableBeatmapsSettingChanged(IPluginSetting<bool> setting) {
        SetBeatmapState(!setting.Value);
    }

    public static void SetBeatmapState(bool state) {
        if(GameplayUI == null) {
            return;
        }
        GameplayUI.SetActive(state);
        Plugin.Logger.LogInfo($"Set beatmap GameplayUI to {state}");
    }

}
