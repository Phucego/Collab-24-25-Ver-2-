using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class LightningStrikeEvent : MonoBehaviour
{
    [Header("Event Settings")]
    [SerializeField] private float lightningChancePerWave = 0.3f; // Example: 30% chance per wave
    [SerializeField] private GameObject lightningEffectPrefab; // Visual effect for lightning

    public UnityEvent<string, int> OnLightningStrike; // Event for UI notification (used in UIManager)

    private WaveManager waveManager;

    private void Awake()
    {
        // Find WaveManager safely
        waveManager = FindObjectOfType<WaveManager>();
        if (waveManager == null)
        {
            Debug.LogError("LightningStrikeEvent: WaveManager not found in scene!");
            enabled = false; // Disable script to prevent further errors
        }
    }

    private void Start()
    {
        // Ensure WaveManager is initialized before hooking events
        if (waveManager != null)
        {
            HookWaveEvents();
        }
        else
        {
            Debug.LogWarning("LightningStrikeEvent: Cannot hook events due to missing WaveManager.");
        }
    }

    private void HookWaveEvents()
    {
        // Subscribe to WaveManager events safely
        if (waveManager != null)
        {
            waveManager.OnWaveComplete += OnWaveCompleteHandler;
            // Start coroutine to monitor wave starts
            StartCoroutine(WatchForWaveStart());
        }
    }

    private void OnWaveCompleteHandler()
    {
        // Example: Trigger lightning chance check after wave completes
        if (Random.value < lightningChancePerWave)
        {
            TriggerLightningStrike();
        }
    }

    private IEnumerator WatchForWaveStart()
    {
        while (true)
        {
            // Wait until WaveManager is ready and a wave is starting
            if (waveManager != null && waveManager._curData != null && waveManager._curData.Count > 0)
            {
                // Check if a wave is starting (e.g., _isSpawning becomes true)
                if (waveManager._isSpawning)
                {
                    // Perform actions when wave starts (e.g., prepare lightning)
                    Debug.Log($"Wave {waveManager._curWave + 1} started. Monitoring for lightning opportunity.");
                    // Example: Wait for wave to progress or complete
                    while (waveManager._isSpawning)
                    {
                        yield return null; // Wait until wave spawning ends
                    }
                }
            }
            else
            {
                Debug.LogWarning("WaveManager data not ready. Retrying...");
            }

            // Wait a frame before checking again
            yield return null;
        }
    }

    private void TriggerLightningStrike()
    {
        // Example: Spawn lightning effect and notify UI
        if (lightningEffectPrefab != null)
        {
            Instantiate(lightningEffectPrefab, Vector3.zero, Quaternion.identity);
        }

        // Notify UI (e.g., UIManager) with path ID (example: -1 for no specific path)
        OnLightningStrike?.Invoke("Lightning", -1);
    }

    private void OnDestroy()
    {
        // Clean up event subscriptions
        if (waveManager != null)
        {
            waveManager.OnWaveComplete -= OnWaveCompleteHandler;
        }
    }
}