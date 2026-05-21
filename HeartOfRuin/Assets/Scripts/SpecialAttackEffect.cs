using UnityEngine;
using UnityEngine.VFX;

public class SpecialAttackEffect : MonoBehaviour
{
    [SerializeField] ParticleSystem[] particleSystems;
    [SerializeField] VisualEffect[] visualEffects;
    [SerializeField] AudioClip[] soundEffects;

    private void Awake()
    {
        Play();
    }
    public void Play()
    {
        PlayParticles();
        PlayVisualEffect();
        PlaySoundEffects();
    }

    void PlayParticles()
    {
        foreach (var ps in particleSystems)
        {
            if(ps != null) ps.Play();
        }
    }
    void PlayVisualEffect()
    {
        foreach (var ve in visualEffects)
        {
            if(ve != null) ve.Play();
        }
    }
    void PlaySoundEffects()
    {
        foreach (var se in soundEffects)
        {
            if(se != null) AudioSource.PlayClipAtPoint(se, transform.position, OptionsManager.SFXVolume);
        }
    }
}
