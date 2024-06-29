using MelonLoader;
using System;
using UnityEngine;
using MeteorMod.ModSettings;
using MeteorMod.ModSettings.ModSettingItems;

namespace MeteorMod.Settings {
    public static class BuiltInDebugMenuSetting {
        public static ModBoolSetting enableDebugSetting = new ModBoolSetting(
            "EnableDebugMenu",
            "Enable Debug Menu",
            "Enables debugTools and developers debug menu\n\nAccess the menu with CTRL+SHIFT+D",
            "MISC",
            false
        );

        public static DebugMenuToggle? debugMenuToggle;
        public static Mgr_DebugTools? debugTools;

        public static void Init() {
            Mgr_ModSettings.AddSetting<ModBoolSetting, bool>("MeteorMod", enableDebugSetting);
            enableDebugSetting.onValueChanged = (Setting.ValueChangeCallback)Delegate.Combine(
                enableDebugSetting.onValueChanged,
                new Setting.ValueChangeCallback(DebugSettingChanged)
            );
        }

        static bool firstSceneLoad = true;
        public static void OnSceneWasLoaded(int buildIndex, string sceneName) {
            if(sceneName == "Splash" || !firstSceneLoad)
                return;
            firstSceneLoad = false;
            // get debug gameobject
            string gameObjectPath = "MANAGER_MASTER/Debug";
            GameObject debugToolsGameObject = GameObject.Find(gameObjectPath);
            if(debugToolsGameObject == null) {
                MelonLogger.Error("Could not get debug object," + gameObjectPath + "GameObject not found");
            } else {
                debugTools = debugToolsGameObject.GetComponent<Mgr_DebugTools>();
                if(debugTools == null) {
                    MelonLogger.Error("Could not get debug, Mgr_DebugTools component not in GameObject" + gameObjectPath);
                }
            }

            // get debug button gameobject
            debugMenuToggle = UnityEngine.Object.FindObjectOfType<DebugMenuToggle>(true);
            if(debugMenuToggle == null) {
                MelonLogger.Error("Could not get DebugMenuToggle");
            }

            // set debug menu state
            SetDebugMenuState(enableDebugSetting.value);
        }

        public static void DebugSettingChanged() {
            bool settingValue = enableDebugSetting.value;
            SetDebugMenuState(settingValue);
        }

        public static void SetDebugMenuState(bool state) {
            if(debugTools == null || debugMenuToggle == null) {
                MelonLogger.Warning("DebugTools errored at Init, cannot apply debug setting");
                return;
            }
            if(IsMenuActive()) {
                debugMenuToggle.ToggleAllDebugBoxes();
            }
            if(state) {
                debugTools.DisableAllDebug = false;
                MelonLogger.Msg("Enabled GVH debug");

                debugMenuToggle.gameObject.SetActive(true);
                MelonLogger.Msg("Enabled GVH debug button");
            } else {
                debugTools.DisableAllDebug = true;
                MelonLogger.Msg("Disabled GVH debug");

                debugMenuToggle.gameObject.SetActive(false);
                MelonLogger.Msg("Disabled GVH debug button");

            }
        }

        public static bool IsMenuActive() {
            if(debugMenuToggle == null) {
                MelonLogger.Warning("DebugMenuToggle is null");
                return false;
            }
            if(debugMenuToggle.debugGameobjects.Length == 0) {
                MelonLogger.Warning("debugGameobjects is empty");
                return false;
            }
            return debugMenuToggle.debugGameobjects[0].activeInHierarchy;
        }
    }
}
