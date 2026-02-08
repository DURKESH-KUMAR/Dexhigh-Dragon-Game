using UnityEngine;
using System.Collections.Generic;

public class AudioManager : Singleton<AudioManager>
{
    [Header("Audio Sources")]
    public AudioSource musicSource;
    public AudioSource sfxSource;
    public AudioSource ambientSource;
    
    [Header("Audio Clips")]
    public AudioClip battleMusic;
    public AudioClip victoryMusic;
    public List<AudioClip> fireSounds;
    public List<AudioClip> hitSounds;
    public List<AudioClip> roarSounds;
    
    [Header("Settings")]
    public float musicVolume = 0.5f;
    public float sfxVolume = 1f;
    public float ambientVolume = 0.3f;
    
    private Dictionary<string, AudioClip> audioClips = new Dictionary<string, AudioClip>();
    
    protected override void Awake()
    {
        base.Awake();
        
        InitializeAudio();
    }
    
    void InitializeAudio()
    {
        // Setup audio sources
        if (musicSource == null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.volume = musicVolume;
        }
        
        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.volume = sfxVolume;
        }
        
        if (ambientSource == null)
        {
            ambientSource = gameObject.AddComponent<AudioSource>();
            ambientSource.loop = true;
            ambientSource.volume = ambientVolume;
        }
        
        // Start battle music
        PlayBattleMusic();
    }
    
    public void PlayBattleMusic()
    {
        if (musicSource && battleMusic)
        {
            musicSource.clip = battleMusic;
            musicSource.Play();
        }
    }
    
    public void PlayVictorySound()
    {
        if (musicSource && victoryMusic)
        {
            musicSource.clip = victoryMusic;
            musicSource.Play();
        }
    }
    
    public void PlayFireSound()
    {
        if (fireSounds.Count > 0 && sfxSource)
        {
            AudioClip clip = fireSounds[Random.Range(0, fireSounds.Count)];
            sfxSource.PlayOneShot(clip);
        }
    }
    
    public void PlayHitSound()
    {
        if (hitSounds.Count > 0 && sfxSource)
        {
            AudioClip clip = hitSounds[Random.Range(0, hitSounds.Count)];
            sfxSource.PlayOneShot(clip);
        }
    }
    
    public void PlayRoarSound()
    {
        if (roarSounds.Count > 0 && sfxSource)
        {
            AudioClip clip = roarSounds[Random.Range(0, roarSounds.Count)];
            sfxSource.PlayOneShot(clip);
        }
    }
    
    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        if (musicSource)
            musicSource.volume = musicVolume;
    }
    
    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        if (sfxSource)
            sfxSource.volume = sfxVolume;
    }
}