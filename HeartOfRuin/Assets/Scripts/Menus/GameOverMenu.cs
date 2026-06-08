using TMPro;
using UnityEngine;

public class GameOverMenu : MonoBehaviour
{
    [SerializeField] TMP_Text finalScoreText;
    [SerializeField] TMP_InputField nameInputField;

    private void OnEnable()
    {
        if (GameManager.Instance != null)
        {
            if (finalScoreText != null)
                finalScoreText.text = $"Final Score: 00000";
            if (nameInputField != null)
                nameInputField.text = OptionsManager.PlayerName;
        }
        else
        {
            if (finalScoreText != null)
                finalScoreText.text = "Final Score: 0";
            if (nameInputField != null)
                nameInputField.text = "";
        }
    }
    public void ReturnToMenu()
    {

        OptionsManager.PlayerName = nameInputField.text.Length > 0 ? nameInputField.text : "Player";
        OptionsManager.SaveOptions();
        
        LevelManager.LoadMainMenu();

    }
}
