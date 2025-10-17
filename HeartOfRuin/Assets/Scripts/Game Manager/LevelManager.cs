using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance; // Singleton instance of the LevelManager class

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this; // Assign the singleton instance
        }
        else
        {
            Debug.LogWarning("Multiple LevelManager instances detected! Destroying duplicate.");
            Destroy(gameObject); // Ensure only one instance exists
            return;
        }
    }


    public static void LoadMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
    public static void LoadOptions()
    {
        SceneManager.LoadScene("OptionsMenu");
    }
    public static void LoadFirstLevel()
    {
        MusicManager musicManager = FindFirstObjectByType<MusicManager>();
        //Check if the music manager is valid and that it doesnty have a game manager component attached
        if (musicManager != null && musicManager.GetComponent<GameManager>() == null)
        {
            Destroy(musicManager.gameObject);
            MusicManager.Instance = null; // Reset the singleton instance
        }


        SceneManager.LoadScene("Level 1");
    }
    
    public static void LoadGameOver()
    {
        SceneManager.LoadScene("GameOver");
    }   
    public static void LoadNextLevel()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        int nextSceneIndex = currentSceneIndex + 1;
        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextSceneIndex);
        }
        else
        {
            Debug.Log("No more levels to load. Returning to Main Menu.");
            LoadMainMenu();
        }
    }

    public static void LoadWinScreen()
    {
        SceneManager.LoadScene("WinScreen");
    }   
}
