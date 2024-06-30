using UnityEngine;
using UnityEngine.SceneManagement;

namespace MeteorMod.ModSettings {
    public class ModSettingsUIBuilder : MonoBehaviour {
        private static ModSettingsUIBuilder _instance;
        public static ModSettingsUIBuilder Instance {
            get {
                if(_instance == null)
                    _instance = FindObjectOfType<ModSettingsUIBuilder>();
                return _instance;
            }
        }
        private void Awake() {
            if (_instance == null)
                _instance = this;
            if (_instance != this)
                Destroy(this);
            SceneManager.sceneLoaded += SceneLoaded;
        }

        private GameObject prefabHolder;


        internal GameObject togglePrefab;
        internal GameObject sliderPrefab;
        internal GameObject listPrefab;

        bool firstSceneLoad = true;
        public void SceneLoaded(Scene scene, LoadSceneMode mode) {
            Plugin.LOG.LogWarning($"ModSettingsUIBuilder SceneLoaded");
            // only run on title scene and at first scene load
            if(SceneHelper.IsTitleScene && firstSceneLoad) {
                firstSceneLoad = false;
                prefabHolder = new GameObject("UIBuilderPrefabs");
                prefabHolder.transform.SetParent(transform);
                LoadPrefabs();
                Plugin.LOG.LogInfo("UIBuilder instance created");
            }
            Plugin.LOG.LogWarning($"ModSettingsUIBuilder SceneLoaded finish");
        }

        public void ReloadPrefabs() {
            Plugin.LOG.LogInfo("Reloading prefabs");
            DeletePrefabs();
            LoadPrefabs();
        }

        private void DeletePrefabs() {
            foreach(Transform child in prefabHolder.transform) {
                Destroy(child.gameObject);
            }
        }

        private void LoadPrefabs() {
            // Load toggle
            SettingsItemToggle toggle = GameObject.FindObjectOfType<SettingsItemToggle>(true);
            togglePrefab = Instantiate(toggle.gameObject, prefabHolder.transform);
            togglePrefab.SetActive(false);
            togglePrefab.name = "ModSettingsItemToggle";
            EnableAllChildComponents(togglePrefab);

            // Load slider
            SettingsItemSlider slider = GameObject.FindObjectOfType<SettingsItemSlider>(true);
            sliderPrefab = Instantiate(slider.gameObject, prefabHolder.transform);
            sliderPrefab.SetActive(false);
            sliderPrefab.name = "ModSettingsItemSlider";
            EnableAllChildComponents(sliderPrefab);

            // Load list
            SettingsItemList list = GameObject.FindObjectOfType<SettingsItemList>(true);
            listPrefab = Instantiate(list.gameObject, prefabHolder.transform);
            listPrefab.SetActive(false);
            listPrefab.name = "ModSettingsItemList";
            EnableAllChildComponents(listPrefab);

            Plugin.LOG.LogInfo("Loaded UI prefabs");
        }


        // for some reason some of the components are disabled
        // the unity editor disagrees
        // couldn't find which script enables or disables them at runtime
        // so we are doing this manually
        private void EnableAllChildComponents(GameObject gameObject) {
            foreach(MonoBehaviour mb in gameObject.GetComponents<MonoBehaviour>()) {
                mb.enabled = true;
            }
            foreach(Transform child in gameObject.transform) {
                EnableAllChildComponents(child.gameObject);
            }
        }
    }


}
