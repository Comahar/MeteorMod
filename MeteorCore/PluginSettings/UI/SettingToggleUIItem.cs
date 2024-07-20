using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MeteorCore.Localiser;
using MeteorCore.Setting.AbstractClasses;
using MeteorCore.Setting.Interfaces.Internal;
using MeteorCore.Utils;

namespace MeteorCore.Setting;

public class PluginSettingToggleUIItem : PluginSettingUIItem<PluginSettingBool, bool> {

    // RewiredAction
    public int input_select { get; set; }

    private Image background;
    private RectTransform knobParent;
    private RectTransform knob;
    private TextMeshProUGUI valueLabel;
    private Coroutine animCoroutine;
    private float animT;
    private float animTotalTime = 0.1f;
    private Color toggledOffColor = Color.gray;
    private Color toggledOnColor = Color.white;


    // Used as a factory for creating UI items
    public override GameObject Initialize(Transform parent, IPluginSetting setting) {
        var settingConverted = setting as PluginSettingBool;
        if(settingConverted == null) {
            Plugin.Logger.LogError($"Could not convert setting to PluginSettingBool with key {setting?.settingKey}");
            return null;
        }

        GameObject toggleGameObject = Instantiate(SettingsUIPrefabs.TogglePrefab, parent);
        toggleGameObject.name = setting.settingKey;
        SettingsItemToggle settingsItemToggle = toggleGameObject.GetComponent<SettingsItemToggle>();
        PluginSettingToggleUIItem thisComponent = toggleGameObject.AddComponent<PluginSettingToggleUIItem>();

        MeteorUtils.CopyComponentValues<SelectableElement>(settingsItemToggle, thisComponent, copyPrivate: true);

        // PluginSettingUIItem members
        thisComponent.setting = settingConverted;
        thisComponent.label = settingsItemToggle.label;
        thisComponent.isDirtyIndicator = settingsItemToggle.isDirtyIndicator;
        thisComponent.actionsToAddToControls = settingsItemToggle.actionsToAddToControls;
        thisComponent.settingChangedAudio = settingsItemToggle.settingChangedAudio;

        // PluginSettingToggleUIItem fields
        thisComponent.knob = settingsItemToggle.knob;
        thisComponent.knobParent = settingsItemToggle.knobParent;
        thisComponent.background = settingsItemToggle.background;
        thisComponent.valueLabel = settingsItemToggle.valueLabel;
        thisComponent.animCoroutine = settingsItemToggle.animCoroutine;
        thisComponent.toggledOffColor = settingsItemToggle.toggledOffColor;
        thisComponent.toggledOnColor = settingsItemToggle.toggledOnColor;
        thisComponent.input_select = settingsItemToggle.input_select;

        // Button on click event
        thisComponent.GetComponentInChildren<Button>().onClick.AddListener(thisComponent.Toggle);

        // If you are going to make another UI item you need to do it like this
        // this needs to be done because the game will destroy the object at the end of the frame
        // however settingsPage will search for all the items in the same frame and check if they are enabled
        Destroy(settingsItemToggle);
        settingsItemToggle.enabled = false;

        Destroy(this.gameObject);

        StaticStringLocaliser localiser = thisComponent.transform.GetChild(0).GetComponent<StaticStringLocaliser>();
        if(localiser == null) {
            Plugin.Logger.LogError("PluginSettingToggleUIItem Could not find localiser");
            return null;
        }
        localiser.BaseText = setting.settingName;
        localiser.Dictionary = PluginLocaliser.ConvertPluginToDictionaryName(setting.owner);

        toggleGameObject.SetActive(true);
        thisComponent.Refresh();
        thisComponent.SetT(thisComponent.setting.PendingValue ? 1f : 0f);
        return toggleGameObject;
    }

    public override void OnEnable() {
        base.OnEnable();
        this.OnLanguagedChanged();
    }

    public virtual void Update() {
        if(!base.isSelected || !base.IsInteractable()) {
            return;
        }
        if(base.Player.GetButtonDown(this.input_select)) {
            this.Toggle();
        }
    }

    public virtual void Toggle() {
        Mgr_AudioPersistant.Instance.oneShotAudioSource.PlayOneShot(this.settingChangedAudio, this.sfxVol);
        this.SetValue(!this.setting.PendingValue, save: false, pending: true, notify: true);
    }

    public override void SetValue(bool value, bool save = false, bool pending = true, bool notify = true) {
        this.setting.SetValue(value, save: save, pending: pending, notify: notify);
        this.Refresh();
        this.TriggerAnimation();
    }

    public override void RevertToDefault() {
        if(!this.IsDirty()) {
            return;
        }
        Mgr_AudioPersistant.Instance.oneShotAudioSource.PlayOneShot(this.settingChangedAudio, this.sfxVol);
        this.setting.SetValue(this.setting.DefaultValue, save: false, pending: false, notify: true);
        this.Refresh();
        this.TriggerAnimation();
    }

    protected override void OnLanguagedChanged() {
        this.Refresh();
    }

    protected override void Refresh() {
        base.Refresh();
        this.RefreshValueLabel();
    }

    protected virtual void RefreshValueLabel() {
        if(this.valueLabel != null) {
            this.valueLabel.text = this.setting.GetPendingValueText();
        }
    }

    private void TriggerAnimation() {
        if(this.animCoroutine != null) {
            base.StopCoroutine(this.animCoroutine);
        }
        this.animCoroutine = base.StartCoroutine(this.AnimCoroutine(this.setting.PendingValue));
    }

    private IEnumerator AnimCoroutine(bool on) {
        float goTo = (on ? 1f : 0f);
        float currentT = this.animT;
        float time = 0f;
        while(time < this.animTotalTime) {
            time += Time.deltaTime;
            this.SetT(Mathf.Lerp(currentT, goTo, time / this.animTotalTime));
            yield return null;
        }
        yield break;
    }

    private void SetT(float t) {
        if(this.knob == null || this.background == null || this.knobParent == null) {
            return;
        }
        this.animT = t;
        this.background.color = Color.Lerp(this.toggledOffColor, this.toggledOnColor, t);
        this.knob.anchoredPosition = new Vector2(Mathf.Lerp(2f, this.knobParent.rect.width - this.knob.rect.width - 2f, t), this.knob.anchoredPosition.y);
    }
}