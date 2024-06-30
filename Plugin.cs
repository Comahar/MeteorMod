using System.Collections.Generic;
using UnityEngine;
using BepInEx;
using MeteorMod.ModSettings;
using MeteorMod.MeteorModSettings;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using HarmonyLib;

namespace MeteorMod {
    public static class MeteorModInfo {
        public const string PLUGIN_GUID = "MeteorMod";
        public const string PLUGIN_NAME = "Meteor Mod";
        public const string PLUGIN_VERSION = "0.2.0";
    }

    [BepInPlugin(MeteorModInfo.PLUGIN_GUID, MeteorModInfo.PLUGIN_NAME, MeteorModInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin {
        //internal static new BepInEx.Logging.ManualLogSource Logger;
        internal static BepInEx.Logging.ManualLogSource LOG;
        internal static BepInPlugin metadata;
        private static Harmony _harmony;

        internal static UnityAction OnUpdate;
        //public static List<Koop.Notify> SceneReadyCallbacks { get; } = [];
        //public static UnityAction<Scene, LoadSceneMode> sceneLoaded;

        private void Awake() {
            // Plugin startup logic
            Logger.LogInfo($"Plugin {MeteorModInfo.PLUGIN_GUID} is loaded!");
            metadata = MetadataHelper.GetMetadata(this);

            LOG = BepInEx.Logging.Logger.CreateLogSource("MeteorMod");
            //BepInEx.Logging.Logger.Sources.Add(LOG);

            _harmony = new Harmony(MeteorModInfo.PLUGIN_GUID);
            _harmony.PatchAll();

            LOG.LogWarning("MeteorMod is loaded!");

            // Early init so the other classes can use the SceneHelper
            SceneHelper.Init();

            var managerMaster = GameObject.Find("MANAGER_MASTER");
            var modMaster = new GameObject("MOD_MeteorMod");
            modMaster.transform.SetParent(managerMaster.transform);

            var UIBuilder = new GameObject("ModSettingsUIBuilder");
            UIBuilder.AddComponent<ModSettingsUIBuilder>().enabled = true;
            UIBuilder.transform.SetParent(modMaster.transform);

            var Mgr_ModSettings = new GameObject("Mgr_ModSettings");
            Mgr_ModSettings.AddComponent<Mgr_ModSettings>().enabled = true;
            Mgr_ModSettings.transform.SetParent(modMaster.transform);

            //InitializeSettings();

        }

        private void Update() {
            return;
            OnUpdate();
        }

        private void InitializeSettings() {
            BuiltInDebugMenuSetting.Init();
            ShowHiddenSettingsSetting.Init();
            DisableSubtitlesSetting.Init();
            SkipSplashScreenSetting.Init();
            DisableBeatmapsSetting.Init();
        }

    }

    //[HarmonyPatch(typeof(SettingsMenu), nameof(SettingsMenu.Start))]
    [HarmonyPatch(typeof(TitleScreen), nameof(TitleScreen.Start))]
    public class AAAATest {
        /*static System.Reflection.MethodBase TargetMethod() 
        {
            return AccessTools.Method(typeof(SettingsMenu), "Start");
        }*/

        public static void Postfix() {
            Plugin.LOG.LogWarning("TitleScreen.Start");
        }
    }
    /*[HarmonyPatch(typeof(Mgr_LevelFlow), nameof(Mgr_LevelFlow.ReadyToRevealNewScene))]
    public class RevealNewScenePatch
    {
        static void Postfix()
        {
            Plugin.LOG.LogWarning("RevealNewScenePatch");
            foreach(Koop.Notify callback in Plugin.SceneReadyCallbacks){
                callback();
            }
        }
    }*/
}
