using System;
using BepInEx;
using MeteorCore.Setting.Interfaces.Internal;

namespace MeteorCore.Setting.Interfaces {
    public interface IPluginSetting<T> : IPluginSetting {
        public T Value { get; }
        public T PendingValue { get; }
        public T DefaultValue { get; }
        public event Action<IPluginSetting<T>> OnAppliedPendingValue;
        public event Action<IPluginSetting<T>> OnRevertedPendingValue;
        public event Action<IPluginSetting<T>> OnPendingValueChanged;
        public event Action<IPluginSetting<T>> OnValueChanged;
        public void SetValue(T value, bool save, bool pending, bool notify);
        public string GetValueText(T value);
    }
}

namespace MeteorCore.Setting.Interfaces.Internal {
    public interface IPluginSetting {
        public object BoxedValue { get; }
        public bool isDirty { get; }
        public Type type { get; }
        public string settingKey { get; }
        public string configSection { get; }

        // Untranslated strings
        public string tooltip { get; }
        public string settingName { get; }

        public BepInPlugin owner { get; }
        public void SetValue(object value, bool save, bool pending, bool notify);
        public void ApplyPendingValue();
        public void RevertPendingValue();

        // These are translated strings
        public string DefaultValueText();
        public string GetCurrentValueText();
        public string GetPendingValueText();
        public string GetSettingName();
        public string GetTooltip();
    }
}