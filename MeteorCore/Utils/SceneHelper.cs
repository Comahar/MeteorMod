using UnityEngine.SceneManagement;
using UnityEngine;

namespace MeteorCore;

public static class SceneHelper {
    public static void Init() {
        SceneManager.sceneLoaded += SceneLoaded;
    }

    private static void SceneLoaded(Scene scene, LoadSceneMode mode) {
        IsTitleScene = scene.name == "Title";
        IsSplashScene = scene.name == "Splash";

        var gameplayUI = GameObject.Find("GameplayUI Root");
        IsPerformenceScene = gameplayUI != null;

        var minigameController = GameObject.Find("MG_Basics/Controller");
        IsMinigameScene = minigameController != null;

        // TODO
        IsLNLScene = false;
        // i don't know how lnl will effect this
        IsGameScene = !IsSplashScene && !IsTitleScene && !IsPerformenceScene && !IsMinigameScene;
    }
    public static bool IsSplashScene { get; private set; }
    public static bool IsTitleScene { get; private set; }
    public static bool IsPerformenceScene { get; private set; }
    public static bool IsMinigameScene { get; private set; }
    public static bool IsGameScene { get; private set; } // Standard game scene with dialogue and not lnl
    public static bool IsLNLScene { get; private set; }
}
