using System.Collections.Generic;
using UnityEngine;

namespace MeteorMod.ModSettings.ModSettingItems {
    public class ModSettingsItemList : MonoBehaviour {
        public SettingsItemList settingsItemList {
            get {
                return this.GetComponent<SettingsItemList>();
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

        public static ModSettingsItemList Create<T>(string objectName, string text, string dictionary, ModListSetting<T> modSetting, SettingsPage page) {
            if (ModSettingsUIBuilder.Instance.listPrefab == null) {
                Plugin.LOG.LogError("Tried to create a list setting item but list prefab is null");
                return null;
            }
            // Create list
            Transform parent = page.transform.Find("ContentCanvasGroup/ContentParent/Content/Viewport/Items");
            GameObject listGameObject = GameObject.Instantiate(ModSettingsUIBuilder.Instance.listPrefab, parent);
            listGameObject.name = objectName;
            listGameObject.SetActive(true);

            // Add this component to gameobject
            ModSettingsItemList thisComponent = listGameObject.AddComponent<ModSettingsItemList>();
            thisComponent.SetText(text, dictionary);
            thisComponent.settingsItemList.settingAsset = modSetting;
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

    public class ModListSetting<T> : Setting<T>, IModSetting<int> {
        public string uiTextDictionary { get; set; }

        public ModListSetting(
            string settingKey,
            string settingName,
            string tooltip,
            string uiTextDictionary,
            T defaultValue,
            List<SettingsOption<T>> options
        ) {
            this._settingKey = settingKey;
            this._settingName = settingName;
            this._tooltip = tooltip;
            this.defaultValue = defaultValue;
            this.options = options;

            this.uiTextDictionary = uiTextDictionary;
        }

        public void SetSettingValue(int value, bool save = true, bool notify = true) {
            this.SetValue(value, false, notify);
            if(save) {
                Mgr_ModSettings.Instance.SaveSettings();
            }
        }

        public override bool TryParseFromString(string value) {
            if(int.TryParse(value, out int result)) {
                this.SetValue(result, false, false);
                return true;
            }
            return false;
        }
    }
}
