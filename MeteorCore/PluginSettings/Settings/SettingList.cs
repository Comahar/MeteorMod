using System;
using System.Collections.Generic;
using BepInEx;
using MeteorCore.Localiser;
using MeteorCore.Setting.AbstractClasses;

namespace MeteorCore.Setting;

public class PluginSettingList<T> : PluginSetting<int> {
    public List<PluginSettingListOption<T>> options;
    public Type optionType { get; protected set; }
    public int optionCount { get => this.options.Count; }

    public PluginSettingList(
        string settingKey,
        string settingName,
        int defaultValue,
        string tooltip,
        string configSection,
        BepInPlugin owner,
        List<PluginSettingListOption<T>> options
    ) : base(
        settingKey: settingKey,
        settingName: settingName,
        tooltip: tooltip,
        configSection: configSection,
        owner: owner,
        defaultValue: defaultValue
    ) {
        this.options = options;
        this.optionType = typeof(T);

        // check if the default value id is in the options
        // if not use the first option as the default
        if(!this.options.Exists(option => option.id == defaultValue)) {
            Plugin.Logger.LogError($"Default value id {defaultValue} not found in options for {settingKey}. Using first option instead");
            this.DefaultValue = this.options[0].id;
        }
    }


    public virtual PluginSettingListOption<T> GetCurrentOption() {
        return this.GetOptionById(this.Value);
    }

    public virtual PluginSettingListOption<T> GetPendingOption() {
        return this.GetOptionById(this.PendingValue);
    }

    public virtual PluginSettingListOption<T> GetOptionById(int id) {
        return this.options.Find(option => option.id == id);
    }

    public virtual int GetOptionIndex(int id) {
        return this.options.FindIndex(option => option.id == id);
    }

    public virtual int GetOptionIndex(PluginSettingListOption<T> value) {
        return this.options.IndexOf(value);
    }

    public override string GetValueText(int id) {
        return PluginLocaliser.Translate(this.GetOptionById(id).text, this.owner);
    }
}

public struct PluginSettingListOption<T> {
    public int id { get; set; }
    public string text { get; set; }
    public T value { get; set; }

    public PluginSettingListOption(int id, string text, T value) {
        this.id = id;
        this.text = text;
        this.value = value;
    }
}
