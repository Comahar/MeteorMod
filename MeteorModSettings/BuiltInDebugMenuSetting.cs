using System;
using UnityEngine;
using MeteorMod.ModSettings;
using MeteorMod.ModSettings.ModSettingItems;
using UnityEngine.SceneManagement;

namespace MeteorMod.MeteorModSettings {
    public static class BuiltInDebugMenuSetting {
        public static ModBoolSetting enableDebugSetting = new ModBoolSetting(
            "EnableDebugMenu",
            "Enable Debug Menu",
            "Enables debugTools and developers debug menu\n\nAccess the menu with CTRL+SHIFT+D",
            "MISC",
            false
        );

        public static DebugMenuToggle debugMenuToggle;
        public static Mgr_DebugTools debugTools;

        public static void Init() {
            Mgr_ModSettings.AddSetting<ModBoolSetting, bool>(Plugin.metadata, enableDebugSetting);
            enableDebugSetting.onValueChanged += DebugSettingChanged;

            //SceneManager.sceneLoaded += SceneLoaded;
        }

        static bool firstSceneLoad = true;
        public static void SceneLoaded(Scene scene, LoadSceneMode mode) {
            Plugin.LOG.LogWarning($"BuiltInDebugMenuSetting SceneLoaded");
            // run only once on the title scene
            // early return if not title scene
            if(!SceneHelper.IsTitleScene || !firstSceneLoad)
                return;
            firstSceneLoad = false;

            // get debug gameobject, early return if not found
            string gameObjectPath = "MANAGER_MASTER/Debug";
            GameObject debugToolsGameObject = GameObject.Find(gameObjectPath);
            if(debugToolsGameObject == null) {
                Plugin.LOG.LogError("Could not get debug object," + gameObjectPath + "GameObject not found");
                return;
            }
            debugTools = debugToolsGameObject.GetComponent<Mgr_DebugTools>();
            if(debugTools == null) {
                Plugin.LOG.LogError("Could not get debug, Mgr_DebugTools component not in GameObject" + gameObjectPath);
                return;
            }

            // get debug button gameobject
            debugMenuToggle = UnityEngine.Object.FindObjectOfType<DebugMenuToggle>(true);
            if(debugMenuToggle == null) {
                Plugin.LOG.LogError("Could not get DebugMenuToggle");
                return;
            }

            // set debug menu state to setting value
            SetDebugMenuState(enableDebugSetting.value);
            Plugin.LOG.LogWarning($"BuiltInDebugMenuSetting SceneLoaded finish");
        }

        public static void DebugSettingChanged() {
            bool settingValue = enableDebugSetting.value;
            SetDebugMenuState(settingValue);
        }

        public static void SetDebugMenuState(bool state) {
            if(debugTools == null || debugMenuToggle == null) {
                Plugin.LOG.LogWarning("DebugTools errored at Init, cannot apply debug setting");
                return;
            }
            if(IsMenuActive()) {
                debugMenuToggle.ToggleAllDebugBoxes();
            }
            if(state) {
                debugTools.DisableAllDebug = false;
                Plugin.LOG.LogInfo("Enabled GVH debug");

                debugMenuToggle.gameObject.SetActive(true);
                Plugin.LOG.LogInfo("Enabled GVH debug button");
            } else {
                debugTools.DisableAllDebug = true;
                Plugin.LOG.LogInfo("Disabled GVH debug");

                debugMenuToggle.gameObject.SetActive(false);
                Plugin.LOG.LogInfo("Disabled GVH debug button");
            }
        }

        public static bool IsMenuActive() {
            if(debugMenuToggle == null) {
                Plugin.LOG.LogWarning("DebugMenuToggle is null");
                return false;
            }
            if(debugMenuToggle.debugGameobjects.Length == 0) {
                Plugin.LOG.LogWarning("debugGameobjects is empty");
                return false;
            }
            return debugMenuToggle.debugGameobjects[0].activeInHierarchy;
        }
    }
}
