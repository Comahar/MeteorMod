using UnityEngine;

namespace MeteorMod.ModSettings.ModSettingItems {
    public class ModSettingsItemSlider : MonoBehaviour {
        public SettingsItemSlider settingsItemSlider {
            get {
                return this.GetComponent<SettingsItemSlider>();
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

        public static ModSettingsItemSlider? Create(string objectName, string text, string dictionary, ModFloatSetting modFloatSetting, SettingsPage page) {
            if(UIBuilder.Instance.sliderPrefab == null) {
                MelonLoader.MelonLogger.Error("Tried to create a slider setting item but slider prefab is null");
                return null;
            }
            // Create slider
            Transform parent = page.transform.Find("ContentCanvasGroup/ContentParent/Content/Viewport/Items");
            GameObject sliderGameObject = GameObject.Instantiate(UIBuilder.Instance.sliderPrefab, parent);
            sliderGameObject.name = objectName;
            sliderGameObject.SetActive(true);

            // Add this component to gameobject
            ModSettingsItemSlider thisComponent = sliderGameObject.AddComponent<ModSettingsItemSlider>();
            thisComponent.SetText(text, dictionary);
            thisComponent.settingsItemSlider.floatSetting = modFloatSetting;
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


    public class ModFloatSetting : FloatSetting, IModSetting<float> {
        public string uiTextDictionary { get; set; }

        public ModFloatSetting(
            string settingKey,
            string settingName,
            string tooltip,
            string uiTextDictionary,
            float defaultValue
        ) {
            this._settingKey = settingKey;
            this._settingName = settingName;
            this._tooltip = tooltip;
            this.defaultValue = defaultValue;

            this.uiTextDictionary = uiTextDictionary;
        }

        public void SetSettingValue(float value, bool save = true, bool notify = true) {
            this.SetValue(value, false, notify);
            if(save) {
                Mgr_ModSettings.Instance.SaveSettings();
            }
        }
    }
}
