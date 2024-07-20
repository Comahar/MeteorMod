using UnityEngine;
using BepInEx;
using MeteorCore.Setting.AbstractClasses;

namespace MeteorCore.Setting;

public class PluginSettingFloat : PluginSetting<float> {
    public PluginSettingFloat(
        string settingKey,
        string settingName,
        string tooltip,
        string configSection,
        BepInPlugin owner,
        float defaultValue,
        float minValue = 0,
        float maxValue = 1,
        float steps = 0.1f
    ) : base(
        settingKey: settingKey,
        settingName: settingName,
        defaultValue: defaultValue,
        tooltip: tooltip,
        configSection: configSection,
        owner: owner
    ) {
        this.minValue = minValue;
        this.maxValue = maxValue;
        this.steps = steps;
    }

    public float minValue { get; set; }
    public float maxValue { get; set; }
    public float steps { get; set; }
    public override void SetValue(float value, bool save, bool pending, bool notify) {
        value = this.Normalize(value);
        base.SetValue(value, save: save, pending: pending, notify: notify);
    }

    public float Normalize(float value) {
        value = Mathf.Clamp(value, this.minValue, this.maxValue);
        if(this.steps > 0) {
            value = Mathf.Round(value / this.steps) * this.steps;
        }
        return value;
    }

    public override string GetValueText(float value) {
        return value.ToString("F" + this.CalculateDecimals());
    }

    private int CalculateDecimals() {
        if(this.steps == 0 || this.steps > 1) {
            return 0;
        }
        // take log10 of steps, multiply by -1 and round up
        return Mathf.CeilToInt(Mathf.Log10(this.steps) * -1);
    }
}