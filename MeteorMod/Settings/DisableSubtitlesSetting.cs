using UnityEngine;
using UnityEngine.SceneManagement;
using MeteorCore.Setting;
using MeteorCore.Setting.Interfaces;

namespace MeteorMod.Settings;
public static class DisableSubtitlesSetting {
    public static PluginSettingBool disableSubtitlesSetting;

    private static GameObject subtitlesGameObject;

    public static void Init() {
        disableSubtitlesSetting = new PluginSettingBool(
            settingKey: "DisableSubtitles",
            settingName: "Disable Subtitles",
            tooltip: "Disables subtitles in the game",
            configSection: MyPluginInfo.PLUGIN_NAME,
            owner: Plugin.metadata,
            defaultValue: false
        );

        Mgr_PluginSettings.AddSetting<bool, PluginSettingToggleUIItem>(disableSubtitlesSetting, MyPluginInfo.PLUGIN_NAME, Plugin.metadata);
        disableSubtitlesSetting.OnValueChanged += DisableSubtitlesSettingChanged;

        SceneManager.sceneLoaded += SceneLoaded;
    }

    public static void SceneLoaded(Scene scene, LoadSceneMode mode) {
        if(!MeteorCore.SceneHelper.IsGameScene)
            return;
        subtitlesGameObject = GameObject.Find("SCENE_MASTER/SpeechMaster/SpeechCanvas/SafeArea/Subtitles");
        if(subtitlesGameObject == null) {
            Plugin.Logger.LogWarning("Could not find DialogueSubtitles GameObject");
            return;
        }
        SetSubtitlesState(!disableSubtitlesSetting.Value);
    }

    public static void DisableSubtitlesSettingChanged(IPluginSetting<bool> setting) {
        SetSubtitlesState(!setting.Value);
    }

    public static void SetSubtitlesState(bool state) {
        if(subtitlesGameObject != null) {
            subtitlesGameObject.SetActive(state);
        }
    }
}
