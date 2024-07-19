using System.Collections.Generic;
using UnityEngine;
using TMPro;
using MeteorCore.Setting.Interfaces;
using MeteorCore.Setting.Interfaces.Internal;

namespace MeteorCore.Setting.AbstractClasses;

/// <summary>
/// A UI item for a plugin setting
/// </summary>
/// <typeparam name="T"> IPluginSetting\<D\> </typeparam> 
/// <typeparam name="D"> The type of the setting (bool, int, etc.) </typeparam>
public abstract class PluginSettingUIItem<T, D> : SelectableElement, IPluginSettingUIItem<T, D> where T : IPluginSetting<D> {
    //// IPluginSettingUIItem
    public T setting { get; protected set; }

    public IPluginSetting settingInterface { get => (IPluginSetting)this.setting; }

    public GameObject isDirtyIndicator { get; protected set; }

    public TextMeshProUGUI label { get; protected set; }

    public List<LabelledRewiredAction> actionsToAddToControls { get; protected set; }

    public Rewired.Player Player { get { return Scr_InputMaster.Instance.Player; } }
    public AudioClip settingChangedAudio;

    public virtual GameObject Initialize(Transform parent, IPluginSetting setting) {
        Plugin.Logger.LogWarning("Using Initialize from base PluginSettingUIItem is not recommended");
        var settingConverted = (T)(setting as IPluginSetting<D>);
        if(settingConverted == null) {
            Plugin.Logger.LogError($"Could not convert setting to IPluginSetting<D>");
            Plugin.Logger.LogError($"Setting type: {setting.GetType()}");
            Plugin.Logger.LogError($"With key {setting.settingKey}");
            return null;
        }
        this.setting = settingConverted;
        return null;
    }


    public virtual void SetSetting(T setting) {
        this.setting = setting;
    }

    public virtual void SetSettingInternal(IPluginSetting setting) {
        this.setting = (T)setting;
    }

    protected override void Awake() {
        base.Awake();
        // this is initilized as false
        // could not find where it is set to true for default items, looked in the inspector and decompiled code 
        this._selectOnPointerEnter = true;
    }

    public override void OnEnable() {
        base.OnEnable();
        this.Refresh();
        if(this.setting != null) {
            this.setting.OnAppliedPendingValue += this.OnApplied;
            this.setting.OnRevertedPendingValue += this.OnRevert;
        }
        LocalisationManager.Instance.LanguageChanged += this.OnLanguagedChanged;
    }

    public override void OnDisable() {
        base.OnDisable();
        if(this.setting != null) {
            this.setting.OnAppliedPendingValue -= this.OnApplied;
            this.setting.OnRevertedPendingValue -= this.OnRevert;
        }
        LocalisationManager.Instance.LanguageChanged -= this.OnLanguagedChanged;
    }

    protected override void OnDestroy() {
        if(this.setting != null) {
            this.setting.OnAppliedPendingValue -= this.OnApplied;
            this.setting.OnRevertedPendingValue -= this.OnRevert;
        }
        LocalisationManager.Instance.LanguageChanged -= this.OnLanguagedChanged;
    }

    private void OnApplied(IPluginSetting<D> setting) {
        this.Refresh();
    }

    private void OnRevert(IPluginSetting<D> setting) {
        this.Refresh();
    }

    protected virtual void Refresh() {
        this.RefreshDirtyIndicator();
        this.RefreshNameLabel();
    }

    protected virtual void RefreshDirtyIndicator() {
        this.isDirtyIndicator?.SetActive(this.IsDirty());
    }

    protected virtual void RefreshNameLabel() {
        if(this.label != null && this.setting != null) {
            this.label.text = this.setting.GetSettingName();
        }
    }

    protected virtual bool IsDirty() {
        return this.setting?.isDirty ?? false;
    }

    public abstract void SetValue(D value, bool save, bool pending, bool notify);
    public abstract void RevertToDefault();
    protected abstract void OnLanguagedChanged();
}