using System;
using System.Collections.Generic;
using MelonLoader;
using UnityEngine;
using MeteorMod.ModSettings;
using MeteorMod.ModSettings.ModSettingItems;

namespace MeteorMod.Settings {
    public static class DisableBeatmapsSetting {
        public static ModBoolSetting disableBeatmapsSetting = new ModBoolSetting(
            "DisableBeatmaps",
            "Disable Beatmaps",
            "Disables rhythm game segments' interactivity and UI.\nToggle in a performance with CTRL+SHIFT+F",
            "MISC",
            false
        );

        public static GameObject? GameplayUI;
        public static List<KeyCode> KeyCombos = new List<KeyCode> { KeyCode.LeftControl, KeyCode.LeftShift, KeyCode.F };
        private static bool comboKeyDown = false;

        public static bool comboPressing {
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
            Mgr_ModSettings.AddSetting<ModBoolSetting, bool>("MeteorMod", disableBeatmapsSetting);
            disableBeatmapsSetting.onValueChanged = (Setting.ValueChangeCallback)Delegate.Combine(
                disableBeatmapsSetting.onValueChanged,
                new Setting.ValueChangeCallback(DisableBeatmapsSettingChanged)
            );
        }

        public static void OnSceneWasLoaded(int buildIndex, string sceneName) {
            GameplayUI = GameObject.Find("GameplayUI Root");
            SetBeatmapState(!disableBeatmapsSetting.value);
        }

        public static void DisableBeatmapsSettingChanged() {
            SetBeatmapState(!disableBeatmapsSetting.value);
        }

        public static void SetBeatmapState(bool state) {
            if(GameplayUI == null) {
                return;
            }
            GameplayUI.SetActive(state);
            MelonLogger.Msg($"Set beatmap GameplayUI to {state}");
        }

        public static void OnUpdate() {
            if(GameplayUI == null) {
                return;
            }
            if(comboPressing) {
                if(!comboKeyDown) {
                    comboKeyDown = true;
                    disableBeatmapsSetting.SetSettingValue(!disableBeatmapsSetting.value);
                }
            } else {
                comboKeyDown = false;
            }
        }
    }
}
