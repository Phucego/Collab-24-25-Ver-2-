using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : Singleton<AudioManager>
{
    public static AudioManager Instance;

    [Header("Background Ambience")]
    public AudioClip[] backgroundAmbienceTracks;

    [Header("Sound Effects")]
    public AudioClip[] soundEffects;

    private List<AudioSource> bgAmbienceSources = new List<AudioSource>();
    private AudioSource sfxSource;

    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
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
            source.ignoreListenerPause = true;
            bgAmbienceSources.Add(source);
        }

        // Create AudioSource for sound effects
        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.volume = 1f;
        sfxSource.ignoreListenerPause = true;
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
        AudioListener.pause = isMuted;
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