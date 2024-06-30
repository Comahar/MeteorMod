using System;
using UnityEngine;
using UnityEngine.UI;
using MeteorMod.ModSettings.ModSettingItems;

namespace MeteorMod.ModSettings {
    public class ModSettingsPage : MonoBehaviour {

        public SettingsPage settingsPage { get; private set; }

        private Transform itemsHolder;

        public static ModSettingsPage Create(string objectName, string pageName) {

            SettingsMenu settingsMenu = GameObject.FindObjectOfType<SettingsMenu>(true);
            if(settingsMenu == null) {
                Plugin.LOG.LogError("Could not find SettingsMenu GameObject");
                return null;
                throw new Exception("Could not find SettingsMenu GameObject");
            }

            Transform pages = settingsMenu.pages[0].transform.parent;
            if(pages == null) {
                Plugin.LOG.LogError("Could not get Pages GameObject");
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
                Plugin.LOG.LogError("Could not get Items of newly instantiated gameobject");
                throw new Exception("Could not get Items of newly instantiated gameobject");
            }
            foreach(Transform child in thisComponent.itemsHolder) {
                GameObject.Destroy(child.gameObject);
            }
            Plugin.LOG.LogInfo("Added Mods settings page. Object: " + objectName + " Name: " + pageName);

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

        public void AddSettingUIItem(SettingsItem setting) {
            setting.transform.SetParent(itemsHolder);
            settingsPage.items.Add(setting);
        }

    }
}
