using System;
using UnityEngine;
using UnityEngine.UI;
using MelonLoader;
using MeteorMod.ModSettings.ModSettingItems;

namespace MeteorMod.ModSettings {
    public class ModSettingsPage : MonoBehaviour {

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public SettingsPage settingsPage { get; private set; }

        private Transform itemsHolder;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        public static ModSettingsPage? Create(string objectName, string pageName) {
            SettingsMenu settingsMenu = GameObject.FindObjectOfType<SettingsMenu>(true);
            if(settingsMenu == null) {
                MelonLogger.Error("Could not find SettingsMenu GameObject");
                return null;
                throw new Exception("Could not find SettingsMenu GameObject");
            }

            Transform pages = settingsMenu.pages[0].transform.parent;
            if(pages == null) {
                MelonLogger.Error("Could not get Pages GameObject");
                return null;
                throw new Exception("Could not get Pages GameObject");
            }

            GameObject modsPage = GameObject.Instantiate(pages.GetChild(0).gameObject, pages);
            modsPage.SetActive(false);
            modsPage.name = objectName;
            ModSettingsPage thisComponent = modsPage.AddComponent<ModSettingsPage>();
            SettingsPage settingsPage = modsPage.GetComponent<SettingsPage>();
            settingsPage.tabName = pageName;
            thisComponent.settingsPage = settingsPage;

            // Add mods page to settingsmenu component
            MenuPage[] newPages = new MenuPage[settingsMenu.pages.Length + 1];
            for(int i = 0; i < settingsMenu.pages.Length; i++) {
                newPages[i] = settingsMenu.pages[i];
            }
            newPages[newPages.Length - 1] = settingsPage;
            settingsMenu.pages = newPages;

            // clean items
            // TODO find a better way to do this
            thisComponent.itemsHolder = modsPage.transform.GetChild(0).GetChild(0).GetChild(0).GetChild(0).GetChild(0);
            if(thisComponent.itemsHolder == null) {
                MelonLogger.Error("Could not get Items of newly instantiated gameobject");
                throw new Exception("Could not get Items of newly instantiated gameobject");
            }
            foreach(Transform child in thisComponent.itemsHolder) {
                GameObject.Destroy(child.gameObject);
            }
            MelonLogger.Msg("Added Mods settings page. Object: " + objectName + " Name: " + pageName);

            thisComponent.ApplyModifications();
            return thisComponent;
        }

        private void ApplyModifications() {
            RectTransform rectTransform = itemsHolder.gameObject.GetComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0, 1);
            rectTransform.pivot = new Vector2(0, 1);
            // add content size fitter
            ContentSizeFitter contentSizeFitter = itemsHolder.gameObject.AddComponent<ContentSizeFitter>();
            contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.MinSize;
            this.GetComponentInChildren<Scrollbar>().value = 1;
        }

        public bool HaveSettingsChangedWhileOpen() {
            foreach(SettingsItem settingItem in settingsPage.items) {
                Setting setting = settingItem.GetSettingAsset();
                if(setting.isDirty)
                    return true;
            }
            return false;
        }

        public void CreateSettingUIItem<T>(T setting) where T : Setting, IModSetting {
            if(setting is ModBoolSetting modBoolSetting) {
                //MelonLogger.Msg("Creating ModBoolSetting " + setting.name);
                ModSettingsItemToggle.Create(
                    modBoolSetting.settingKey,
                    modBoolSetting.settingName,
                    modBoolSetting.uiTextDictionary,
                    modBoolSetting,
                    settingsPage
                );
            } else if(setting is ModFloatSetting modFloatSetting) {
                //MelonLogger.Msg("Creating ModFloatSetting " + setting.name);
                ModSettingsItemSlider.Create(
                    modFloatSetting.settingKey,
                    modFloatSetting.settingName,
                    modFloatSetting.uiTextDictionary,
                    modFloatSetting,
                    settingsPage
                );
            } else {
                MelonLogger.Error("Setting type not supported " + setting.GetType());
            }
        }

        public void CreateSettingUIItem<T, D>(T setting) where T : Setting<D>, IModSetting {
            if(setting is ModListSetting<int> modListSetting) {
                //MelonLogger.Msg("Creating ModListSetting " + setting.name);
                ModSettingsItemList.Create(
                    modListSetting.settingKey,
                    modListSetting.settingName,
                    modListSetting.uiTextDictionary,
                    modListSetting,
                    settingsPage
                );
            } else {
                MelonLogger.Error("Setting type not supported " + setting.GetType());
            }
        }
    }
}
