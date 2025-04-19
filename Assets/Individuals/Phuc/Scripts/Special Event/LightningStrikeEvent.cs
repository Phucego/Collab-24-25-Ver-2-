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
    
    private bool hasPlayedLightningSound = false;

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

        if (BuildingManager.Instance != null)
        {
            BuildingManager.Instance.OnTowerPlaced.AddListener(HandleTowerPlaced);
            BuildingManager.Instance.OnTowerRemoved.AddListener(HandleTowerRemoved);
        }
    }

    private void OnDestroy()
    {
        if (WaveManager.Instance != null)
            WaveManager.Instance.OnWaveComplete -= StopLightning;

        if (BuildingManager.Instance != null)
        {
            BuildingManager.Instance.OnTowerPlaced.RemoveListener(HandleTowerPlaced);
            BuildingManager.Instance.OnTowerRemoved.RemoveListener(HandleTowerRemoved);
        }
    }

    private IEnumerator WaitForWaveManager()
    {
        float timeout = 5f;
        float elapsed = 0f;
        while (WaveManager.Instance == null && elapsed < timeout)
        {
            Debug.LogWarning("WaveManager.Instance is null. Waiting for initialization.");
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (WaveManager.Instance == null)
        {
            Debug.LogError($"WaveManager.Instance not found after {timeout}s. Lightning strikes may not trigger.");
            yield break;
        }

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

        if (SceneManager.GetActiveScene().name != targetScene)
        {
            Debug.LogWarning($"Scene {SceneManager.GetActiveScene().name} does not match targetScene {targetScene}. Lightning strikes disabled.");
            yield break;
        }

        UpdatePlaceableSpots();
    }

    private void UpdatePlaceableSpots()
    {
        placeableSpots.Clear();
        GameObject[] placeholders = GameObject.FindGameObjectsWithTag("Placeable");
        foreach (var spot in placeholders)
        {
            if (spot.GetComponent<TowerController>() == null)
            {
                placeableSpots.Add(spot);
            }
        }
        Debug.Log($"Initialized placeableSpots: {placeableSpots.Count} empty placeholders found.");
    }

    private void HandleTowerPlaced(GameObject placeholder)
    {
        if (placeholder == null || !placeholder.CompareTag("Placeable"))
        {
            Debug.LogWarning($"HandleTowerPlaced: Invalid placeholder {placeholder?.name}. Expected Placeable tag.");
            return;
        }

        if (placeableSpots.Contains(placeholder))
        {
            placeableSpots.Remove(placeholder);
            Debug.Log($"Tower placed on placeholder {placeholder.name}. Removed from placeableSpots. Count: {placeableSpots.Count}");
        }
        else
        {
            Debug.Log($"Tower placed on {placeholder.name}, but it was not in placeableSpots.");
        }
    }

    private void HandleTowerRemoved(GameObject tower)
    {
        GameObject placeholder = tower;
        if (!placeableSpots.Contains(placeholder) && placeholder.CompareTag("Placeable") && placeholder.GetComponent<TowerController>() == null)
        {
            placeableSpots.Add(placeholder);
            Debug.Log($"Tower removed from {placeholder.name}. Added to placeableSpots. Count: {placeableSpots.Count}");
        }
    }

    private IEnumerator WatchForWaveStart()
    {
        bool wasSpawning = false;

        while (true)
        {
            if (WaveManager.Instance != null)
            {
                bool isSpawning = false;
                try
                {
                    isSpawning = (bool)typeof(WaveManager).GetField("_isSpawning", System.Reflection.BindingFlags.NonPublic 
                        | System.Reflection.BindingFlags.Instance).GetValue(WaveManager.Instance);
                }
                catch (System.Exception e)
                {
                    isSpawning = false;
                }
                int currentWave = WaveManager.Instance != null ? (int)typeof(WaveManager).GetField("_curWave", System.Reflection.BindingFlags.NonPublic 
                    | System.Reflection.BindingFlags.Instance).GetValue(WaveManager.Instance) + 1 : 1;
                
                if (isSpawning && !wasSpawning)
                {
                    StrikeRandomSpots(Mathf.Max(1, currentWave));
                }
                wasSpawning = isSpawning;
            }
            yield return new WaitForSeconds(0.1f);
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

        if (!hasPlayedLightningSound)
        {
            AudioManager.Instance.PlaySoundEffect("LightningStrike_SFX");
            hasPlayedLightningSound = true;
        }

        if (targetSpot.TryGetComponent(out PlaceholderID pathInfo))
            OnLightningStrike?.Invoke(targetSpot.name, pathInfo.placeholderID);

        activeEffects[targetSpot] = lightningEffect;

        //UIManager.Instance.StartNextWaveCountdown();
    }

    public void StrikeRandomSpots(int strikeCount)
    {
        hasPlayedLightningSound = false;

        if (placeableSpots.Count == 0)
        {
            Debug.LogWarning("No empty placeholders available for lightning strikes.");
            return;
        }

        List<GameObject> availableSpots = new List<GameObject>();
        foreach (var spot in placeableSpots)
        {
            if (spot.GetComponent<TowerController>() == null)
            {
                availableSpots.Add(spot);
            }
            else
            {
                Debug.Log($"Skipped {spot.name}: Has TowerController (tower present).");
            }
        }

        int actualStrikeCount = Mathf.Min(strikeCount, availableSpots.Count);

        List<GameObject> shuffled = new List<GameObject>(availableSpots);
        ShuffleList(shuffled);

        for (int i = 0; i < actualStrikeCount; i++)
        {
            StartLightning(shuffled[i]);
            Debug.Log($"Lightning struck empty placeholder: {shuffled[i].name}");
        }

        if (actualStrikeCount < strikeCount)
        {
            int remainingStrikes = strikeCount - actualStrikeCount;
            for (int i = 0; i < remainingStrikes; i++)
            {
                StartLightning(shuffled[i % shuffled.Count]);
                Debug.Log($"Lightning struck repeated empty placeholder: {shuffled[i % shuffled.Count].name}");
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