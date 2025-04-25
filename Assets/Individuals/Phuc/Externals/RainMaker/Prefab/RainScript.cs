//
// Rain Maker (c) 2015 Digital Ruby, LLC
// http://www.digitalruby.com
//

using UnityEngine;
using System.Collections;

namespace DigitalRuby.RainMaker
{
    public class RainScript : BaseRainScript
    {
        [Tooltip("The height above the camera that the rain will start falling from")]
        public float RainHeight = 25.0f;

        [Tooltip("How far the rain particle system is ahead of the player")]
        public float RainForwardOffset = -7.0f;

        [Tooltip("The top y value of the mist particles")]
        public float RainMistHeight = 3.0f;

        // Audio source for rain sound (assumed to be set in BaseRainScript or attached to GameObject)
        private AudioSource rainAudioSource;
        private float rainOriginalVolume;
        private AudioManager audioManager; // Cached reference to avoid null issues

        private void UpdateRain()
        {
            // Keep rain and mist above the player
            if (RainFallParticleSystem != null)
            {
                if (FollowCamera)
                {
                    var s = RainFallParticleSystem.shape;
                    s.shapeType = ParticleSystemShapeType.ConeVolume;
                    RainFallParticleSystem.transform.position = Camera.transform.position;
                    RainFallParticleSystem.transform.Translate(0.0f, RainHeight, RainForwardOffset);
                    RainFallParticleSystem.transform.rotation = Quaternion.Euler(0.0f, Camera.transform.rotation.eulerAngles.y, 0.0f);
                    if (RainMistParticleSystem != null)
                    {
                        var s2 = RainMistParticleSystem.shape;
                        s2.shapeType = ParticleSystemShapeType.Hemisphere;
                        Vector3 pos = Camera.transform.position;
                        pos.y += RainMistHeight;
                        RainMistParticleSystem.transform.position = pos;
                    }
                }
                else
                {
                    var s = RainFallParticleSystem.shape;
                    s.shapeType = ParticleSystemShapeType.Box;
                    if (RainMistParticleSystem != null)
                    {
                        var s2 = RainMistParticleSystem.shape;
                        s2.shapeType = ParticleSystemShapeType.Box;
                        Vector3 pos = RainFallParticleSystem.transform.position;
                        pos.y += RainMistHeight;
                        pos.y -= RainHeight;
                        RainMistParticleSystem.transform.position = pos;
                    }
                }
            }
        }

        protected override void Start()
        {
            base.Start();

            // Initialize rain audio source
            rainAudioSource = GetComponent<AudioSource>();
            if (rainAudioSource == null)
            {
                Debug.LogWarning("No AudioSource found on RainScript GameObject. Checking BaseRainScript for audio source.");
                // Optionally, add logic to access BaseRainScript's AudioSource if available
                // Example: rainAudioSource = base.rainSound; // If BaseRainScript has a protected AudioSource field
            }
            else
            {
                rainOriginalVolume = rainAudioSource.volume;
                Debug.Log($"Rain AudioSource initialized with volume: {rainOriginalVolume}");
                
                // Apply initial mute state from PlayerPrefs
                bool isMuted = PlayerPrefs.GetInt("MuteState", 0) == 1;
                rainAudioSource.volume = isMuted ? 0f : rainOriginalVolume;
            }

            // Cache AudioManager reference with delayed initialization
            StartCoroutine(InitializeAudioManager());
        }

        private IEnumerator InitializeAudioManager()
        {
            // Wait until AudioManager is available
            while (AudioManager.Instance == null)
            {
                Debug.Log("Waiting for AudioManager to initialize...");
                yield return new WaitForSeconds(0.1f);
            }

            audioManager = AudioManager.Instance;
            if (audioManager != null)
            {
                audioManager.OnMuteStateChanged += UpdateRainAudioMuteState;
                Debug.Log("Successfully subscribed to AudioManager.OnMuteStateChanged");
            }
            else
            {
                Debug.LogError("AudioManager.Instance is null after waiting. Rain sound mute functionality will not work.");
            }
        }

        protected void OnDestroy() 
        {
            
            // Unsubscribe from AudioManager events using cached reference
            if (audioManager != null)
            {
                audioManager.OnMuteStateChanged -= UpdateRainAudioMuteState;
                Debug.Log("Unsubscribed from AudioManager.OnMuteStateChanged");
            }
        }

        private void UpdateRainAudioMuteState(bool isMuted)
        {
            if (rainAudioSource != null)
            {
                rainAudioSource.volume = isMuted ? 0f : rainOriginalVolume;
                Debug.Log($"Rain audio volume set to: {rainAudioSource.volume}");
            }
        }

        protected override void Update()
        {
            base.Update();
            UpdateRain();
        }
    }
}