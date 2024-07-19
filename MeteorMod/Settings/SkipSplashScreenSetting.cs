using System.Collections;
using UnityEngine.SceneManagement;
using HarmonyLib;
using MeteorCore.Setting;

namespace MeteorMod.Settings;
public static class SkipSplashScreenSetting {
    public static PluginSettingBool skipSplashScreenSetting;

    public static void Init() {
        skipSplashScreenSetting = new PluginSettingBool(
            settingKey: "SkipSplashScreen",
            settingName: "Skip Splash Screen",
            tooltip: "Skips the start splash screen",
            configSection: MyPluginInfo.PLUGIN_NAME,
            owner: Plugin.metadata,
            defaultValue: false
        );

        Mgr_PluginSettings.AddSetting<bool, PluginSettingToggleUIItem>(skipSplashScreenSetting, Plugin.settingPageName, Plugin.metadata);
    }

    [HarmonyPatch(typeof(SplashScreen), nameof(SplashScreen.Start))]
    public class SplashScreen_Start_Patch_Skip {
        static bool Prefix(SplashScreen __instance) {
            if(skipSplashScreenSetting == null) {
                Plugin.Logger.LogError("SkipSplashScreenSetting is null");
                return true;
            }
            bool skipSplashScreen = skipSplashScreenSetting.Value;
            if(!skipSplashScreen) {
                return true;
            }
            Plugin.Logger.LogInfo("Skipping SplashScreen");
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
