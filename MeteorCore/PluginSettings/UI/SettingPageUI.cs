using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MeteorCore.Localiser;
using MeteorCore.Utils;
using MeteorCore.Setting.Interfaces;
using MeteorCore.Setting.Interfaces.Internal;

namespace MeteorCore.Setting;

public class PluginSettingsPage : ElementListPage, IPluginSettingsPage {
    protected TextMeshProUGUI tooltipTitle;
    protected TextMeshProUGUI tooltip;
    protected TextMeshProUGUI defaultValueTip;
    protected LabelledRewiredAction revertToDefault;
    protected IPluginSettingUIItem selectedSettingUIItem {
        get {
            if(this.elementController == null || this.elementController.selectedElementIndex < 0) {
                return null;
            }
            return this.settingsUIItems[this.elementController.selectedElementIndex];
        }
    }
    public Transform settingsHolder { get; private set; }
    public List<IPluginSettingUIItem> settingsUIItems = new List<IPluginSettingUIItem>();
    public List<IPluginSetting> settings { get; private set; }

    public override void OnEnable() {
        base.OnEnable();
        this.RefreshTooltip(this.selectedSettingUIItem);
        LocalisationManager.Instance.LanguageChanged += this.OnLanguagedChanged;
        if(this.elementController != null && this.elementController.selectedElementIndex == -1) {
            this.elementController.SelectElement(0);
        }
    }

    protected override void OnDisable() {
        base.OnDisable();
        LocalisationManager.Instance.LanguageChanged -= this.OnLanguagedChanged;
    }

    protected override void OnDestroy() {
        if(LocalisationManager.Instance != null) {
            LocalisationManager.Instance.LanguageChanged -= this.OnLanguagedChanged;
        }
    }

    public override void Update() {
        if(!base.isActive || !base.IsInteractable())
            return;
        base.Update();
        if(this.elementController.selectedElementIndex == -1)
            return;
        if(this.player.GetButtonDown(this.revertToDefault.action)) {
            this.selectedSettingUIItem.RevertToDefault();
        }
    }

    // Called by SettingsMenu
    public override void InitializePage() { }

    protected virtual void OnLanguagedChanged() {
        this.RefreshTooltip(this.selectedSettingUIItem);
    }

    public override void OnSelectionChanged(int previousIndex, int currentIndex) {
        if(this.selectionChangedAudio != null) {
            Mgr_AudioPersistant.instance.oneShotAudioSource.PlayOneShot(this.selectionChangedAudio, this.sfxVol);
        }

        IPluginSettingUIItem settingItem = this.settingsUIItems[currentIndex];
        this.RefreshTooltip(settingItem);
        base.UpdateControls();
    }

    protected virtual void RefreshTooltip(IPluginSettingUIItem settingUIItem) {
        if(this.tooltipTitle == null || this.tooltip == null)
            return;
        if(settingUIItem == null)
            return;
        this.tooltipTitle.text = settingUIItem.settingInterface.GetSettingName();
        this.tooltip.text = settingUIItem.settingInterface.GetTooltip();
        string defaultValuePrefix = PluginLocaliser.Translate("Default", Plugin.metadata);
        string defaultValueText = settingUIItem.settingInterface.DefaultValueText();
        if(string.IsNullOrEmpty(defaultValueText)) {
            this.defaultValueTip.text = "";
        } else {
            this.defaultValueTip.text = $"({defaultValuePrefix}: {settingUIItem.settingInterface.DefaultValueText()})";
        }
    }

    // used by TabbedMenu.OnControlsUpdated
    public override void GetControlsToAdd(List<LabelledRewiredAction> actions) {
        base.GetControlsToAdd(actions);
        if(this.elementController.selectedElementIndex == -1)
            return;
        actions.Add(this.revertToDefault);
        actions.AddRange(this.settingsUIItems[this.elementController.selectedElementIndex].actionsToAddToControls);
    }


