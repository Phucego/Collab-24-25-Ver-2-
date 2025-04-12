using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class LightningStrikeEvent : MonoBehaviour
{
    [Header("Lightning Strike Settings")]
    [SerializeField] private SceneField targetScene;
    [SerializeField] private GameObject lightningEffectPrefab;

    public List<GameObject> placeableSpots = new List<GameObject>();
    private Dictionary<GameObject, GameObject> activeEffects = new Dictionary<GameObject, GameObject>();

    [Header("Events")]
    public UnityEvent<string, int> OnLightningStrike;
    
    private bool hasPlayedLightningSound = false;  // Flag to track sound play status
    private void Start()
    {
        StartCoroutine(InitializeLightningStrike());

        if (WaveManager.Instance != null)
        {
            HookWaveEvents();
        }
        else
        {
            StartCoroutine(WaitForWaveManager());
        }
    }

    private void OnDestroy()
    {
        if (WaveManager.Instance != null)
            WaveManager.Instance.OnWaveComplete -= StopLightning;
    }

    private IEnumerator WaitForWaveManager()
    {
        while (WaveManager.Instance == null)
            yield return null;

        HookWaveEvents();
    }

    private void HookWaveEvents()
    {
        WaveManager.Instance.OnWaveComplete += StopLightning;
        StartCoroutine(WatchForWaveStart());
    }

    private IEnumerator InitializeLightningStrike()
    {
        yield return new WaitForSeconds(1f);

        if (SceneManager.GetActiveScene().name != targetScene) yield break;

        GameObject[] placeholders = GameObject.FindGameObjectsWithTag("Placeable");
        placeableSpots.AddRange(placeholders);
    }

    private IEnumerator WatchForWaveStart()
    {
        bool wasSpawning = false;

        while (true)
        {
            if (WaveManager.Instance != null)
            {
                bool isSpawning = typeof(WaveManager).GetField("_isSpawning", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(WaveManager.Instance) as bool? ?? false;

                if (isSpawning && !wasSpawning)
                {
                    int currentWave = UIManager.Instance != null ? UIManager.Instance.currentWave : 1;

                    // Filter out placeholders that already have towers
                    List<GameObject> availableSpots = new List<GameObject>();
                    foreach (var spot in placeableSpots)
                    {
                        // Assuming PlaceholderID or another component can help identify if a tower is present
                        if (spot.GetComponent<TowerController>() == null) // No tower on this spot
                        {
                            availableSpots.Add(spot);
                        }
                    }

                    // Ensure we don't try to pick more unique spots than available
                    int strikes = Mathf.Min(currentWave, availableSpots.Count);

                    List<GameObject> shuffled = new List<GameObject>(availableSpots);
                    ShuffleList(shuffled);

                    for (int i = 0; i < strikes; i++)
                    {
                        StartLightning(shuffled[i]);
                    }
                }

                wasSpawning = isSpawning;
            }

            yield return new WaitForSeconds(0.5f);
        }
    }

    private void ShuffleList<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int rnd = Random.Range(0, i + 1);
            T temp = list[i];
            list[i] = list[rnd];
            list[rnd] = temp;
        }
    }



    private void StartLightning(GameObject targetSpot)
    {
        if (targetSpot == null) return;

        if (targetSpot.TryGetComponent(out Collider col))
            col.enabled = false;

        GameObject lightningEffect = Instantiate(lightningEffectPrefab, targetSpot.transform.position + Vector3.up * 1.5f, Quaternion.Euler(-90f, 0f, 0f));

        // Play lightning sound only once per strike event
        if (!hasPlayedLightningSound)
        {
            AudioManager.Instance.PlaySoundEffect("LightningStrike_SFX");
            hasPlayedLightningSound = true;
        }

        if (targetSpot.TryGetComponent(out PlaceholderID pathInfo))
            OnLightningStrike?.Invoke(targetSpot.name, pathInfo.placeholderID);

        activeEffects[targetSpot] = lightningEffect;

        UIManager.Instance.StartNextWaveCountdown();
    }

    public void StrikeRandomSpots(int strikeCount)
    {
        // Reset the sound flag before striking
        hasPlayedLightningSound = false;

        if (placeableSpots.Count == 0) return;

        // Filter out placeholders that already have towers
        List<GameObject> availableSpots = new List<GameObject>();
        foreach (var spot in placeableSpots)
        {
            // Assuming PlaceholderID or another component can help identify if a tower is present
            if (spot.GetComponent<TowerController>() == null) // No tower on this spot
            {
                availableSpots.Add(spot);
            }
        }

        // Ensure we don't strike more places than available
        int actualStrikeCount = Mathf.Min(strikeCount, availableSpots.Count);

        List<GameObject> shuffled = new List<GameObject>(availableSpots);
        ShuffleList(shuffled);

        for (int i = 0; i < actualStrikeCount; i++)
        {
            StartLightning(shuffled[i]);
        }

        // If there are not enough valid spots, place the remaining strikes on the available spots
        if (actualStrikeCount < strikeCount)
        {
            int remainingStrikes = strikeCount - actualStrikeCount;
            for (int i = 0; i < remainingStrikes; i++)
            {
                StartLightning(shuffled[i % shuffled.Count]); // Repeating strikes if not enough spots
            }
        }
    }

    private void StopLightning()
    {
        foreach (var pair in activeEffects)
        {
            if (pair.Key.TryGetComponent(out Collider col))
                col.enabled = true;

            Destroy(pair.Value);
        }

        activeEffects.Clear();
        UIManager.Instance.HideLightningStrikeUI();
    }
}
