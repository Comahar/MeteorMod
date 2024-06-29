using System;
using MelonLoader;
using UnityEngine;
using MeteorMod.ModSettings;
using MeteorMod.ModSettings.ModSettingItems;

namespace MeteorMod.Settings {
    public static class DisableSubtitlesSetting {
        public static ModBoolSetting disableSubtitles = new ModBoolSetting(
            "DisableSubtitles",
            "Disable Subtitles",
            "Disables subtitles in the game",
            "MISC",
            false
        );

        private static GameObject? subtitles;

        public static void Init() {
            Mgr_ModSettings.AddSetting<ModBoolSetting, bool>("MeteorMod", disableSubtitles);
            disableSubtitles.onValueChanged = (Setting.ValueChangeCallback)Delegate.Combine(
                disableSubtitles.onValueChanged,
                new Setting.ValueChangeCallback(DisableSubtitlesSettingChanged)
            );
        }

        public static void OnSceneWasInitialized(int buildIndex, string sceneName) {
            if(sceneName == "Splash" || sceneName == "Title" || MeteorMod.IsMinigameScene || MeteorMod.IsPerformenceScene)
                return;
            subtitles = GameObject.Find("SCENE_MASTER/SpeechMaster/SpeechCanvas/SafeArea/Subtitles");
            if(subtitles == null) {
                MelonLogger.Warning("Could not find DialogueSubtitles GameObject");
                return;
            }
            SetSubtitlesState(!disableSubtitles.value);
        }

        public static void DisableSubtitlesSettingChanged() {
            SetSubtitlesState(!disableSubtitles.value);
        }

        public static void SetSubtitlesState(bool state) {
            if(subtitles != null) {
                subtitles.SetActive(state);
            }
        }
    }
}
