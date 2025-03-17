using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : Singleton<AudioManager>
{
    public static AudioManager Instance;

    [Header("Background Music")]
    public AudioClip backgroundMusic;

    [Header("Sound Effects")]
    public AudioClip[] soundEffects;

    private AudioSource bgMusicSource;
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

        // Create AudioSource components
        bgMusicSource = gameObject.AddComponent<AudioSource>();
        sfxSource = gameObject.AddComponent<AudioSource>();

        // Sound effects volume adjustment
        sfxSource.volume = 1f;

        // Background music settings
        bgMusicSource.loop = true;
        bgMusicSource.clip = backgroundMusic;
        bgMusicSource.playOnAwake = true;
        bgMusicSource.volume = 0.4f;

        // Ensure audio plays normally even when time scale changes
        bgMusicSource.ignoreListenerPause = true;
        sfxSource.ignoreListenerPause = true;
    }

    private void Start()
    {
        // Play background music
        bgMusicSource.Play();
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
