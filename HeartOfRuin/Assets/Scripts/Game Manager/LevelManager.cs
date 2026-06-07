using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    private const string LoadingScreenSceneName = "LoadingScreen";

    private static float s_minimumLoadingDelay = 2.0f;
    private static float s_LoadingScreenSettleDelay = 5.0f;

    private class CoroutineRunner : MonoBehaviour { }

    public static void LoadSceneAdditiveWithLoadingScreen(string sceneName)
    {
        Debug.Log($"[LevelManager] Preparing to load scene: {sceneName}");
        
        // Validation check to see if scenes exist in the build settings
        if (!Application.CanStreamedLevelBeLoaded(sceneName))
        {
            Debug.LogError($"[LevelManager] Cannot load scene '{sceneName}'. Is it added to the Build Settings?");
            return;
        }

        if (!Application.CanStreamedLevelBeLoaded(LoadingScreenSceneName))
        {
            Debug.LogError($"[LevelManager] Cannot load loading screen '{LoadingScreenSceneName}'. Is it added to the Build Settings?");
            return;
        }

        Debug.Log($"[LevelManager] Starting loading sequence for scene: {sceneName}");
        
        // Spawn a temporary runner to process the coroutine so we avoid needing an instance of LevelManager
        GameObject runnerObj = new GameObject("[LevelManager_CoroutineRunner]");
        DontDestroyOnLoad(runnerObj);
        
        
        CoroutineRunner runner = runnerObj.AddComponent<CoroutineRunner>();
        runner.StartCoroutine(LoadSceneSequence(sceneName, runnerObj));
    }

    private static IEnumerator LoadSceneSequence(string newSceneName, GameObject runnerObj)
    {
        float totalSequenceStartTime = Time.realtimeSinceStartup;
        float stepStartTime;

        string currentMainScene = SceneManager.GetActiveScene().name;
        Debug.Log($"[LevelManager] [{Time.realtimeSinceStartup:F2}s] Current active scene is: {currentMainScene}");

        // load the loading screen additively.
        stepStartTime = Time.realtimeSinceStartup;
        Debug.Log($"[LevelManager] [{stepStartTime:F2}s] Step 1: Loading loading screen '{LoadingScreenSceneName}'...");
        AsyncOperation loadLoadingScreen = SceneManager.LoadSceneAsync(LoadingScreenSceneName, LoadSceneMode.Additive);
        if (loadLoadingScreen == null)
        {
            Debug.LogError($"[LevelManager] Failed to start loading '{LoadingScreenSceneName}'. Aborting sequence.");
            Destroy(runnerObj);
            yield break;
        }

        while (!loadLoadingScreen.isDone)
        {
            yield return null;
        }
        Debug.Log($"[LevelManager] -> Loading screen loaded successfully in {Time.realtimeSinceStartup - stepStartTime:F3}s.");

        // Set the loading screen as active.
        Scene activeLoadingScreen = SceneManager.GetSceneByName(LoadingScreenSceneName);
        bool setActiveResult = SceneManager.SetActiveScene(activeLoadingScreen);
        Debug.Log($"[LevelManager] -> Setting active scene to '{LoadingScreenSceneName}': {(setActiveResult ? "SUCCESS" : "FAILED")}");

        // Unload the current main scene
        stepStartTime = Time.realtimeSinceStartup;
        Debug.Log($"[LevelManager] [{stepStartTime:F2}s] Step 2: Unloading current scene '{currentMainScene}'...");
        AsyncOperation unloadCurrentScene = SceneManager.UnloadSceneAsync(currentMainScene);
        if (unloadCurrentScene != null)
        {
            while (!unloadCurrentScene.isDone)
            {
                yield return null;
            }
            Debug.Log($"[LevelManager] -> Scene '{currentMainScene}' unloaded successfully in {Time.realtimeSinceStartup - stepStartTime:F3}s.");
        }
        else
        {
            Debug.LogWarning($"[LevelManager] -> Failed to unload '{currentMainScene}'. It might already be unloaded or invalid.");
        }
        
        // Load the new scene additively
        stepStartTime = Time.realtimeSinceStartup;
        Debug.Log($"[LevelManager] [{stepStartTime:F2}s] Step 3: Loading new scene '{newSceneName}'...");
        AsyncOperation loadNewScene = SceneManager.LoadSceneAsync(newSceneName, LoadSceneMode.Additive);
        if (loadNewScene == null)
        {
            Debug.LogError($"[LevelManager] Failed to start loading new scene '{newSceneName}'. Aborting sequence.");
            Destroy(runnerObj);
            yield break;
        }

        // Prevent the new scene from activating its objects and cameras immediately
        loadNewScene.allowSceneActivation = false;

        // Wait until Unity has finished bringing the scene into memory (halts at 0.9 progress)
        while (loadNewScene.progress < 0.9f)
        {
            yield return null;
        }

        // Perform the delay BEFORE allowing the level to activate and show on screen
        float elapsedSequenceTime = Time.realtimeSinceStartup - totalSequenceStartTime;
        if (elapsedSequenceTime < s_minimumLoadingDelay)
        {
            float remainingDelay = s_minimumLoadingDelay - elapsedSequenceTime;
            Debug.Log($"[LevelManager] Artificial Delay constraint not met. Delaying transition for an additional {remainingDelay:F2}s...");
            yield return new WaitForSecondsRealtime(remainingDelay);
        }

        // Now it's safe to activate the scene visually
        loadNewScene.allowSceneActivation = true;

        while (!loadNewScene.isDone)
        {
            yield return null;
        }
        Debug.Log($"[LevelManager] -> New scene '{newSceneName}' loaded successfully in {Time.realtimeSinceStartup - stepStartTime:F3}s.");

        // Set the newly loaded scene as the active scene
        Scene newActiveScene = SceneManager.GetSceneByName(newSceneName);
        bool setNewActiveResult = SceneManager.SetActiveScene(newActiveScene);
        Debug.Log($"[LevelManager] -> Setting active scene to '{newSceneName}': {(setNewActiveResult ? "SUCCESS" : "FAILED")}");

        // Since we are using additive loading, the loading screen is still active and covering the screen, leverage this with another artificial delay to allow the new scene to fully initialize.
        Debug.Log("[LevelManager] Holding loading screen briefly to allow new scene to settle...");
        yield return new WaitForSecondsRealtime(s_LoadingScreenSettleDelay);

        //Unload the loading screen post delay
        stepStartTime = Time.realtimeSinceStartup;
        Debug.Log($"[LevelManager] [{stepStartTime:F2}s] Step 4: Unloading loading screen '{LoadingScreenSceneName}'...");
        AsyncOperation unloadLoadingScreen = SceneManager.UnloadSceneAsync(LoadingScreenSceneName);
        if (unloadLoadingScreen != null)
        {
            while (!unloadLoadingScreen.isDone)
            {
                yield return null;
            }
            Debug.Log($"[LevelManager] -> Loading screen unloaded successfully in {Time.realtimeSinceStartup - stepStartTime:F3}s.");
        }
        else
        {
            Debug.LogWarning($"[LevelManager] -> Failed to unload loading screen. It might already be unloaded.");
        }

        float totalDuration = Time.realtimeSinceStartup - totalSequenceStartTime;
        Debug.Log($"[LevelManager] [{Time.realtimeSinceStartup:F2}s] Loading sequence complete. Total time taken: {totalDuration:F3}s.");

        // Finished execution, safely destroy the temporary runner (which also securely destroys the fallback camera)
        if (runnerObj != null)
        {
            Destroy(runnerObj);
        }
    }

    public static void LoadMainMenu()
    {
        LoadSceneAdditiveWithLoadingScreen("MainMenu");
    }

    public static void LoadOptions()
    {
        LoadSceneAdditiveWithLoadingScreen("OptionsMenu");
    }

    public static void LoadFirstLevel()
    {
        ClearSeparateMusicManager();
        LoadSceneAdditiveWithLoadingScreen("Level 1");
    }

    public static void LoadSafeHub()
    {
        ClearSeparateMusicManager();
        LoadSceneAdditiveWithLoadingScreen("SafeHub");
    }

    private static void ClearSeparateMusicManager()
    {
        MusicManager musicManager = FindFirstObjectByType<MusicManager>(); // Requires Unity 2023.1+ or use FindObjectOfType
        if (musicManager != null && musicManager.GetComponent<GameManager>() == null)
        {
            Destroy(musicManager.gameObject);
            MusicManager.Instance = null; // Reset the singleton instance
        }
    }

    public static void LoadGameOver()
    {
        LoadSceneAdditiveWithLoadingScreen("GameOver");
    }   

    public static void LoadNextLevel()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        int nextSceneIndex = currentSceneIndex + 1;

        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            string nextScenePath = SceneUtility.GetScenePathByBuildIndex(nextSceneIndex);
            string nextSceneName = System.IO.Path.GetFileNameWithoutExtension(nextScenePath);
            LoadSceneAdditiveWithLoadingScreen(nextSceneName);
        }
        else
        {
            Debug.Log("[LevelManager] No more levels to load. Returning to Main Menu.");
            LoadMainMenu();
        }
    }

    public static void LoadWinScreen()
    {
        LoadSceneAdditiveWithLoadingScreen("WinScreen");
    }   
}
