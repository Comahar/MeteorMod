using BepInEx;
using MeteorCore.Setting.AbstractClasses;

namespace MeteorCore.Setting;

public class PluginSettingBool : PluginSetting<bool> {
    public PluginSettingBool(
        string settingKey,
        string settingName,
        bool defaultValue,
        string tooltip,
        string configSection,
        BepInPlugin owner
    ) : base(
        settingKey: settingKey,
        settingName: settingName,
        defaultValue: defaultValue,
        tooltip: tooltip,
        configSection: configSection,
        owner: owner
    ) { }

    public override string GetValueText(bool value) {
        return Localiser.PluginLocaliser.Translate(value ? "On" : "Off", Plugin.metadata);
    }
}