    // Called by Mgr_PluginSettings
    // this function should not use any "this" references except for Destroy
    public virtual GameObject Initialize(string pageName, List<IPluginSetting> settings) {
        // Find base settings menu
        SettingsMenu settingsMenu = GameObject.FindObjectOfType<SettingsMenu>(true);
        if(settingsMenu == null) {
            Plugin.Logger.LogError("Could not find SettingsMenu GameObject");
            return null;
        }
        GameObject basePage = settingsMenu.pages[0].gameObject;
        Transform pages = basePage.transform.parent;

        // create a page copy, add PluginSettingsPage component
        // copy some members from base SettingsPage
        // remove SettingsPage component
        // delete this gameobject
        GameObject newPage = GameObject.Instantiate(basePage, pages);
        SettingsPage settingsPage = newPage.GetComponent<SettingsPage>();
        PluginSettingsPage newPagePluginSettingsPage = newPage.AddComponent<PluginSettingsPage>();

        // Copy some members from base page
        MeteorUtils.CopyComponentValues<MenuPage>(settingsPage, newPagePluginSettingsPage, copyPrivate: true);
        MeteorUtils.CopyComponentValues<ElementListPage>(settingsPage, newPagePluginSettingsPage, copyPrivate: true);
        newPagePluginSettingsPage.tooltip = settingsPage.tooltip;
        newPagePluginSettingsPage.tooltipTitle = settingsPage.tooltipTitle;
        newPagePluginSettingsPage.defaultValueTip = settingsPage.defaultValueTip;
        newPagePluginSettingsPage.revertToDefault = settingsPage.revertToDefault;

        // Set page name
        newPagePluginSettingsPage.tabName = pageName;



        // Add this menu page to settingsmenu component
        Array.Resize(ref settingsMenu.pages, settingsMenu.pages.Length + 1);
        settingsMenu.pages[settingsMenu.pages.Length - 1] = newPagePluginSettingsPage;

        // Clean items of copied page
        newPagePluginSettingsPage.settingsHolder = newPage.transform.Find("ContentCanvasGroup/ContentParent/Content/Viewport/Items/");
        if(newPagePluginSettingsPage.settingsHolder == null) {
            Plugin.Logger.LogError("Could not get Items of newly instantiated settings page");
        }
        foreach(Transform child in newPagePluginSettingsPage.settingsHolder) {
            GameObject.Destroy(child.gameObject);
        }

        // These are done to make the page scrollable
        RectTransform rectTransform = newPagePluginSettingsPage.settingsHolder.gameObject.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0, 1);
        rectTransform.pivot = new Vector2(0, 1);
        ContentSizeFitter contentSizeFitter = newPagePluginSettingsPage.settingsHolder.gameObject.AddComponent<ContentSizeFitter>();
        contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.MinSize;

        // remove SettingsPage component
        settingsPage.enabled = false;
        Destroy(settingsPage);

        // Destroy this gameobject
        Destroy(this.gameObject);

        newPage.SetActive(false);
        return newPage;
    }

    public void OnInitialized() {
        if(this.elementController == null) {
            Plugin.Logger.LogError("Could not get ElementController in PluginSettingsPage");
            return;
        }

        List<SelectableElement> selectableElements = new List<SelectableElement>();
        foreach(IPluginSettingUIItem setting in this.settingsUIItems) {
            SelectableElement[] elements = setting.gameObject.GetComponents<SelectableElement>();
            if(elements.Length == 0) {
                Plugin.Logger.LogWarning($"SelectableElement not found for setting UI item {setting.settingInterface?.settingKey}");
                continue;
            }
            // only add non disabled elements
            // destroy call may be made in the same frame
            // toke make this work destroyed components needs to be disabled when destroyed
            foreach(SelectableElement element in elements) {
                if(element.enabled == false)
                    continue;
                selectableElements.Add(element);
            }
        }

        this.elementController.onSelectionChanged += this.OnSelectionChanged;
        this.elementController.SetSelectableList(selectableElements.ToArray());
        this.SetElements(selectableElements.ToArray());
    }

    public void AddSettingUIItem(IPluginSettingUIItem setting) {
        this.settingsUIItems.Add(setting);
    }
}
