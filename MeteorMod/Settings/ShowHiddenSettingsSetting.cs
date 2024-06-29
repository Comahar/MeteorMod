using System;
using System.Collections.Generic;
using UnityEngine;
using MeteorMod.ModSettings;
using MeteorMod.ModSettings.ModSettingItems;

namespace MeteorMod.Settings {
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
            Mgr_ModSettings.AddSetting<ModBoolSetting, bool>("GVHHelper", enableHiddenSettings);
            enableHiddenSettings.onValueChanged = (Setting.ValueChangeCallback)Delegate.Combine(
                enableHiddenSettings.onValueChanged,
                new Setting.ValueChangeCallback(ShowHiddenSettingChanged)
            );
        }

        public static void OnSceneWasLoaded(int buildIndex, string sceneName) {
            if(sceneName == "Title") {
                SettingsMenu settingsMenu = UnityEngine.Object.FindObjectsOfType<SettingsMenu>(true)[0];
                SettingsItem[] settingsItems = settingsMenu.gameObject.GetComponentsInChildren<SettingsItem>(true);
                // only save unique settings

                foreach(SettingsItem settingsItem in settingsItems) {
                    if(!settingsItem.gameObject.activeSelf) {
                        if(disabledSettings.Contains(settingsItem)) {
                            continue;
                        }
                        disabledSettings.Add(settingsItem);
                        var label = settingsItem.GetComponentInChildren<TMPro.TMP_Text>();
                        label.color = new Color(1f, 1f, 0.7f, 1f);
                    }
                }

                ShowHiddenSettingChanged();

                MelonLoader.MelonLogger.Msg("ShowHiddenSettings initialized with " + disabledSettings.Count + " settings");
            }
        }



        public static void ShowHiddenSettingChanged() {
            bool settingValue = enableHiddenSettings.value;
            SetHiddenSettingsState(settingValue);
        }

        public static void SetHiddenSettingsState(bool state) {
            foreach(SettingsItem settingsItem in disabledSettings) {
                if(settingsItem != null) {
                    settingsItem.gameObject.SetActive(state);
                }
            }
            MelonLoader.MelonLogger.Msg("Set hidden " + disabledSettings.Count + " settings to " + state);
        }
    }
}
