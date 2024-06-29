using UnityEngine;
using MelonLoader;
using MeteorMod.ModSettings;
using MeteorMod.Settings;

namespace MeteorMod {
    public class MeteorMod : MelonMod {
        public override void OnInitializeMelon() {
            UIBuilder.Init();
            BuiltInDebugMenuSetting.Init();
            ShowHiddenSettingsSetting.Init();
            DisableSubtitlesSetting.Init();
            SkipSplashScreenSetting.Init();
            DisableBeatmapsSetting.Init();
        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName) {
            isPerformenceScene = null;
            isMinigameScene = null;

            UIBuilder.Instance.OnSceneWasLoaded(buildIndex, sceneName);
            Mgr_ModSettings.Instance.OnSceneWasLoaded(buildIndex, sceneName);

            ShowHiddenSettingsSetting.OnSceneWasLoaded(buildIndex, sceneName);
            BuiltInDebugMenuSetting.OnSceneWasLoaded(buildIndex, sceneName);
            DisableBeatmapsSetting.OnSceneWasLoaded(buildIndex, sceneName);
        }

        public override void OnSceneWasInitialized(int buildIndex, string sceneName) {
            if(sceneName == "Splash") {
                return;
            }
            DisableSubtitlesSetting.OnSceneWasInitialized(buildIndex, sceneName);
        }

        public override void OnUpdate() {
            DisableBeatmapsSetting.OnUpdate();
        }

        private static bool? isPerformenceScene;
        public static bool IsPerformenceScene {
            get {
                if(isPerformenceScene == null) {
                    GameObject gameObject = GameObject.Find("GameplayUI Root");
                    isPerformenceScene = gameObject != null;
                }
                //MelonLogger.Warning("IsPerformenceScene: " + isPerformenceScene.Value);
                return isPerformenceScene.Value;
            }
        }

        private static bool? isMinigameScene;
        public static bool IsMinigameScene {
            get {
                if(isMinigameScene == null) {
                    GameObject gameObject = GameObject.Find("MG_Basics/Controller");
                    isMinigameScene = gameObject != null;
                }
                //MelonLogger.Warning("IsMinigameScene: " + isMinigameScene.Value);
                return isMinigameScene.Value;
            }
        }
    }
}
