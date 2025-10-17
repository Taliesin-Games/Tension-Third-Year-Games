using UnityEngine;
using UnityEngine.SceneManagement;

public class OptionsManager : MonoBehaviour
{
    const string MASTER_VOLUME_PREF = "MasterVolume";   
    const string MUSIC_VOLUME_PREF = "MusicVolume";     
    const string SFX_VOLUME_PREF = "SFXVolume";         
    
    const string PLAYER_NAME_PREF = "PlayerName";

    static float masterVolume = 1.0f;
    static float musicVolume = 1.0f;
    static float sfxVolume = 1.0f;

    static string playerName = "Player";

    public static float MasterVolume
    {
        get => masterVolume;
        set
        {
            masterVolume = Mathf.Clamp01(value);
            AudioListener.volume = masterVolume;
            PlayerPrefs.SetFloat(MASTER_VOLUME_PREF, masterVolume);
        }
    }
    public static float MusicVolume
    {
        get => musicVolume;
        set
        {
            musicVolume = Mathf.Clamp01(value);
            MusicManager.Instance?.SetVolume(musicVolume);
            PlayerPrefs.SetFloat(MUSIC_VOLUME_PREF, musicVolume);
        } 
    }
    public static float SFXVolume
    {
        get => sfxVolume;
        set
        {
            sfxVolume = Mathf.Clamp01(value);
            PlayerPrefs.SetFloat(SFX_VOLUME_PREF, sfxVolume);

        } 
    }
    public static string PlayerName
    {
        get => playerName;
        set
        {
            playerName = value.Length > 0 ? value : "Player";
            PlayerPrefs.SetString(PLAYER_NAME_PREF, playerName);
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void LoadOptions()
    {
        masterVolume = PlayerPrefs.GetFloat(MASTER_VOLUME_PREF, 1.0f);
        musicVolume = PlayerPrefs.GetFloat(MUSIC_VOLUME_PREF, 1.0f);
        sfxVolume = PlayerPrefs.GetFloat(SFX_VOLUME_PREF, 1.0f);
        AudioListener.volume = masterVolume;
        MusicManager.Instance?.SetVolume(musicVolume);

        playerName = PlayerPrefs.GetString(PLAYER_NAME_PREF, "Player");
    }

    public static void SaveOptions()
    {   
        PlayerPrefs.Save();
    }
}
