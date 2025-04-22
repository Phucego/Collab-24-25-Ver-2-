using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class LightningStrikeEvent : MonoBehaviour
{
    [Header("Tornado Strike Settings")]
    [SerializeField] private GameObject tornadoEffectPrefab; // Visual effect for tornado

    public List<GameObject> placeableSpots = new List<GameObject>();
    private Dictionary<GameObject, GameObject> activeEffects = new Dictionary<GameObject, GameObject>();
    public UnityEvent<string, int> OnLightningStrike; // For UI notification (UIManager)
    private bool hasPlayedLightningSound = false;
    private WaveManager waveManager;
    private UIManager uiManager;
    private int lastKnownWave = -1;

    private void Awake()
    {
        waveManager = WaveManager.Instance;
        if (waveManager == null)
        {
            Debug.LogError("LightningStrikeEvent: WaveManager.Instance not found in scene!");
            enabled = false;
        }

        uiManager = FindObjectOfType<UIManager>();
        if (uiManager == null)
        {
            Debug.LogError("LightningStrikeEvent: UIManager.Instance not found in scene!");
            enabled = false;
        }
    }

    private void Start()
    {
        if (waveManager != null)
        {
            StartCoroutine(InitializeTornadoStrike());
            StartCoroutine(WaitForWaveManagerData());
        }
        else
        {
            Debug.LogWarning("LightningStrikeEvent: Cannot initialize due to missing WaveManager.");
        }

        if (BuildingManager.Instance != null)
        {
            BuildingManager.Instance.OnTowerPlaced.AddListener(HandleTowerPlaced);
            BuildingManager.Instance.OnTowerRemoved.AddListener(HandleTowerRemoved);
        }
        else
        {
            Debug.LogWarning("LightningStrikeEvent: BuildingManager.Instance not found.");
        }

        // Hook into UIManager's startWaveButton
        if (uiManager != null && uiManager.startWaveButton != null)
        {
            uiManager.startWaveButton.onClick.AddListener(OnStartWaveButtonClicked);
        }
    }

    private void OnStartWaveButtonClicked()
    {
        if (waveManager == null || waveManager._curWave != 0) return;

        Debug.Log("UIManager startWaveButton clicked. Triggering tornado for wave 1.");
        TriggerTornadoStrike(1);
    }

    private IEnumerator WaitForWaveManagerData()
    {
        float timeout = 15f;
        float elapsed = 0f;
        while (waveManager != null && (waveManager._curData == null || waveManager._curData.Count == 0) && elapsed < timeout)
        {
            Debug.LogWarning($"Waiting for WaveManager._curData to initialize. Elapsed: {elapsed:F1}s, SelectedLevel: {waveManager._selectedLevel}");
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (waveManager == null || waveManager._curData == null || waveManager._curData.Count == 0)
        {
            Debug.LogError($"WaveManager._curData not initialized after {timeout}s. Tornado strikes disabled.");
            yield break;
        }

        Debug.Log("WaveManager._curData initialized. Starting wave monitoring.");
        HookWaveEvents();
    }

    private void HookWaveEvents()
    {
        waveManager.OnWaveComplete += StopLightning;
        StartCoroutine(WatchForWaveStart());
    }

    private IEnumerator InitializeTornadoStrike()
    {
        yield return new WaitForSeconds(1f);
        UpdatePlaceableSpots();
    }

    private void UpdatePlaceableSpots()
    {
        placeableSpots.Clear();
        GameObject[] placeholders = GameObject.FindGameObjectsWithTag("Placeable");
        foreach (var spot in placeholders)
        {
            if (!spot.CompareTag("Placeable"))
            {
                Debug.LogWarning($"Placeholder {spot.name} has incorrect tag: {spot.tag}. Expected 'Placeable'.");
                continue;
            }
            bool hasTower = spot.GetComponent<TowerController>() != null;
            Debug.Log($"Placeholder {spot.name}: Tag={spot.tag}, HasTowerController={hasTower}");
            if (!hasTower)
            {
                placeableSpots.Add(spot);
            }
        }
        Debug.Log($"Updated placeableSpots: {placeableSpots.Count} empty placeholders found.");
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
        int lastCurWave = -1;

        while (waveManager != null)
        {
            bool isSpawning = waveManager._isSpawning;
            int currentWave = waveManager._curWave + 1;
            bool isWaitingForNextWave = waveManager._waitingForNextWave;
            int remTime = waveManager._remTime;

            if (currentWave != lastKnownWave)
            {
                Debug.Log($"WaveManager wave changed: currentWave={currentWave}, lastKnownWave={lastKnownWave}, _curWave={waveManager._curWave}");
                lastKnownWave = currentWave;
            }

            Debug.Log($"WaveManager state: isSpawning={isSpawning}, wasSpawning={wasSpawning}, currentWave={currentWave}, _curWave={waveManager._curWave}, waitingForNextWave={isWaitingForNextWave}, remTime={remTime}, placeableSpots.Count={placeableSpots.Count}");

            // Trigger for waves 2+ when _isSpawning transitions or _curWave increments
            if (currentWave > 1 && !isWaitingForNextWave)
            {
                if (isSpawning && !wasSpawning)
                {
                    Debug.Log($"Detected _isSpawning transition for wave {currentWave}. Triggering tornadoes.");
                    TriggerTornadoStrike(currentWave);
                }
                else if (waveManager._curWave > lastCurWave)
                {
                    Debug.LogWarning($"Detected _curWave increment without _isSpawning transition for wave {currentWave}. Triggering tornadoes.");
                    TriggerTornadoStrike(currentWave);
                }
            }

            wasSpawning = isSpawning;
            lastCurWave = waveManager._curWave;
            yield return new WaitForSeconds(0.1f);
        }

        Debug.LogError("WaveManager became null during WatchForWaveStart.");
    }

    private void TriggerTornadoStrike(int currentWave)
    {
        if (tornadoEffectPrefab == null)
        {
            Debug.LogError("tornadoEffectPrefab is not assigned. Cannot spawn tornadoes.");
            return;
        }

        // Refresh placeableSpots before spawning
        UpdatePlaceableSpots();

        Debug.Log($"Wave {currentWave} started. Triggering {currentWave} tornado strike(s).");
        StrikeRandomSpots(currentWave);
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
        if (targetSpot == null)
        {
            Debug.LogWarning("StartLightning: Target spot is null.");
            return;
        }

        if (targetSpot.TryGetComponent(out Collider col))
            col.enabled = false;

        GameObject tornadoEffect = Instantiate(tornadoEffectPrefab, targetSpot.transform.position + Vector3.up * 1.5f, Quaternion.Euler(-90f, 0f, 0f));

        if (!hasPlayedLightningSound)
        {
            AudioManager.Instance.PlaySoundEffect("LightningStrike_SFX");
            hasPlayedLightningSound = true;
        }

        if (targetSpot.TryGetComponent(out PlaceholderID pathInfo))
        {
            OnLightningStrike?.Invoke(targetSpot.name, pathInfo.placeholderID);
            Debug.Log($"OnLightningStrike invoked for {targetSpot.name}, pathID={pathInfo.placeholderID}");
        }
        else
        {
            OnLightningStrike?.Invoke(targetSpot.name, -1);
            Debug.Log($"OnLightningStrike invoked for {targetSpot.name}, pathID=-1");
        }

        activeEffects[targetSpot] = tornadoEffect;
        Debug.Log($"Tornado spawned at {targetSpot.name}. Active effects: {activeEffects.Count}");
    }

    public void StrikeRandomSpots(int strikeCount)
    {
        hasPlayedLightningSound = false;

        if (placeableSpots.Count == 0)
        {
            Debug.LogWarning("No empty placeholders available for tornado strikes.");
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
            Debug.Log($"Tornado struck empty placeholder: {shuffled[i].name}");
        }

        if (actualStrikeCount < strikeCount)
        {
            int remainingStrikes = strikeCount - actualStrikeCount;
            for (int i = 0; i < remainingStrikes; i++)
            {
                StartLightning(shuffled[i % shuffled.Count]);
                Debug.Log($"Tornado struck repeated empty placeholder: {shuffled[i % shuffled.Count].name}");
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
        Debug.Log("Calling UIManager.StartNextWaveCountdown from StopLightning.");
        UIManager.Instance.StartNextWaveCountdown();
    }

    private void OnDestroy()
    {
        if (waveManager != null)
        {
            waveManager.OnWaveComplete -= StopLightning;
        }

        if (BuildingManager.Instance != null)
        {
            BuildingManager.Instance.OnTowerPlaced.RemoveListener(HandleTowerPlaced);
            BuildingManager.Instance.OnTowerRemoved.RemoveListener(HandleTowerRemoved);
        }

        if (uiManager != null && uiManager.startWaveButton != null)
        {
            uiManager.startWaveButton.onClick.RemoveListener(OnStartWaveButtonClicked);
        }
    }
}