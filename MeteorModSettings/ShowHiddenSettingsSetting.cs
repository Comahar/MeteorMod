using System;
using System.Collections.Generic;
using UnityEngine;
using MeteorMod.ModSettings;
using MeteorMod.ModSettings.ModSettingItems;
using UnityEngine.SceneManagement;

namespace MeteorMod.MeteorModSettings {
    public static class ShowHiddenSettingsSetting {
        public static ModBoolSetting enableHiddenSettings = new ModBoolSetting(
            "EnableHiddenSettings",
            "Enable Hidden Settings",
            "Enables hidden setting created by the developer\n\nAs they are hidden there is a good chance they may not work.\nThey are labelled yellow.",
            "MISC",
            false
        );

        private static List<SettingsItem> disabledSettings = new List<SettingsItem>();

        public static void Init() {
            Mgr_ModSettings.AddSetting<ModBoolSetting, bool>(Plugin.metadata, enableHiddenSettings);
            enableHiddenSettings.onValueChanged += ShowHiddenSettingChanged;

            //SceneManager.sceneLoaded += SceneLoaded;
        }

        private static void SceneLoaded(Scene scene, LoadSceneMode mode) {
            Plugin.LOG.LogWarning($"ShowHiddenSettingsSetting SceneLoaded");
            if(!SceneHelper.IsTitleScene)
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

            ShowHiddenSettingChanged();

            Plugin.LOG.LogInfo("ShowHiddenSettings initialized with " + disabledSettings.Count + " settings");
            Plugin.LOG.LogWarning($"ShowHiddenSettingsSetting SceneLoaded finish");
        }


        public static void ShowHiddenSettingChanged() {
            bool settingValue = enableHiddenSettings.value;
            SetHiddenSettingsState(settingValue);
        }

        public static void SetHiddenSettingsState(bool state) {
            foreach(SettingsItem settingsItem in disabledSettings) {
                settingsItem.gameObject.SetActive(state);
            }
            Plugin.LOG.LogInfo("Set hidden " + disabledSettings.Count + " settings to " + state);
        }
    }
}
