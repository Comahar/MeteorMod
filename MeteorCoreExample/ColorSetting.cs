using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using BepInEx;
using MeteorCore.Localiser;
using MeteorCore.Setting;
using MeteorCore.Setting.AbstractClasses;
using MeteorCore.Setting.Interfaces.Internal;

namespace MeteorCoreExample;

public class MySettingColorUIItem : PluginSettingUIItem<MySettingColor, Color> {
    public SelectableGroupController selectableGroupController;
    private RewiredAxisAction uiHorizontal = new RewiredAxisAction { threshold = 0.8f, action = "UIHorizontal" };

    public override GameObject Initialize(Transform parent, IPluginSetting setting) {
        this.transform.SetParent(parent);
        // set setting
        var settingConverted = setting as MySettingColor;
        if(settingConverted == null) {
            Plugin.Logger.LogError($"Could not convert setting to PluginSettingFloat with key {setting?.settingKey}");
            return null;
        }
        this.setting = settingConverted;

        // add neccessary components
        RectTransform rectTransform = this.AddRectTransform(this.gameObject);
        rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, 100);
        this.gameObject.AddComponent<CanvasGroup>();

        // create label
        GameObject label = Instantiate(SettingsUIPrefabs.LabelPrefab, this.gameObject.transform);
        label.SetActive(true);
        StaticStringLocaliser localiser = label.GetComponent<StaticStringLocaliser>();
        if(localiser == null) {
            Plugin.Logger.LogError("MySettingColorUIItem Could not find localiser when initializing");
            return null;
        }
        localiser.BaseText = setting.settingName;
        localiser.Dictionary = PluginLocaliser.ConvertPluginToDictionaryName(setting.owner);

        // create buttons parent
        GameObject buttons = new GameObject("Buttons");
        buttons.transform.SetParent(this.gameObject.transform);
        RectTransform buttonsTransform = this.AddRectTransform(
            buttons,
            // Align right
            anchorMin: new Vector2(1, 0),
            anchorMax: new Vector2(1, 1),
            pivot: new Vector2(1, 0.5f),
            sizeDelta: new Vector2(690, 0)
        );
        this.selectableGroupController = buttons.AddComponent<SelectableGroupController>();
        GridLayoutGroup gridLayoutGroup = buttons.AddComponent<GridLayoutGroup>();
        gridLayoutGroup.cellSize = new Vector2(690 / 8 - 20, 690 / 8 - 20);
        gridLayoutGroup.spacing = new Vector2(10, 10);

        // create buttons
        List<SelectableElement> selectableElements = new List<SelectableElement>();
        for(int i = 0; i < this.setting.options.Length; i++) {
            GameObject colorButton = new GameObject("ColorButton");
            colorButton.transform.SetParent(buttons.transform);
            RectTransform buttonRectTransform = this.AddRectTransform(colorButton);

            SelectableElement selectableElement = colorButton.AddComponent<SelectableElement>();
            selectableElement.onSelected += this.OnButtonSelected;
            selectableElement.onDeselected += this.OnButtonDeselected;
            selectableElements.Add(selectableElement);

            Outline outline = colorButton.AddComponent<Outline>();
            outline.effectColor = Color.white;
            outline.effectDistance = new Vector2(3, 3);
            outline.enabled = false;

            // set sprite as circle saved in project
            Image image = colorButton.AddComponent<Image>();
            image.sprite = this.GetCircleSprite();
            image.color = this.setting.options[i];

            Button button = colorButton.AddComponent<Button>();
            button.targetGraphic = image;
            button.onClick.AddListener(() => {
                selectableElement.Select();
            });

            if(this.setting.options[i] == this.setting.Value) {
                outline.enabled = true;
            }
        }
        this.selectableGroupController.SetSelectableList(selectableElements.ToArray());
        this.selectableGroupController.selectedElementIndex = this.setting.options.ToList().IndexOf(this.setting.Value);

        // set neccassary PluginSettingUIItem fields
        this.label = label.GetComponent<TextMeshProUGUI>();
        this.isDirtyIndicator = label.transform.Find("IsDirtyIndicator").gameObject;
        this.settingChangedAudio = null;
        this.actionsToAddToControls = [
            new LabelledRewiredAction {
                    label = "Modify",
                    enabled = true,
                    action = 19
                }
        ];
        // get audioclip from a settingPage that has the reference
        this.settingChangedAudio = SettingsUIPrefabs.TogglePrefab.GetComponent<SettingsItem>().settingChangedAudio;

