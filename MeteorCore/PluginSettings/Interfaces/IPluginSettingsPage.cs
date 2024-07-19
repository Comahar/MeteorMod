using System.Collections.Generic;
using UnityEngine;
using MeteorCore.Setting.Interfaces.Internal;

namespace MeteorCore.Setting.Interfaces;

public interface IPluginSettingsPage {
    public Transform settingsHolder { get; }
    public List<IPluginSetting> settings { get; }
    // Called when creating settings page
    // Can be used as a factory and return different GameObject
    public GameObject Initialize(string pageName, List<IPluginSetting> settings);
    // Called when settings items are initilized and added to the page
    public void OnInitialized();
    public void AddSettingUIItem(IPluginSettingUIItem setting);
}
