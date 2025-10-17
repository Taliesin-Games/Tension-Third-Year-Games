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
            finalScoreText.text = $"Final Score: 00000";
            nameInputField.text = OptionsManager.PlayerName;
        }
        else
        {
            finalScoreText.text = "Final Score: 0";
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
