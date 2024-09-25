using System;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [Header("Audio Source")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource SFXSource;

    [Header("Audio Clip")]
    public AudioClip background;
    public AudioClip step;
    public AudioClip collect;
    public AudioClip jump;
    public AudioClip run;

    private void Start()
    {
        musicSource.clip = background;
        musicSource.Play();
    }

    public void PlaySFX(AudioClip clip)
    {
        SFXSource.PlayOneShot(clip);
    }

    // Implemented method to stop sound effects
    public void StopSFX()
    {
        if (SFXSource.isPlaying)
        {
            SFXSource.Stop(); // Stop the currently playing sound effect
        }
    }
}
