using UnityEngine;
using HarmonyLib;
using MeteorMod.ModSettings;

namespace MeteorMod.ModSettings.ModSettingItems {
    public class ModSettingsItemToggle : MonoBehaviour {
        public SettingsItemToggle settingsItemToggle {
            get {
                return this.GetComponent<SettingsItemToggle>();
            }
        }

        public string text {
            get {
                return this.localiser.BaseText;
            }
            set {
                this.localiser.BaseText = value;
            }
        }

        public StaticStringLocaliser localiser {
            get {
                return this.GetComponentInChildren<StaticStringLocaliser>();
            }
        }

        public static ModSettingsItemToggle Create(string objectName, string text, string dictionary, ModBoolSetting modBoolSetting, SettingsPage page) {
            if(ModSettingsUIBuilder.Instance.togglePrefab == null) {
                Plugin.LOG.LogError("Tried to create a toggle setting item but list prefab is null");
                return null;
            }
            // Create toggle
            Transform parent = page.transform.Find("ContentCanvasGroup/ContentParent/Content/Viewport/Items");
            GameObject toggleGameObject = GameObject.Instantiate(ModSettingsUIBuilder.Instance.togglePrefab, parent);
            toggleGameObject.name = objectName;
            toggleGameObject.SetActive(true);

            // Add this component to gameobject
            ModSettingsItemToggle thisComponent = toggleGameObject.AddComponent<ModSettingsItemToggle>();
            thisComponent.SetText(text, dictionary);
            thisComponent.settingsItemToggle.boolSetting = modBoolSetting;
            return thisComponent;
        }

        public void SetText(string text) {
            this.localiser.BaseText = text;
        }

        public void SetText(string text, string dictionary) {
            this.localiser.BaseText = text;
            this.localiser.Dictionary = dictionary;
        }

    }

    public class ModBoolSetting : BoolSetting, IModSetting<bool> {
        public string uiTextDictionary { get; set; }

        public ModBoolSetting(
            string settingKey,
            string settingName,
            string tooltip,
            string uiTextDictionary,
            bool defaultValue
        ) {
            this._settingKey = settingKey;
            this._settingName = settingName;
            this._tooltip = tooltip;
            this.defaultValue = defaultValue;

            this.uiTextDictionary = uiTextDictionary;
        }

        public override SettingsItem CreateUIElement() {

        public void SetSettingValue(bool value, bool save = true, bool notify = true) {
            this.SetValue(value, false, notify);
            if(save) {
                Mgr_ModSettings.Instance.SaveSettings();
            }
        }
    }

    [HarmonyPatch(typeof(SettingsItemToggle), nameof(SettingsItemToggle.RefreshLabel))]
    public class SettingsItemToggle_RefreshLabel_PatchBugfix {
        public static bool Prefix(SettingsItemToggle __instance) {
            /* Original
             * SetT(boolSetting.value ? 1f : 0f);
             * OnLanguagedChanged();
             * This bug causes the toggle to return the original value position when changing settings pages
             * it should set to the pending value instead
             * This can be done with a transpiler but too much work for a simple function
             */
            __instance.SetT(__instance.boolSetting.pendingValue ? 1f : 0f);
            __instance.OnLanguagedChanged();

            return false; // skip the original as it is bugged
        }
    }
}
