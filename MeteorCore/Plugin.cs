using UnityEngine;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using MeteorCore.Setting;

namespace MeteorCore {
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin {
        internal static new ManualLogSource Logger;
        internal static Transform PluginManager;
        internal static Harmony harmony;
        internal static BepInPlugin metadata;

        private void Awake() {
            // Enable unity logging, this is disabled by default for some reason
            Debug.unityLogger.logEnabled = true;
            Debug.unityLogger.filterLogType = LogType.Warning;

            metadata = MetadataHelper.GetMetadata(this);

            Logger = base.Logger;
            Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");

            harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
            harmony.PatchAll();

            // Register localisation
            Localiser.PluginLocaliser.RegisterPlugin(metadata);

            // Create plugin manager
            var managerMaster = GameObject.Find("MANAGER_MASTER");
            PluginManager = new GameObject("MeteorCore").transform;
            PluginManager.SetParent(managerMaster.transform);

            // Plugin Hooks
            SceneHelper.Init();
            Mgr_PluginSettings.Create();
        }
    }
}
