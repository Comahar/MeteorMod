using System;
using UnityEngine;

namespace MeteorMod.ModSettings.ModSettingItems {

    public abstract class ModSetting<T> : Setting<T>, IModSetting<T> {
        public string uiTextDictionary { get; set; }

        public ModSetting(
            string settingKey,
            string settingName,
            string tooltip,
            string uiTextDictionary,
            T defaultValue
        ) {
            this._settingKey = settingKey;
            this._settingName = settingName;
            this._tooltip = tooltip;
            this.defaultValue = defaultValue;
            this.uiTextDictionary = uiTextDictionary;
        }

        public void SetSettingValue(object value, bool save = true, bool notify = true) {
            this.SetValue((T)value, false, notify);
        }

        public void SetSettingValue(T value, bool save = true, bool notify = true) {
            this.SetValue(value, false, notify);
            if(save) {
                Mgr_ModSettings.Instance.SaveSettings();
            }
        }

        public object BoxedValue {
            get {
                return this.value;
            }
            set {
                this.value = (T)value;
            }
        }

        public abstract SettingsItem CreateUIElement();
    }

    public interface IModSetting {
        public string uiTextDictionary { get; }
        public object BoxedValue { get; set; }
        public SettingsItem CreateUIElement();
        public void SetSettingValue(object value, bool save = true, bool notify = true);
    }

    public interface IModSetting<T> : IModSetting {
        public void SetSettingValue(T value, bool save = true, bool notify = true);
    }
}