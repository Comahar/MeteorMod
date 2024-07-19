using UnityEngine;
using UnityEngine.SceneManagement;
using HarmonyLib;
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

/*
* This is the original code
* 	public void ClearSubtitles(bool playHideAnim = false)
*	{
*		this.SubtitlesController.ClearSubtitles(playHideAnim);
*		this.LlController.ClearSubtitles(playHideAnim, false);
*	}
* The patch is needed when the subtitles are disabled because this method will throw an exception that interrupts new scene loading
*/
[HarmonyPatch(typeof(InkMaster), nameof(InkMaster.ClearSubtitles))]
public static class ClearSubtitlesPatch {
    static bool Prefix(InkMaster __instance) {
        // skip the original method by returning false if SubtitlesController is null
        return __instance.SubtitlesController != null;
    }
}
