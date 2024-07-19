using System.Collections.Generic;
using UnityEngine;
using TMPro;
using MeteorCore.Setting.Interfaces.Internal;

namespace MeteorCore.Setting.Interfaces {
    public interface IPluginSettingUIItem<T, D> : IPluginSettingUIItem where T : IPluginSetting<D> {
        public T setting { get; }
        public void SetValue(D value, bool save, bool pending, bool notify);
        public void SetSetting(T setting);
    }
}

namespace MeteorCore.Setting.Interfaces.Internal {
    public interface IPluginSettingUIItem {
        /// <summary>
        /// The setting interface of the UI item. Normally handled by PluginSettingUIItem class.
        /// </summary>
        public IPluginSetting settingInterface { get; }

        /// <summary>
        /// The dirty indicator of the UI item. Should be set by the initialize function.
        /// </summary>
        public GameObject isDirtyIndicator { get; }

        /// <summary>
        /// The label of the UI item. Should be set by the initialize function.
        /// </summary>
        public TextMeshProUGUI label { get; }

        /// <summary>
        /// The gameobject of the UI item. Should be set by the initialize function.
        /// </summary>
        public GameObject gameObject { get; }

        /// <summary>
        /// Actions to add to the controls of the UI item. Should be set by the initialize function.
        /// </summary>
        public List<LabelledRewiredAction> actionsToAddToControls { get; }

        /// <summary>
        /// Initializes the UI item and returns the gameobject.
        /// This can also function as a factory for creating UI items.
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="setting"></param>
        /// <returns></returns>
        public GameObject Initialize(Transform parent, IPluginSetting setting);

        /// <summary>
        /// Sets the setting to the setting interface this is used by the library to set the setting and should not be used by others.
        /// </summary>
        /// <param name="setting"></param>
        public void SetSettingInternal(IPluginSetting setting);

        /// <summary>
        /// Base game only reverts to default if the setting is dirty so this should be implemented accordingly.
        /// </summary>
        public void RevertToDefault();
    }
}