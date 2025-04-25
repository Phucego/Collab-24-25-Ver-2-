using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : Singleton<AudioManager>
{
    public static AudioManager Instance { get; private set; }

    [Header("Background Ambience")]
    public AudioClip[] backgroundAmbienceTracks;

    [Header("Sound Effects")]
    public AudioClip[] soundEffects;

    private List<AudioSource> bgAmbienceSources = new List<AudioSource>();
    private AudioSource sfxSource;
    private List<float> bgAmbienceOriginalVolumes = new List<float>();
    private float sfxOriginalVolume;

    // Event to notify when mute state changes
    public event Action<bool> OnMuteStateChanged;

    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Persist across scenes
            Debug.Log($"AudioManager Instance initialized: {gameObject.name}");
        }
        else
        {
            Debug.LogWarning($"Duplicate AudioManager detected on {gameObject.name}. Destroying this instance.");
            Destroy(gameObject);
            return;
        }

        // Create AudioSource components for ambience
        foreach (AudioClip clip in backgroundAmbienceTracks)
        {
            AudioSource source = gameObject.AddComponent<AudioSource>();
            source.clip = clip;
            source.loop = true;
            source.playOnAwake = false;
            source.volume = 0.4f;
            bgAmbienceOriginalVolumes.Add(source.volume);
            bgAmbienceSources.Add(source);
        }

        // Create AudioSource for sound effects
        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxOriginalVolume = 1f;
        sfxSource.volume = sfxOriginalVolume;

        Debug.Log($"Initialized {bgAmbienceSources.Count} ambience sources and SFX source: {sfxSource != null}");
    }

    private void Start()
    {
        PlayAllAmbience();
    }

    private void PlayAllAmbience()
    {
        foreach (AudioSource source in bgAmbienceSources)
        {
            source.Play();
        }
    }

    public void PlaySoundEffect(string soundName)
    {
        AudioClip clip = GetSoundEffectByName(soundName);

        if (clip != null)
        {
            sfxSource.PlayOneShot(clip);
        }
        else
        {
            Debug.LogWarning("Sound not found: " + soundName);
        }
    }

    private AudioClip GetSoundEffectByName(string name)
    {
        foreach (AudioClip clip in soundEffects)
        {
            if (clip.name == name)
            {
                return clip;
            }
        }
        return null;
    }
    
    public void SetAudioPaused(bool isMuted)
    {
        Debug.Log($"Setting audio paused: {isMuted}");
        for (int i = 0; i < bgAmbienceSources.Count; i++)
        {
            bgAmbienceSources[i].volume = isMuted ? 0f : bgAmbienceOriginalVolumes[i];
            Debug.Log($"Ambience {i} volume: {bgAmbienceSources[i].volume}");
        }
        sfxSource.volume = isMuted ? 0f : sfxOriginalVolume;
        Debug.Log($"SFX volume: {sfxSource.volume}");

        // Notify subscribers of mute state change
        OnMuteStateChanged?.Invoke(isMuted);
    }
    
    // OPTIONAL: If using an AudioMixer
    public void SetAudioMixerUnscaled(AudioMixer audioMixer)
    {
        if (audioMixer != null)
        {
            audioMixer.updateMode = AudioMixerUpdateMode.UnscaledTime;
        }
    }
}