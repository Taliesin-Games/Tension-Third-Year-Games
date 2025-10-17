using UnityEngine;

public class MainMenu : MonoBehaviour
{
    //[SerializeField] GameObject leaderboardUI;
    public void StartGame()
    {
        LevelManager.LoadFirstLevel();
    }

    public void OpenOptions()
    {
        LevelManager.LoadOptions();
    }

    //public void ToggleLeaderboard()
    //{
    //    leaderboardUI.SetActive(!leaderboardUI.activeSelf);

    //    if (leaderboardUI.activeSelf) 
    //    {
    //        LeaderboardManager.Instance.GetScores();
    //    }
    //    else
    //    {
    //        leaderboardUI.GetComponent<LeaderboardUI>().ClearLeaderboard();
    //    }
    //}
}
