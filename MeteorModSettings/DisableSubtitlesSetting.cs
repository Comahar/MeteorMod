using System;
using UnityEngine;
using MeteorMod.ModSettings;
using MeteorMod.ModSettings.ModSettingItems;
using BepInEx;
using UnityEngine.SceneManagement;

namespace MeteorMod.MeteorModSettings {
    public static class DisableSubtitlesSetting {
        public static ModBoolSetting disableSubtitles = new ModBoolSetting(
            "DisableSubtitles",
            "Disable Subtitles",
            "Disables subtitles in the game",
            "MISC",
            false
        );

        private static GameObject subtitles;

        public static void Init() {
            Mgr_ModSettings.AddSetting<ModBoolSetting, bool>(Plugin.metadata, disableSubtitles, true);
            disableSubtitles.onValueChanged += DisableSubtitlesSettingChanged;

            //SceneManager.sceneLoaded += SceneLoaded;
        }

        public static void SceneLoaded(Scene scene, LoadSceneMode mode) {
            Plugin.LOG.LogWarning($"DisableSubtitlesSetting SceneLoaded");
            if(!SceneHelper.IsGameScene)
                return;
            subtitles = GameObject.Find("SCENE_MASTER/SpeechMaster/SpeechCanvas/SafeArea/Subtitles");
            if(subtitles == null) {
                Plugin.LOG.LogWarning("Could not find DialogueSubtitles GameObject");
                return;
            }
            SetSubtitlesState(!disableSubtitles.value);
            Plugin.LOG.LogWarning($"DisableSubtitlesSetting SceneLoaded finish");
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
