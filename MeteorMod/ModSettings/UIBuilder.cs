using UnityEngine;
using MelonLoader;

namespace MeteorMod.ModSettings {
    public class UIBuilder {

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        private GameObject prefabHolder;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        public static UIBuilder Instance {
            get {
                if(instance == null)
                    instance = new UIBuilder();
                return instance;
            }
        }
        private static UIBuilder? instance;

        internal GameObject? togglePrefab;
        internal GameObject? sliderPrefab;
        internal GameObject? listPrefab;

        public static UIBuilder Init() {
            instance = new UIBuilder();
            return instance;
        }

        bool firstSceneLoad = true;
        public void OnSceneWasLoaded(int buildIndex, string sceneName) {
            // only run on title scene and at first scene load
            if(sceneName == "Title" && firstSceneLoad) {
                firstSceneLoad = false;
                if(Instance.prefabHolder == null) {
                    Instance.prefabHolder = new GameObject("UIBuilderPrefabs");
                    GameObject.DontDestroyOnLoad(Instance.prefabHolder);
                }
                Instance.LoadPrefabs();
                MelonLogger.Msg("UIBuilder instance created");
            }
        }

        public void ReloadPrefabs() {
            MelonLogger.Msg("Reloading prefabs");
            DeletePrefabs();
            LoadPrefabs();
        }

        private void DeletePrefabs() {
            foreach(Transform child in prefabHolder.transform) {
                GameObject.Destroy(child.gameObject);
            }
        }

        private void LoadPrefabs() {

            // Load toggle
            SettingsItemToggle toggle = GameObject.FindObjectOfType<SettingsItemToggle>(true);
            togglePrefab = GameObject.Instantiate(toggle.gameObject, prefabHolder.transform);
            togglePrefab.SetActive(false);
            togglePrefab.name = "ModSettingsItemToggle";
            EnableAllChildComponents(togglePrefab);

            // Load slider
            SettingsItemSlider slider = GameObject.FindObjectOfType<SettingsItemSlider>(true);
            sliderPrefab = GameObject.Instantiate(slider.gameObject, prefabHolder.transform);
            sliderPrefab.SetActive(false);
            sliderPrefab.name = "ModSettingsItemSlider";
            EnableAllChildComponents(sliderPrefab);

            // Load list
            SettingsItemList list = GameObject.FindObjectOfType<SettingsItemList>(true);
            listPrefab = GameObject.Instantiate(list.gameObject, prefabHolder.transform);
            listPrefab.SetActive(false);
            listPrefab.name = "ModSettingsItemList";
            EnableAllChildComponents(listPrefab);

            MelonLogger.Msg("Loaded UI prefabs");
        }


        // for some reason some of the components are disabled
        // the unity editor disagrees
        // couldn't find which script enables them at runtime
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
