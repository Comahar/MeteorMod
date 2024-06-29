using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using MelonLoader;

namespace MeteorMod.ModSettings {
    public class Mgr_ModSettings : MonoBehaviour {
        public static Mgr_ModSettings Instance {
            get {
                if(instance == null) {
                    instance = Create();
                }
                return instance;
            }
        }

        private static Mgr_ModSettings? instance;

        private ModSettingsPage? defaultPage;

        private static Mgr_ModSettings Create() {
            GameObject gameObject = new GameObject("ModSettingsManager");
            GameObject.DontDestroyOnLoad(gameObject);
            Mgr_ModSettings component = gameObject.AddComponent<Mgr_ModSettings>();
            instance = component;

            return component;
        }

        public void OnSceneWasLoaded(int buildIndex, string sceneName) {
            if(sceneName == "Splash" || MeteorMod.IsPerformenceScene || MeteorMod.IsMinigameScene)
                return;

            // Create default mod settings page
            Instance.defaultPage = ModSettingsPage.Create("ModsSettingPage", "Mods");
            foreach(KeyValuePair<string, List<Setting>> modSettings in _modSettings) {
                foreach(Setting setting in modSettings.Value) {
                    // TODO this probably wouldn't work for AddSettingItem<T, D>
                    dynamic dynSetting = setting;
                    AddSettingItem(dynSetting, Instance.defaultPage);
                }
            }
        }


        // Mod Setting List
        public Dictionary<string, List<Setting>> modSettings {
            get {
                return _modSettings;
            }
        }

        private static Dictionary<string, List<Setting>> _modSettings = new Dictionary<string, List<Setting>>();

        private static Dictionary<string, MelonPreferences_Category> melonPreferencesCategories = new Dictionary<string, MelonPreferences_Category>();
        private static Dictionary<string, List<MelonPreferences_Entry>> melonPreferencesEntries = new Dictionary<string, List<MelonPreferences_Entry>>();
        
        //public static Dictionary<string, ModSettingsPage> settingPages = new Dictionary<string, ModSettingsPage>();
        //private static Dictionary<string, Setting> settingPageMap = new Dictionary<string, Setting>();

        public static bool RemoveSetting<T, D>(string modName, T setting, ModSettingsPage page) where T : Setting, IModSetting<D> {
            if(!_modSettings.ContainsKey(modName)) {
                return false;
            }
            return _modSettings[modName].Remove(setting);
        }

        public static void AddSetting<T, D>(string modName, T setting, string settingPageName) where T : Setting<D>, IModSetting<D> {
            if(!_modSettings.ContainsKey(modName)) {
                _modSettings.Add(modName, new List<Setting>());
            }
            _modSettings[modName].Add(setting);

            if(!melonPreferencesCategories.ContainsKey(modName)) {
                MelonPreferences_Category category = MelonPreferences.CreateCategory(modName);
                melonPreferencesCategories.Add(modName, category);
            }
            if(!melonPreferencesEntries.ContainsKey(modName)) {
                melonPreferencesEntries.Add(modName, new List<MelonPreferences_Entry>());
            }
            MelonPreferences_Entry<D> entry = MelonPreferences.CreateEntry<D>(
                modName,
                setting.settingKey,
                setting,
                setting.settingName,
                setting.tooltip,
                false
            );
            melonPreferencesEntries[modName].Add(entry);

            //settingPageMap.Add(settingPageName, setting);

            // set using SetValue with pending: false to not make it dirty
            setting.SetValue(entry.Value, pending: false, notify: false);
        }

        public static void AddSetting<T, D>(string modName, T setting) where T : Setting<D>, IModSetting<D> {
            //TODO user should be able to set the page
            AddSetting<T, D>(modName, setting, "default");
            //AddSetting<T, D>(modName, setting, defaultPage);
        }

        private void AddSettingItem<T>(T setting, ModSettingsPage page) where T : Setting, IModSetting {
            page.CreateSettingUIItem(setting);
        }

        private void AddSettingItem<T, D>(T setting, ModSettingsPage page) where T : Setting<D>, IModSetting {
            page.CreateSettingUIItem<T, D>(setting);
        }

        // normally this is in SettingsPage class however this is better handedled here
        // as a bonus this class is singleton so accessing in static methods are easier
        public bool HaveSettingsChanged() {
            foreach(KeyValuePair<string, List<Setting>> modSettings in _modSettings) {
                foreach(Setting setting in modSettings.Value) {
                    if(setting.isDirty)
                        return true;
                }
            }
            return false;
        }

        public void SaveSettings() {
            foreach(KeyValuePair<string, List<Setting>> modSettings in _modSettings) {
                string modName = modSettings.Key;
                List<MelonPreferences_Entry> melonPreferences_Entries = melonPreferencesEntries[modName];
                foreach(Setting setting in modSettings.Value) {
                    MelonPreferences_Entry melonPreferences_Entry_nonTyped = melonPreferences_Entries.Find(
                        entry => entry.Identifier == setting.settingKey
                    );
                    // couldn't find a better way of casting generic type to typed object
                    dynamic melonPreferenceEntry = melonPreferences_Entry_nonTyped;
                    dynamic settingTyped = setting;
                    melonPreferenceEntry.Value = settingTyped.value;
                }
            }
            MelonPreferences.Save();
        }
    }

    [HarmonyPatch(typeof(SettingsMenu), nameof(SettingsMenu.HaveSettingsChangedWhileOpen))]
    public class SettingsPage_HaveSettingsChangedWhileOpen_Patch {
        public static void Postfix(ref bool __result) {
            if(__result)
                return;
            __result = Mgr_ModSettings.Instance.HaveSettingsChanged();
        }
    }

    [HarmonyPatch(typeof(Mgr_Settings), nameof(Mgr_Settings.ApplySettings))]
    public class Mgr_Settings_ApplySettings_Patch {
        public static void Prefix() {
            MelonLogger.Msg("Saving");
            try {
                foreach(KeyValuePair<string, List<Setting>> modSettings in Mgr_ModSettings.Instance.modSettings) {
                    foreach(Setting setting in modSettings.Value) {
                        setting.ApplyPendingValue();
                    }
                }
                Mgr_ModSettings.Instance.SaveSettings();
                MelonLogger.Msg("Mod settings saved");
            } catch(Exception e) {
                MelonLogger.Error("Error saving mod settings: " + e);
            }
        }
    }

    [HarmonyPatch(typeof(SettingsMenu), nameof(SettingsMenu.RevertAllPendingSettingsChanges))]
    public class SettingsMenu_RevertAllPendingSettingsChanges_Patch {
        public static void Postfix() {
            foreach(KeyValuePair<string, List<Setting>> modSettings in Mgr_ModSettings.Instance.modSettings) {
                foreach(Setting setting in modSettings.Value) {
                    setting.RevertPendingValue();
                }
            }
        }
    }

    public interface IModSetting {
        public string uiTextDictionary { get; }
    }

    public interface IModSetting<T> : IModSetting {
        public void SetSettingValue(T value, bool save = true, bool notify = true);
    }
}
