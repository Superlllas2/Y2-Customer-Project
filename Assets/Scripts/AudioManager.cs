using System;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [Header("Audio Source")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] public AudioSource SFXSource; // Keep this public to access it in Footsteps

    [Header("Audio Clip")]
    public AudioClip background;
    public AudioClip step; // Walking sound
    public AudioClip generator;
    public AudioClip generatorPut;
    public AudioClip pipe;
    public AudioClip run; // Running sound

    private void Start()
    {
        musicSource.clip = background;
        musicSource.Play();
    }

    public void PlaySFX(AudioClip clip)
    {
        SFXSource.PlayOneShot(clip);
    }

    public void StopSFX()
    {
        if (SFXSource.isPlaying)
        {
            SFXSource.Stop(); // Stop the currently playing sound effect
        }
    }
}