        return this.gameObject;
    }

    public void Update() {
        if(!base.isSelected || !base.IsInteractable()) {
            return;
        }
        bool flag = base.Player.GetButtonDown(this.uiHorizontal, RewiredAxisAction.Dir.POSITIVE);
        bool flag2 = base.Player.GetButtonDown(this.uiHorizontal, RewiredAxisAction.Dir.NEGATIVE);
        if(flag) {
            this.selectableGroupController.SelectNextElement();
        }
        if(flag2) {
            this.selectableGroupController.SelectPreviousElement();
        }
    }

    protected override void OnLanguagedChanged() { }

    public override void SetValue(Color color, bool save = false, bool pending = true, bool notify = true) {
        this.setting.SetValue(color, save: save, pending: pending, notify: notify);
        this.Refresh();
    }

    public override void RevertToDefault() {
        if(!this.IsDirty()) {
            return;
        }
        Mgr_AudioPersistant.Instance.oneShotAudioSource.PlayOneShot(this.settingChangedAudio, this.sfxVol);
        this.setting.SetValue(this.setting.DefaultValue, save: false, pending: false, notify: true);
        int index = this.setting.options.ToList().IndexOf(this.setting.Value);
        this.selectableGroupController.SelectElement(index);
        this.Refresh();
    }

    protected override void Refresh() {
        base.Refresh();
        this.RefreshDirtyIndicator();
    }

    private void OnButtonSelected(SelectableElement selectable) {
        Mgr_AudioPersistant.Instance.oneShotAudioSource.PlayOneShot(this.settingChangedAudio, this.sfxVol);
        selectable.gameObject.GetComponent<Outline>().enabled = true;
        this.SetValue(selectable.gameObject.GetComponent<Image>().color);
    }

    private void OnButtonDeselected(SelectableElement selectable) {
        selectable.gameObject.GetComponent<Outline>().enabled = false;
    }

    // Helper functions
    private RectTransform AddRectTransform(
        GameObject gameObject,
        Vector2? anchorMin = null,
        Vector2? anchorMax = null,
        Vector2? anchoredPosition = null,
        Vector2? pivot = null,
        Vector2? sizeDelta = null,
        Vector3? localScale = null
    ) {
        RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
        if(rectTransform == null) {
            rectTransform = gameObject.AddComponent<RectTransform>();
        }
        rectTransform.anchorMin = anchorMin ?? new Vector2(0, 1);
        rectTransform.anchorMax = anchorMax ?? new Vector2(0, 1);
        rectTransform.anchoredPosition = anchoredPosition ?? Vector2.zero;
        rectTransform.pivot = pivot ?? new Vector2(0, 1);
        rectTransform.localScale = localScale ?? Vector3.one;
        rectTransform.sizeDelta = sizeDelta ?? Vector2.zero;
        return rectTransform;
    }

    private Sprite GetCircleSprite() {
        System.IO.Stream stream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("MeteorCoreExample.Assets.circle.png");
        if(stream == null) {
            Plugin.Logger.LogError("Could not find circle.png");
            return null;
        }
        byte[] buffer;
        using(MemoryStream memoryStream = new MemoryStream()) {
            stream.CopyTo(memoryStream);
            buffer = memoryStream.ToArray();
        }
        Texture2D texture = new Texture2D(1, 1);
        texture.LoadImage(buffer);
        return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
    }
}


public class MySettingColor : PluginSetting<Color> {
    public Color[] options { get; set; }
    private string defaultValueText;
    public MySettingColor(
        string settingKey,
        string settingName,
        BepInPlugin owner,
        Color defaultValue,
        string defaultValueText,
        Color[] options,
        string tooltip,
        string configSection
    ) : base(
        settingKey: settingKey,
        settingName: settingName,
        defaultValue: defaultValue,
        tooltip: tooltip,
        configSection: configSection,
        owner: owner
    ) {
        this.options = options;
        this.defaultValueText = defaultValueText;
    }
    public override void SetValue(Color value, bool save, bool pending, bool notify) {
        base.SetValue(value, save: save, pending: pending, notify: notify);
    }

    public override string DefaultValueText() {
        return PluginLocaliser.Translate(this.defaultValueText, Plugin.metadata);
    }
}
