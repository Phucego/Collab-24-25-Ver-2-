using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class VineEntangleEvent : MonoBehaviour
{
    [Header("Vine Entangle Settings")]
    [SerializeField] private SceneField targetScene;
    [SerializeField] private GameObject vineEffectPrefab;
    [SerializeField] private float entangleInterval = 25f;
    [SerializeField] private float entangleDuration = 30f;

    [SerializeField] private List<GameObject> placeableSpots = new List<GameObject>();
    private Dictionary<GameObject, GameObject> activeVines = new Dictionary<GameObject, GameObject>();
    public List<GameObject> towerSpots;
    [Header("Events")]
    public UnityEvent<string, int> OnVineEntangle;
    
    private bool hasPlayedVineSound = false;

    private void Start()
    {
        if (WaveManager.Instance != null)
            StartCoroutine(EntangleRoutine());
        else
            StartCoroutine(WaitForWaveManager());

        StartCoroutine(UpdatePlaceableSpotsRoutine());

        if (BuildingManager.Instance != null)
        {
            BuildingManager.Instance.OnTowerPlaced.AddListener(HandleTowerPlaced);
            BuildingManager.Instance.OnTowerRemoved.AddListener(HandleTowerRemoved);
        }
    }

    private IEnumerator WaitForWaveManager()
    {
        while (WaveManager.Instance == null)
            yield return null;

        StartCoroutine(EntangleRoutine());
    }
    private void HandleTowerPlaced(GameObject towerSpot)
    {
        if (!towerSpots.Contains(towerSpot))
            towerSpots.Add(towerSpot);
    }

    private void HandleTowerRemoved(GameObject towerSpot)
    {
        if (towerSpots.Contains(towerSpot))
            towerSpots.Remove(towerSpot);

        // Also remove any vines on it
        if (activeVines.ContainsKey(towerSpot))
        {
            Destroy(activeVines[towerSpot]);
            activeVines.Remove(towerSpot);
        }

        if (activeVines.Count == 0)
            UIManager.Instance?.HideVineEntangleUI();
    }

    // Continuously updates the list of placeable spots
    private IEnumerator UpdatePlaceableSpotsRoutine()
    {
        while (true)
        {
            GameObject[] placeholders = GameObject.FindGameObjectsWithTag("Placeable");
            placeableSpots.Clear();
            placeableSpots.AddRange(placeholders);

            yield return new WaitForSeconds(2f); // Refresh interval
        }
    }

    private IEnumerator EntangleRoutine()
    {
        while (true)
        {
            // Only proceed if 3 or more towers are placed
            if (towerSpots.Count < 3)
            {
                yield return new WaitForSeconds(entangleInterval);
                continue;
            }

            EntangleTowers(towerSpots);
            yield return new WaitForSeconds(entangleInterval);
        }
    }


    // Dynamically gets all active tower spots
    private List<GameObject> GetCurrentTowerSpots()
    {
        List<GameObject> currentTowerSpots = new List<GameObject>();

        foreach (var spot in placeableSpots)
        {
            if (spot != null && spot.GetComponent<TowerController>() != null)
                currentTowerSpots.Add(spot);
        }

        return currentTowerSpots;
    }

    private void EntangleTowers(List<GameObject> towerSpots)
    {
        ShuffleList(towerSpots);
        hasPlayedVineSound = false; // Reset flag for this entangle round

        foreach (GameObject spot in towerSpots)
        {
            if (!activeVines.ContainsKey(spot))
            {
                StartEntangle(spot);
            }
        }
    }
    
    private void StartEntangle(GameObject targetSpot)
    {
        if (targetSpot == null) return;

        if (targetSpot.TryGetComponent(out TowerController tower))
            tower.enabled = false;

        GameObject vineEffect = Instantiate(vineEffectPrefab, targetSpot.transform.position + Vector3.up * 1.2f, Quaternion.identity);
        activeVines[targetSpot] = vineEffect;

        if (!hasPlayedVineSound)
        {
            AudioManager.Instance?.PlaySoundEffect("VineEntangle_SFX");
            hasPlayedVineSound = true;
        }

        if (targetSpot.TryGetComponent(out PlaceholderID pathInfo))
            OnVineEntangle?.Invoke(targetSpot.name, pathInfo.placeholderID);

        UIManager.Instance?.ShowVineEntangleUI();

        StartCoroutine(RemoveVineAfterDuration(targetSpot, entangleDuration));
    }

    private IEnumerator RemoveVineAfterDuration(GameObject targetSpot, float duration)
    {
        yield return new WaitForSeconds(duration);

        if (targetSpot != null && activeVines.TryGetValue(targetSpot, out GameObject vineEffect))
        {
            if (targetSpot.TryGetComponent(out TowerController tower))
                tower.enabled = true;

            Destroy(vineEffect);
            activeVines.Remove(targetSpot);

            if (activeVines.Count == 0)
                UIManager.Instance?.HideVineEntangleUI();
        }
    }

    public void RemoveAllVines()
    {
        foreach (var pair in activeVines)
        {
            if (pair.Key.TryGetComponent(out TowerController tower))
                tower.enabled = true;

            Destroy(pair.Value);
        }

        activeVines.Clear();
        UIManager.Instance?.HideVineEntangleUI();
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
}
