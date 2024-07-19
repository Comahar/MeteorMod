using System;
using BepInEx;
using MeteorCore.Setting.Interfaces;
using MeteorCore.Localiser;

namespace MeteorCore.Setting.AbstractClasses;

/// <summary>
/// A plugin setting
/// </summary>
/// <typeparam name="T"> The type of the setting (bool, int, etc.) </typeparam>
public abstract class PluginSetting<T> : IPluginSetting<T> {
    public virtual object BoxedValue { get => this.Value; set => this.Value = (T)value; }
    public virtual T Value { get; private set; }
    public virtual T PendingValue { get; protected set; }
    public virtual T DefaultValue { get; protected set; }
    public virtual string tooltip { get; protected set; }
    public virtual string configSection { get; protected set; }
    public virtual BepInPlugin owner { get; protected set; }
    public virtual Type type => typeof(T);
    public virtual string settingKey { get; }
    public virtual string settingName { get; }
    public virtual bool isDirty { get; protected set; }

    /// <summary>
    /// Invoked when the pending value is applied
    /// </summary>
    public event Action<IPluginSetting<T>> OnAppliedPendingValue;

    /// <summary>
    /// Invoked when the pending value is reverted
    /// </summary>
    public event Action<IPluginSetting<T>> OnRevertedPendingValue;

    /// <summary>
    /// Invoked when the pending value is changed
    /// </summary>
    public event Action<IPluginSetting<T>> OnPendingValueChanged;

    /// <summary>
    /// Invoked when the value is set (when SetValue is called with pending: false)
    /// </summary>
    public event Action<IPluginSetting<T>> OnValueChanged;

    public PluginSetting(
        string settingKey,
        string settingName,
        T defaultValue,
        string tooltip,
        string configSection,
        BepInPlugin owner
    ) {
        this.settingKey = settingKey;
        this.settingName = settingName;
        this.DefaultValue = defaultValue;
        this.tooltip = tooltip;
        this.configSection = configSection;
        this.owner = owner;
        this.isDirty = false;
    }

    public virtual void SetValue(T value, bool save, bool pending, bool notify) {
        if(pending) {
            this.PendingValue = value;
            this.isDirty = true;
            OnPendingValueChanged?.Invoke(this);
        } else {
            this.Value = value;
            this.PendingValue = value;
            this.isDirty = false;
            OnValueChanged?.Invoke(this);
        }
        if(save) {
            Mgr_PluginSettings.Instance.SaveSettings();
        }
    }

    public virtual void SetValue(object value, bool save, bool pending, bool notify) {
        if(value is not T) {
            Plugin.Logger.LogError($"Setting {this.settingKey} could not be set to {value} because it is not of type {typeof(T)}");
            return;
        }
        this.SetValue((T)value, save, pending, notify);
    }

    public virtual void ApplyPendingValue() {
        this.SetValue(this.PendingValue, save: false, pending: false, notify: true);
        OnAppliedPendingValue?.Invoke(this);
    }

    public virtual void RevertPendingValue() {
        this.PendingValue = this.Value;
        this.isDirty = false;
        OnRevertedPendingValue?.Invoke(this);
    }


    public virtual string GetValueText(T value) {
        return PluginLocaliser.Translate(value.ToString(), this.owner);
    }

    public virtual string DefaultValueText() {
        return this.GetValueText(this.DefaultValue);
    }

    public virtual string GetCurrentValueText() {
        return this.GetValueText(this.Value);
    }

    public virtual string GetPendingValueText() {
        return this.GetValueText(this.PendingValue);
    }

    public virtual string GetSettingName() {
        return PluginLocaliser.Translate(this.settingName, this.owner);
    }

    public virtual string GetTooltip() {
        return PluginLocaliser.Translate(this.tooltip, this.owner);
    }
}