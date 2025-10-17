using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance;

    [SerializeField] AudioClip backgroundMusic;

    AudioSource audioSource;


    void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        audioSource = GetComponent<AudioSource>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        audioSource.volume = OptionsManager.MusicVolume;
    }

    public void PlayMusic()
    {
        if (audioSource.clip == backgroundMusic && audioSource.isPlaying) return;

        audioSource.clip = backgroundMusic;
        audioSource.loop = true;
        audioSource.Play();
        

    }
    
    public void SetVolume(float volume)
    {
        audioSource.volume = volume;
    }
}
