using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using MeteorCore.Setting;
using MeteorCore.Setting.Interfaces;

namespace MeteorMod.Settings;
public static class ShowHiddenSettingsSetting {
    public static PluginSettingBool enableHiddenSettingsSetting;

    private static List<SettingsItem> disabledSettings = new List<SettingsItem>();

    public static void Init() {
        enableHiddenSettingsSetting = new PluginSettingBool(
            settingKey: "ShowHiddenSettings",
            settingName: "Show Hidden Settings",
            tooltip: "Shows hidden settings.\n\nAs they are hidden there is a good chance they may not work.\nThey are labelled <color=#FFFFB2>yellow</color>.",
            configSection: MyPluginInfo.PLUGIN_NAME,
            owner: Plugin.metadata,
            defaultValue: false
        );

        Mgr_PluginSettings.AddSetting<bool, PluginSettingToggleUIItem>(enableHiddenSettingsSetting, MyPluginInfo.PLUGIN_NAME, Plugin.metadata);
        enableHiddenSettingsSetting.OnValueChanged += ShowHiddenSettingChanged;

        SceneManager.sceneLoaded += SceneLoaded;
    }

    private static void SceneLoaded(Scene scene, LoadSceneMode mode) {
        if(!Mgr_PluginSettings.SceneHasSettingsMenu)
            return;
        SettingsMenu settingsMenu = UnityEngine.Object.FindObjectsOfType<SettingsMenu>(true)[0];
        SettingsItem[] settingsItems = settingsMenu.gameObject.GetComponentsInChildren<SettingsItem>(true);

        foreach(SettingsItem settingsItem in settingsItems) {
            if(!settingsItem.gameObject.activeSelf) {
                // for some reason there is duplicate settingsItems may be related to platform specific settings (ps5)
                // only save unique settings
                if(disabledSettings.Contains(settingsItem)) {
                    continue;
                }
                disabledSettings.Add(settingsItem);
                var label = settingsItem.GetComponentInChildren<TMPro.TMP_Text>();
                label.color = new Color(1f, 1f, 0.7f, 1f);
            }
        }
        ShowHiddenSettingChanged(enableHiddenSettingsSetting);
        Plugin.Logger.LogInfo("ShowHiddenSettings initialized with " + disabledSettings.Count + " settings");
    }


    public static void ShowHiddenSettingChanged(IPluginSetting<bool> setting) {
        bool settingValue = setting.Value;
        SetHiddenSettingsState(settingValue);
    }

    public static void SetHiddenSettingsState(bool state) {
        foreach(SettingsItem settingsItem in disabledSettings) {
            if(settingsItem == null) {
                continue;
            }
            settingsItem.gameObject.SetActive(state);
        }
        Plugin.Logger.LogInfo((state ? "Enabled" : "Disabled") + $" {disabledSettings.Count} hidden settings");
    }
}