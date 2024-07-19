using System.Collections.Generic;
using UnityEngine;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using MeteorCore.Localiser;
using MeteorCore.Setting;

namespace MeteorCoreExample;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin {
    internal static new ManualLogSource Logger;
    internal static BepInPlugin metadata;

    // Basic Toggle Setting
    PluginSettingBool toggleSetting;

    // Basic Slider Setting
    PluginSettingFloat sliderSetting;

    // Basic List Setting
    PluginSettingList<string> listSetting;

    // Custom Color Setting
    MySettingColor colorSetting;


    public void logMsgRecv(string _condition, string _stackTrace, LogType _type) {
        if(_type == LogType.Exception) {
            Logger.LogError(_condition);
            Logger.LogError(_stackTrace);
        }
    }

    private void Awake() {
        // Plugin startup logic
        Logger = base.Logger;
        Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");

        metadata = MetadataHelper.GetMetadata(this);

        // Register localisation
        PluginLocaliser.RegisterPlugin(metadata);

        // Settings
        this.toggleSetting = new PluginSettingBool(
            settingKey: "myToggleSetting",
            settingName: "Toggle",
            defaultValue: false,
            tooltip: "Example tooltip\n\nThis is a multiline tooltip",
            configSection: MyPluginInfo.PLUGIN_NAME,
            owner: metadata
        );

        this.sliderSetting = new PluginSettingFloat(
            settingKey: "mySliderSetting",
            settingName: "Slider",
            tooltip: "You can also use <s>markdown</s> <b>markup</b> in the tooltip",
            configSection: MyPluginInfo.PLUGIN_NAME,
            owner: metadata,
            defaultValue: 0f,
            minValue: 0f,
            maxValue: 10f,
            steps: 0.5f
        );

        var colorOptions = new Color[]{
            Color.red,
            Color.green,
            Color.blue,
            Color.yellow,
            Color.magenta,
            Color.cyan,
            Color.gray,
            Color.white,
            Color.black,
        };
        this.colorSetting = new MySettingColor(
            settingKey: "myColorSetting",
            settingName: "Custom Setting",
            tooltip: $"Options:\n- <color=#FF0000>Red</color>\n- <color=#00FF00>Green</color>\n- <color=#0000FF>Blue</color>\n- <color=#FFFF00>Yellow</color>\n- <color=#FF00FF>Magenta</color>\n- <color=#00FFFF>Cyan</color>\n- <color=#C0C0C0>Gray</color>\n- <color=#FFFFFF>White</color>\n- <color=#000000>Black</color> <- Black",
            configSection: MyPluginInfo.PLUGIN_NAME,
            owner: metadata,
            defaultValue: Color.red,
            defaultValueText: "<color=#FF0000>Red</color>",
            options: colorOptions
        );

        // List Setting
        var listOptions = new List<PluginSettingListOption<string>>{
            new PluginSettingListOption<string>(id: 1, text: "<color=#e4eec2>Fang</color>", value: "fang"),
            new PluginSettingListOption<string>(id: 2, text: "<color=#ccbdee>Trish</color>", value: "trish"),
            new PluginSettingListOption<string>(id: 3, text: "<color=#f1b5b3>Reed</color>", value: "reed"),
            new PluginSettingListOption<string>(id: 4, text: "<color=#ffb576>Naser</color>", value: "naser"),
            new PluginSettingListOption<string>(id: 5, text: "<color=#ffd1b8>Naomi</color>", value: "naomi"),
            new PluginSettingListOption<string>(id: 6, text: "<color=#ff4f83>Rosa</color>", value: "rosa"),
            new PluginSettingListOption<string>(id: 7, text: "<color=#bfa2fd>Sage</color>", value: "sage"),
            new PluginSettingListOption<string>(id: 8, text: "<color=#cfe78c>Stella</color>", value: "stella"),
            new PluginSettingListOption<string>(id: 9, text: "<color=#ff641e>Mango</color>", value: "mango"),
        };

        this.listSetting = new PluginSettingList<string>(
            settingKey: "myListSetting",
            settingName: "List",
            tooltip: "Tooltip",
            configSection: MyPluginInfo.PLUGIN_NAME,
            owner: metadata,
            defaultValue: 1,
            options: listOptions
        );

        // Register settings
        Mgr_PluginSettings.AddPage<PluginSettingsPage>("Examples", PluginLocaliser.ConvertPluginToDictionaryName(metadata), "ExampleSettingPage");
        Mgr_PluginSettings.AddSetting<bool, PluginSettingToggleUIItem>(this.toggleSetting, "Examples", metadata);
        Mgr_PluginSettings.AddSetting<float, PluginSettingSliderUIItem>(this.sliderSetting, "Examples", metadata);
        Mgr_PluginSettings.AddSetting<Color, MySettingColorUIItem>(this.colorSetting, "Examples", metadata);
        // while using PluginSettingListUIItem must have a concrete type and setting type must be int
        Mgr_PluginSettings.AddSetting<int, PluginStringListSettingUIItem>(this.listSetting, "Examples", metadata);
    }
}