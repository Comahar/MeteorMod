using System;
using System.Collections.Generic;
using UnityEngine;
using MeteorMod.ModSettings;
using MeteorMod.ModSettings.ModSettingItems;
using UnityEngine.SceneManagement;

namespace MeteorMod.MeteorModSettings {
    public static class DisableBeatmapsSetting {
        public static ModBoolSetting disableBeatmapsSetting = new ModBoolSetting(
            "DisableBeatmaps",
            "Disable Beatmaps",
            "Disables rhythm game segments' interactivity and UI.\nToggle in a performance with CTRL+SHIFT+F",
            "MISC",
            false
        );

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
            Mgr_ModSettings.AddSetting<ModBoolSetting, bool>(Plugin.metadata, disableBeatmapsSetting);
            disableBeatmapsSetting.onValueChanged += DisableBeatmapsSettingChanged;

            //SceneManager.sceneLoaded += SceneLoaded;
            Plugin.OnUpdate += OnUpdate;
        }

        public static void SceneLoaded(Scene scene, LoadSceneMode mode) {
            Plugin.LOG.LogWarning($"DisableBeatmapsSetting SceneLoaded");
            if (!SceneHelper.IsPerformenceScene)
                return;
            GameplayUI = GameObject.Find("GameplayUI Root");
            if(GameplayUI == null) {
                Plugin.LOG.LogWarning("Could not find GameplayUI Root");
                return;
            }
            SetBeatmapState(!disableBeatmapsSetting.value);
            Plugin.LOG.LogWarning($"DisableBeatmapsSetting SceneLoaded finish");
        }

        public static void OnUpdate() {
            if(GameplayUI == null) {
                return;
            }
            if(IsPressingCombo) {
                if(!comboKeyDown) {
                    comboKeyDown = true;
                    disableBeatmapsSetting.SetSettingValue(!disableBeatmapsSetting.value);
                }
            } else {
                comboKeyDown = false;
            }
        }

        public static void DisableBeatmapsSettingChanged() {
            SetBeatmapState(!disableBeatmapsSetting.value);
        }

        public static void SetBeatmapState(bool state) {
            if(GameplayUI == null) {
                return;
            }
            GameplayUI.SetActive(state);
            Plugin.LOG.LogInfo($"Set beatmap GameplayUI to {state}");
        }

    }
}
