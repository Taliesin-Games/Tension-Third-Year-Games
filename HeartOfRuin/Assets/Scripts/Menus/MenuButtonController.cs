using UnityEngine;
using UnityEngine.UI;

public class MenuButtonController : MonoBehaviour
{
    [SerializeField] Button menuButton;
    [SerializeField] Button quitButton;
    
    void Start()
    {
        if (menuButton != null)
        {
            menuButton.onClick.AddListener(LevelManager.LoadMainMenu);
        }

        if (quitButton != null)
        {
            quitButton.onClick.AddListener(QuitGame);
        }
    }

    private void QuitGame()
    {
        Debug.Log("Quitting the game...");
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
