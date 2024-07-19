using UnityEngine;
using UnityEngine.Events;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using MeteorMod.Settings;
using MeteorCore.Setting;
using MeteorCore.Localiser;

namespace MeteorMod;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin {
    internal static new ManualLogSource Logger;
    internal static BepInPlugin metadata;
    internal static Harmony harmony;
    internal static UnityAction OnUpdate;
    internal static readonly string settingPageName = MyPluginInfo.PLUGIN_NAME;

    private void Awake() {
        Logger = base.Logger;
        Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");

        metadata = MetadataHelper.GetMetadata(this);

        harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
        harmony.PatchAll();

        // Register localisation
        PluginLocaliser.RegisterPlugin(metadata, projectName: "MeteorMod");

        this.InitializeSettings();
    }

    private void InitializeSettings() {
        Mgr_PluginSettings.AddPage<PluginSettingsPage>(settingPageName, PluginLocaliser.ConvertPluginToDictionaryName(Plugin.metadata), "MeteorMod Settings");
        BuiltInDebugMenuSetting.Init();
        ShowHiddenSettingsSetting.Init();
        DisableSubtitlesSetting.Init();
        SkipSplashScreenSetting.Init();
        DisableBeatmapsSetting.Init();
    }

    private void Update() {
        OnUpdate?.Invoke();
    }
}
