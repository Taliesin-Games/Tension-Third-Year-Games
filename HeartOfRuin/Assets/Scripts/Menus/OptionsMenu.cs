using UnityEngine;
using UnityEngine.UI;

public class OptionsMenu : MonoBehaviour
{
    [SerializeField] Slider masterVolumeSlider;
    [SerializeField] Slider musicVolumeSlider;
    [SerializeField] Slider sfxVolumeSlider;
    private void Start()
    {
        OptionsManager.LoadOptions();
        masterVolumeSlider.value = OptionsManager.MasterVolume;
        musicVolumeSlider.value = OptionsManager.MusicVolume;
        sfxVolumeSlider.value = OptionsManager.SFXVolume;
    }
    public void ReturnToStart()
    {
        OptionsManager.SaveOptions();
        LevelManager.LoadMainMenu();
    }
    public void SetMasterVolume(float value)
    {
        OptionsManager.MasterVolume = value;
    }
    public void SetMusicVolume(float value)
    {
        OptionsManager.MusicVolume = value;
    }
    public void SetSFXVolume(float value)
    {
        OptionsManager.SFXVolume = value;
    }
    
}
