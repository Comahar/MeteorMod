using System.Collections;
using UnityEngine.SceneManagement;
using HarmonyLib;
using MeteorMod.ModSettings;
using MeteorMod.ModSettings.ModSettingItems;

namespace MeteorMod.MeteorModSettings {
    public static class SkipSplashScreenSetting {
        public static ModBoolSetting skipSplashScreenSetting = new ModBoolSetting(
            "SkipSplashScreen",
            "Skip Splash Screen",
            "Skips the start splash screen",
            "MISC",
            false
        );

        public static void Init() {
            Mgr_ModSettings.AddSetting<ModBoolSetting, bool>(Plugin.metadata, skipSplashScreenSetting);
        }
    }

    [HarmonyPatch(typeof(SplashScreen), nameof(SplashScreen.Start))]
    public class SplashScreen_Start_Patch_Skip {
        static bool Prefix(SplashScreen __instance) {
            bool skipSplashScreen = true;
            //bool skipSplashScreen = SkipSplashScreenSetting.skipSplashScreenSetting.value;

            if(!skipSplashScreen) {
                return true;
            }
            Plugin.LOG.LogInfo("Skipping SplashScreen");
            var result = Skip(__instance);
            __instance.StartCoroutine(result);
            return false;
        }

        static IEnumerator Skip(SplashScreen instance) {
            var loadSceneOp = SceneManager.LoadSceneAsync("Title", LoadSceneMode.Additive);
            while(!loadSceneOp.isDone) {
                yield return null;
            }
            SceneManager.UnloadSceneAsync(instance.gameObject.scene);
        }
    }
}
