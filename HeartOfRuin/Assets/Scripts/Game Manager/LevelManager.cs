using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    private const string LoadingScreenSceneName = "LoadingScreen";

    // Dummy MonoBehaviour to execute the Coroutine
    private class CoroutineRunner : MonoBehaviour { }

    public static void LoadSceneAdditiveWithLoadingScreen(string sceneName)
    {
        Debug.Log($"Starting loading sequence for scene: {sceneName}");
        
        // Spawn a temporary runner to process the coroutine so we avoid needing an instance of LevelManager
        GameObject runnerObj = new GameObject("[LevelManager_CoroutineRunner]");
        DontDestroyOnLoad(runnerObj);
        CoroutineRunner runner = runnerObj.AddComponent<CoroutineRunner>();
        
        runner.StartCoroutine(LoadSceneSequence(sceneName, runnerObj));
    }

    private static IEnumerator LoadSceneSequence(string newSceneName, GameObject runnerObj)
    {
        string currentMainScene = SceneManager.GetActiveScene().name;

        // 1. Load the loading screen additively
        Debug.Log("Loading loading screen...");
        AsyncOperation loadLoadingScreen = SceneManager.LoadSceneAsync(LoadingScreenSceneName, LoadSceneMode.Additive);
        while (!loadLoadingScreen.isDone)
        {
            yield return null;
        }

        // Set the loading screen as active so we can safely unload the previous main scene
        SceneManager.SetActiveScene(SceneManager.GetSceneByName(LoadingScreenSceneName));   

        // 2. Unload the current main scene
        Debug.Log("Unloading current scene...");
        AsyncOperation unloadCurrentScene = SceneManager.UnloadSceneAsync(currentMainScene);
        while (!unloadCurrentScene.isDone)
        {
            yield return null;
        }
        
        // 3. Load the new scene additively
        Debug.Log("Loading new scene...");
        AsyncOperation loadNewScene = SceneManager.LoadSceneAsync(newSceneName, LoadSceneMode.Additive);
        while (!loadNewScene.isDone)
        {
            yield return null;
        }

        // Set the newly loaded scene as the active scene
        SceneManager.SetActiveScene(SceneManager.GetSceneByName(newSceneName));

        // 4. Unload the loading screen
        Debug.Log("Unloading loading screen...");
        AsyncOperation unloadLoadingScreen = SceneManager.UnloadSceneAsync(LoadingScreenSceneName);
        while (!unloadLoadingScreen.isDone)
        {
            yield return null;
        }

        // Finished execution, safely destroy the temporary runner
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
            Debug.Log("No more levels to load. Returning to Main Menu.");
            LoadMainMenu();
        }
    }

    public static void LoadWinScreen()
    {
        LoadSceneAdditiveWithLoadingScreen("WinScreen");
    }   
}
