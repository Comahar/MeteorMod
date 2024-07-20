using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Koop;
using MeteorCore.Localiser;
using MeteorCore.Setting.AbstractClasses;
using MeteorCore.Setting.Interfaces.Internal;
using MeteorCore.Utils;

namespace MeteorCore.Setting;

public class PluginSettingSliderUIItem : PluginSettingUIItem<PluginSettingFloat, float> {
    protected TextMeshProUGUI valueLabel;
    protected Slider slider;
    protected RewiredAxisAction uiHorizontal;
    protected float holdTime;
    protected int lastInput;


    public override GameObject Initialize(Transform parent, IPluginSetting setting) {
        var settingConverted = setting as PluginSettingFloat;
        if(settingConverted == null) {
            Plugin.Logger.LogError($"Could not convert setting to PluginSettingFloat with key {setting?.settingKey}");
            return null;
        }

        GameObject sliderGameObject = Instantiate(SettingsUIPrefabs.SliderPrefab, parent);
        sliderGameObject.name = setting.settingKey;
        SettingsItemSlider settingsItemSlider = sliderGameObject.GetComponent<SettingsItemSlider>();
        PluginSettingSliderUIItem thisComponent = sliderGameObject.AddComponent<PluginSettingSliderUIItem>();

        MeteorUtils.CopyComponentValues<SelectableElement>(settingsItemSlider, thisComponent, copyPrivate: true);

        // PluginSettingUIItem members
        thisComponent.setting = settingConverted;
        thisComponent.label = settingsItemSlider.label;
        thisComponent.isDirtyIndicator = settingsItemSlider.isDirtyIndicator;
        thisComponent.actionsToAddToControls = settingsItemSlider.actionsToAddToControls;
        thisComponent.settingChangedAudio = settingsItemSlider.settingChangedAudio;

        // PluginSettingSliderUIItem fields
        thisComponent.valueLabel = settingsItemSlider.textLabel;
        thisComponent.slider = settingsItemSlider.slider;
        thisComponent.uiHorizontal = settingsItemSlider.uiHorizontal;
        thisComponent.holdTime = settingsItemSlider.holdTime;
        thisComponent.lastInput = settingsItemSlider.lastInput;

        // Set slider values
        Slider slider = thisComponent.GetComponentInChildren<Slider>();
        slider.onValueChanged.AddListener(thisComponent.OnSliderValueChanged);
        slider.maxValue = settingConverted.maxValue;
        slider.minValue = settingConverted.minValue;
        slider.wholeNumbers = false;

        Destroy(settingsItemSlider);
        settingsItemSlider.enabled = false;
        Destroy(this.gameObject);

        StaticStringLocaliser localiser = thisComponent.transform.GetChild(0).GetComponent<StaticStringLocaliser>();
        if(localiser == null) {
            Plugin.Logger.LogError("PluginSettingSliderUIItem Could not find localiser");
            return null;
        }
        localiser.BaseText = setting.settingName;
        localiser.Dictionary = PluginLocaliser.ConvertPluginToDictionaryName(setting.owner);

        sliderGameObject.SetActive(true);
        thisComponent.slider.value = settingConverted.Value;
        thisComponent.Refresh();
        return sliderGameObject;
    }

    protected virtual void Update() {
        if(!base.isSelected || !base.IsInteractable()) {
            return;
        }
        bool flag = base.Player.GetButtonDown(this.uiHorizontal, RewiredAxisAction.Dir.NEGATIVE);
        bool flag2 = base.Player.GetButtonDown(this.uiHorizontal, RewiredAxisAction.Dir.POSITIVE);
        if(flag) {
            this.holdTime = 0f;
            this.lastInput = 1;
        } else if(flag2) {
            this.holdTime = 0f;
            this.lastInput = -1;
        } else if(this.lastInput != 0) {
            if(!base.Player.GetButton(this.uiHorizontal, (this.lastInput == 1) ? RewiredAxisAction.Dir.NEGATIVE : RewiredAxisAction.Dir.POSITIVE)) {
                this.lastInput = 0;
            } else {
                this.holdTime += Time.deltaTime;
                if(this.holdTime >= Scr_InputMaster.Instance.TimeToHoldBeforeLoopInputs) {
                    while(this.holdTime > Scr_InputMaster.Instance.TimeToHoldBeforeLoopInputs + Scr_InputMaster.Instance.TimeBetweenLoopInputs) {
                        this.holdTime -= Scr_InputMaster.Instance.TimeBetweenLoopInputs;
                        if(this.lastInput == 1) {
                            flag = true;
                        } else if(this.lastInput == -1) {
                            flag2 = true;
                        }
                    }
                }
            }
        }
        if(flag) {
            this.MoveDownStep();
            return;
        }
        if(flag2) {
            this.MoveUpStep();
        }
    }

    private void MoveDownStep() {
        Mgr_AudioPersistant.Instance.oneShotAudioSource.PlayOneShot(this.settingChangedAudio, this.sfxVol);
        this.SetValue(this.setting.PendingValue - this.setting.steps);
    }

    private void MoveUpStep() {
        Mgr_AudioPersistant.Instance.oneShotAudioSource.PlayOneShot(this.settingChangedAudio, this.sfxVol);
        this.SetValue(this.setting.PendingValue + this.setting.steps);
    }

    public override void SetValue(float value, bool save = false, bool pending = true, bool notify = true) {
        value = this.setting.Normalize(value);
        this.setting.SetValue(value, save: save, pending: pending, notify: notify);
        this.Refresh();
    }

    public override void RevertToDefault() {
        if(!this.IsDirty()) {
            return;
        }
        Mgr_AudioPersistant.Instance.oneShotAudioSource.PlayOneShot(this.settingChangedAudio, this.sfxVol);
        this.setting.SetValue(this.setting.DefaultValue, save: false, pending: false, notify: true);
        this.Refresh();
    }

    protected override void OnLanguagedChanged() { }

    protected virtual void OnSliderValueChanged(float value) {
        value = this.setting.Normalize(value);
        this.slider.value = value;
        if(value == this.setting.PendingValue)
            return;
        Mgr_AudioPersistant.Instance.oneShotAudioSource.PlayOneShot(this.settingChangedAudio, this.sfxVol);
        if(value > this.setting.PendingValue) {
            Scr_GamepadManager.Instance.PlayGamepadFeedback(Scr_GamepadManager.SettingsListLeft, Side.Left, 1f, 1f);
        } else {
            Scr_GamepadManager.Instance.PlayGamepadFeedback(Scr_GamepadManager.SettingsListRight, Side.Right, 1f, 1f);
        }
        this.setting.SetValue(value, save: false, pending: true, notify: true);
        this.Refresh();
    }

    protected override void Refresh() {
        base.Refresh();
        this.RefreshDirtyIndicator();
        this.RefreshValueLabel();
    }

    protected virtual void RefreshValueLabel() {
        if(this.setting != null) {
            if(this.valueLabel != null) {
                this.valueLabel.text = this.setting.GetPendingValueText();
            }
            if(this.slider != null) {
                Plugin.Logger.LogInfo("Setting slider value to " + this.setting.PendingValue);
                this.slider.value = this.setting.PendingValue;
            }
        }
    }
}