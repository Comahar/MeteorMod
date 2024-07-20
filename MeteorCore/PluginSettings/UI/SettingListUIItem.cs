using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MeteorCore.Localiser;
using MeteorCore.Setting.AbstractClasses;
using MeteorCore.Setting.Interfaces.Internal;
using MeteorCore.Utils;

namespace MeteorCore.Setting;

public class PluginSettingListUIItem<T> : PluginSettingUIItem<PluginSettingList<T>, int> {

    protected TextMeshProUGUI valueLabel;
    protected Image imgLeftButton;
    protected Image imgRightButton;
    protected bool wrapAround = false;
    protected float arrowOffAlpha = 0.2f;
    protected RewiredAxisAction uiHorizontal;
    protected StaticStringLocaliser localiser;
    protected float holdTime;
    protected int lastInput;

    protected int selectedOptionIndex;


    public override GameObject Initialize(Transform parent, IPluginSetting setting) {
        var settingConverted = setting as PluginSettingList<T>;
        if(settingConverted == null) {
            Plugin.Logger.LogError($"Could not convert setting to PluginSettingList<T> with key {setting?.settingKey}");
            return null;
        }

        GameObject listGameObject = Instantiate(SettingsUIPrefabs.ListPrefab, parent);
        listGameObject.name = setting.settingKey;
        SettingsItemList settingsItemList = listGameObject.GetComponent<SettingsItemList>();
        // Special component add as generic type adding is not supported
        Component component = listGameObject.AddComponent(this.GetType());
        PluginSettingListUIItem<T> thisComponent = component as PluginSettingListUIItem<T>;
        if(thisComponent == null) {
            Plugin.Logger.LogError($"PluginSettingListUIItem Could not add component {this.GetType()} to {listGameObject.name}");
            return null;
        }
        MeteorUtils.CopyComponentValues<SelectableElement>(settingsItemList, thisComponent, copyPrivate: true);

        // PluginSettingUIItem members
        thisComponent.setting = settingConverted;
        thisComponent.label = settingsItemList.label;
        thisComponent.isDirtyIndicator = settingsItemList.isDirtyIndicator;
        thisComponent.actionsToAddToControls = settingsItemList.actionsToAddToControls;
        thisComponent.settingChangedAudio = settingsItemList.settingChangedAudio;

        // PluginSettingListUIItem fields
        thisComponent.valueLabel = settingsItemList.textLabel;
        thisComponent.imgLeftButton = settingsItemList.imgLeftButton;
        thisComponent.imgRightButton = settingsItemList.imgRightButton;
        thisComponent.wrapAround = settingsItemList.wrapAround;
        thisComponent.arrowOffAlpha = settingsItemList.arrowOffAlpha;
        thisComponent.uiHorizontal = settingsItemList.uiHorizontal;
        thisComponent.localiser = settingsItemList.Localiser;
        thisComponent.holdTime = settingsItemList.holdTime;
        thisComponent.lastInput = settingsItemList.lastInput;

        // Bind button actions
        thisComponent.imgLeftButton.GetComponent<Button>().onClick.AddListener(thisComponent.PreviousChoice);
        thisComponent.imgRightButton.GetComponent<Button>().onClick.AddListener(thisComponent.NextChoice);

        // destroy value label statics tring localiser as we handle the translation ourselves
        StaticStringLocaliser valueLocaliser = thisComponent.valueLabel.GetComponent<StaticStringLocaliser>();
        if(valueLocaliser != null) {
            Destroy(valueLocaliser);
        }

        Destroy(settingsItemList);
        settingsItemList.enabled = false;
        Destroy(this.gameObject);

        StaticStringLocaliser localiser = thisComponent.transform.GetChild(0).GetComponent<StaticStringLocaliser>();
        if(localiser == null) {
            Plugin.Logger.LogError("PluginSettingListUIItem Could not find localiser");
            return null;
        }
        localiser.BaseText = setting.settingName;
        localiser.Dictionary = PluginLocaliser.ConvertPluginToDictionaryName(setting.owner);

        listGameObject.SetActive(true);
        thisComponent.SelectOption(thisComponent.setting.GetOptionIndex(thisComponent.setting.Value), pending: false);
        thisComponent.Refresh();
        return listGameObject;
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
            this.PreviousChoice();
            return;
        }
        if(flag2) {
            this.NextChoice();
        }
    }

    protected virtual void PreviousChoice() {
        this.SelectOption(this.selectedOptionIndex - 1, pending: true);
        Mgr_AudioPersistant.Instance.oneShotAudioSource.PlayOneShot(this.settingChangedAudio, this.sfxVol);
    }

    protected virtual void NextChoice() {
        this.SelectOption(this.selectedOptionIndex + 1, pending: true);
        Mgr_AudioPersistant.Instance.oneShotAudioSource.PlayOneShot(this.settingChangedAudio, this.sfxVol);
    }

    protected virtual void SelectOption(int index, bool pending) {
        if(index < 0) {
            Scr_GamepadManager.Instance.PlayGamepadFeedback(Scr_GamepadManager.SettingsListLeft, Koop.Side.Left);
            if(this.wrapAround) {
                index = this.setting.optionCount - 1;
            } else {
                return;
            }
        } else if(index >= this.setting.optionCount) {
            Scr_GamepadManager.Instance.PlayGamepadFeedback(Scr_GamepadManager.SettingsListRight, Koop.Side.Right);
            if(this.wrapAround) {
                index = 0;
            } else {
                return;
            }
        }
        this.selectedOptionIndex = index;
        this.SetValue(this.setting.options[index].id, save: false, pending: pending, notify: true);
    }

    public override void SetValue(int id, bool save = false, bool pending = true, bool notify = true) {
        this.setting.SetValue(id, save: save, pending: pending, notify: notify);
        this.Refresh();
    }

    public override void RevertToDefault() {
        this.setting.SetValue(this.setting.DefaultValue, save: false, pending: false, notify: true);
        this.selectedOptionIndex = this.setting.GetOptionIndex(this.setting.Value);
        Mgr_AudioPersistant.Instance.oneShotAudioSource.PlayOneShot(this.settingChangedAudio, this.sfxVol);
        this.Refresh();
    }

    protected override void OnLanguagedChanged() {
        this.Refresh();
    }

    protected override void Refresh() {
        base.Refresh();
        this.RefreshValueLabel();

        // Refresh arrow colors
        if(this.imgLeftButton != null) {
            Color color = this.imgLeftButton.color;
            color.a = (this.selectedOptionIndex > 0) ? 1f : this.arrowOffAlpha;
            this.imgLeftButton.color = color;
        }
        if(this.imgRightButton != null) {
            Color color = this.imgRightButton.color;
            color.a = (this.selectedOptionIndex < this.setting.optionCount - 1) ? 1f : this.arrowOffAlpha;
            this.imgRightButton.color = color;
        }
    }

    protected virtual void RefreshValueLabel() {
        if(this.valueLabel != null) {
            this.valueLabel.text = this.setting.GetPendingValueText();
        }
    }
}

// these are used make a concrete type for the list setting, as closed generic monobehaviours are not supported
// you can make your own types if needed
public class PluginStringListSettingUIItem : PluginSettingListUIItem<string> { }
public class PluginIntListSettingUIItem : PluginSettingListUIItem<int> { }
public class PluginFloatListSettingUIItem : PluginSettingListUIItem<float> { }
public class PluginBoolListSettingUIItem : PluginSettingListUIItem<bool> { }