using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance; // Singleton instance of the LevelManager class

    private const string LoadingScreenSceneName = "LoadingScreen";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this; // Assign the singleton instance
            
            // ENSURE the LevelManager isn't destroyed when its parent scene unloads!
            // Makes the LevelManager persist across all scenes.
            transform.SetParent(null); // DontDestroyOnLoad only works on root objects
            DontDestroyOnLoad(gameObject); 
        }
        else
        {
            Debug.LogWarning("Multiple LevelManager instances detected! Destroying duplicate.");
            Destroy(gameObject); // Ensure only one instance exists
            return;
        }
    }

    // New generic loading method that triggers the sequence
    public static void LoadSceneAdditiveWithLoadingScreen(string sceneName)
    {
        if (Instance != null)
        {
            Debug.Log($"Starting loading sequence for scene: {sceneName}");
            Instance.StartCoroutine(Instance.LoadSceneSequence(sceneName));
        }
        else
        {
            Debug.LogError("No LevelManager instance found to start loading sequence.");
            SceneManager.LoadScene(sceneName); // Fallback
        }
    }

    private IEnumerator LoadSceneSequence(string newSceneName)
    {
        string currentMainScene = SceneManager.GetActiveScene().name;

        // 1. Load the loading screen additively
        AsyncOperation loadLoadingScreen = SceneManager.LoadSceneAsync(LoadingScreenSceneName, LoadSceneMode.Additive);
        while (!loadLoadingScreen.isDone)
        {
            yield return null;
        }

        // Set the loading screen as active so we can safely unload the previous main scene
        SceneManager.SetActiveScene(SceneManager.GetSceneByName(LoadingScreenSceneName));   

        // 2. Unload the current main scene
        AsyncOperation unloadCurrentScene = SceneManager.UnloadSceneAsync(currentMainScene);
        while (!unloadCurrentScene.isDone)
        {
            yield return null;
        }

        // 3. Load the new scene additively
        AsyncOperation loadNewScene = SceneManager.LoadSceneAsync(newSceneName, LoadSceneMode.Additive);
        while (!loadNewScene.isDone)
        {
            yield return null;
        }

        // Set the newly loaded scene as the active scene
        SceneManager.SetActiveScene(SceneManager.GetSceneByName(newSceneName));

        // 4. Unload the loading screen
        AsyncOperation unloadLoadingScreen = SceneManager.UnloadSceneAsync(LoadingScreenSceneName);
        while (!unloadLoadingScreen.isDone)
        {
            yield return null;
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
            // Because we don't have the scene path easily available to get the string name,
            // we will need to load index using string path workaround or manage paths. 
            // In Unity 5.3+, we can bypass by grabbing the scene path.
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
