using UnityEngine;
using UnityEngine.SceneManagement;

namespace MeteorCore.Setting;

public class SettingsUIPrefabs : MonoBehaviour {
    private static SettingsUIPrefabs _instance;
    public static SettingsUIPrefabs Instance {
        get {
            if(_instance == null)
                _instance = FindObjectOfType<SettingsUIPrefabs>();
            return _instance;
        }
    }
    private void Awake() {
        if(_instance == null)
            _instance = this;
        if(_instance != this)
            Destroy(this);
    }

    public static GameObject LabelPrefab { get; private set; }
    public static GameObject TogglePrefab { get; private set; }
    public static GameObject SliderPrefab { get; private set; }
    public static GameObject ListPrefab { get; private set; }
    private bool firstSceneLoad = true;

    public static GameObject Create() {
        GameObject prefabs = new GameObject("SettingsUIPrefabs");
        prefabs.transform.SetParent(Mgr_PluginSettings.Instance.transform);
        prefabs.AddComponent<RectTransform>();
        prefabs.AddComponent<SettingsUIPrefabs>();
        return prefabs;
    }

    public void SceneLoaded(Scene scene, LoadSceneMode mode) {
        if(this.firstSceneLoad && Mgr_PluginSettings.SceneHasSettingsMenu) {
            this.firstSceneLoad = false;
            this.CopyBaseUIITems();
        }
    }

    // Copies all base UI item types for usage by othger plugins
    private void CopyBaseUIITems() {
        GameObject prefabs = new GameObject("SettingsUIPrefabs");
        prefabs.transform.SetParent(this.transform);

        // Find toggle
        SettingsItemToggle toggle = GameObject.FindObjectOfType<SettingsItemToggle>(true);
        if(toggle != null) {
            TogglePrefab = Instantiate(toggle.gameObject, prefabs.transform);
            TogglePrefab.SetActive(false);
            TogglePrefab.name = "PluginSettingsItemToggle";
            //this.EnableAllChildComponents(TogglePrefab);
        } else {
            Plugin.Logger.LogError("Could not find toggle prefab");
        }

        // Copy label from toggle
        StaticStringLocaliser localiser = TogglePrefab.GetComponentInChildren<StaticStringLocaliser>();
        if(localiser != null) {
            LabelPrefab = Instantiate(localiser.gameObject, prefabs.transform);
            LabelPrefab.SetActive(false);
            LabelPrefab.name = "PluginSettingsLabel";
            localiser.BaseText = "LocaliserUnsetText";
            localiser.Dictionary = "LocaliserUnsetDictionary";
        } else {
            Plugin.Logger.LogError("Could not find label prefab");
        }

        // Find slider
        SettingsItemSlider slider = GameObject.FindObjectOfType<SettingsItemSlider>(true);
        if(slider != null) {
            SliderPrefab = Instantiate(slider.gameObject, prefabs.transform);
            SliderPrefab.SetActive(false);
            SliderPrefab.name = "PluginSettingsItemSlider";
            //this.EnableAllChildComponents(SliderPrefab);
        } else {
            Plugin.Logger.LogError("Could not find slider prefab");
        }

        // Find list
        SettingsItemList list = GameObject.FindObjectOfType<SettingsItemList>(true);
        if(list != null) {
            ListPrefab = Instantiate(list.gameObject, prefabs.transform);
            ListPrefab.SetActive(false);
            ListPrefab.name = "PluginSettingsItemList";
            //this.EnableAllChildComponents(ListPrefab);
        } else {
            Plugin.Logger.LogError("Could not find list prefab");
        }

        Plugin.Logger.LogInfo("Loaded base setting UI prefabs");

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
            this.EnableAllChildComponents(child.gameObject);
        }
    }
}
