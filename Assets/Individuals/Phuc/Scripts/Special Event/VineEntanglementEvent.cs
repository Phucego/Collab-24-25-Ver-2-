using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using DG.Tweening;
using TMPro;

public class VineEntangleEvent : MonoBehaviour
{
    [Header("Vine Entangle Settings")]
    [SerializeField] private SceneField targetScene;
    [SerializeField] private GameObject vineEffectPrefab;
    [SerializeField] private float entangleInterval = 25f;
    [SerializeField] private float entangleDuration = 30f;

    [SerializeField] private List<GameObject> placeableSpots = new List<GameObject>();
    private Dictionary<GameObject, GameObject> activeVines = new Dictionary<GameObject, GameObject>();
    private Dictionary<GameObject, int> debuffTypes = new Dictionary<GameObject, int>();
    public List<GameObject> towerSpots;

    public float distanceUpVine;
    [Header("Events")]
    public UnityEvent<string, int> OnVineEntangle;

    private bool hasPlayedVineSound = false;

    private void Start()
    {
        if (UIManager.Instance?.vineEntangleNotificationPanel != null)
        {
            UIManager.Instance.vineEntangleNotificationPanel.SetActive(false);
            UIManager.Instance.vineEntangleNotificationPanel.transform.localScale = Vector3.zero;
        }

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

        if (activeVines.ContainsKey(towerSpot))
        {
            Destroy(activeVines[towerSpot]);
            activeVines.Remove(towerSpot);
            debuffTypes.Remove(towerSpot);
        }

        if (activeVines.Count == 0)
            UIManager.Instance?.HideVineEntangleUI();
    }

    private IEnumerator UpdatePlaceableSpotsRoutine()
    {
        while (true)
        {
            GameObject[] placeholders = GameObject.FindGameObjectsWithTag("Placeable");
            placeableSpots.Clear();
            placeableSpots.AddRange(placeholders);
            yield return new WaitForSeconds(2f);
        }
    }

    private IEnumerator EntangleRoutine()
    {
        while (true)
        {
            if (towerSpots.Count < 3)
            {
                yield return new WaitForSeconds(entangleInterval);
                continue;
            }

            EntangleOneTower(towerSpots);
            yield return new WaitForSeconds(entangleInterval);
        }
    }

    private void EntangleOneTower(List<GameObject> towerSpots)
    {
        hasPlayedVineSound = false;

        List<GameObject> validSpots = new List<GameObject>();
        foreach (var spot in towerSpots)
        {
            if (!activeVines.ContainsKey(spot))
                validSpots.Add(spot);
        }

        if (validSpots.Count == 0)
            return;

        int randomIndex = Random.Range(0, validSpots.Count);
        GameObject targetSpot = validSpots[randomIndex];

        StartEntangle(targetSpot);
    }

    private void StartEntangle(GameObject targetSpot)
    {
        if (targetSpot == null) return;

        int debuffType = Random.Range(1, 3); // Randomly choose 1 (FireRate) or 2 (Damage)
        if (targetSpot.TryGetComponent(out TowerController tower))
        {
            float debuffValue = debuffType == 1 ? 0.6667f : 0.25f; // 40% attack speed reduction or 25% damage reduction
            tower.ApplyDebuff(debuffType, entangleDuration, debuffValue);
            debuffTypes[targetSpot] = debuffType;
        }

        GameObject vineEffect = Instantiate(vineEffectPrefab, targetSpot.transform.position + Vector3.up * distanceUpVine, Quaternion.identity);
        activeVines[targetSpot] = vineEffect;

        if (!hasPlayedVineSound)
        {
            AudioManager.Instance?.PlaySoundEffect("VineEntangle_SFX");
            hasPlayedVineSound = true;
        }

        if (targetSpot.TryGetComponent(out PlaceholderID pathInfo))
            OnVineEntangle?.Invoke(targetSpot.name, pathInfo.placeholderID);

        UIManager.Instance?.ShowVineEntangleUI();

        AnimateVineNotification(targetSpot);
    }

    private IEnumerator RemoveVineAfterDuration(GameObject targetSpot, float duration)
    {
        yield return new WaitForSeconds(duration);

        if (targetSpot != null && activeVines.TryGetValue(targetSpot, out GameObject vineEffect))
        {
            if (targetSpot.TryGetComponent(out TowerController tower) && debuffTypes.TryGetValue(targetSpot, out int debuffType))
            {
                tower.SetBaseStat(debuffType);
            }

            Destroy(vineEffect);
            activeVines.Remove(targetSpot);
            debuffTypes.Remove(targetSpot);

            if (activeVines.Count == 0)
                UIManager.Instance?.HideVineEntangleUI();
        }
    }

    public void RemoveAllVines()
    {
        foreach (var pair in activeVines)
        {
            if (pair.Key.TryGetComponent(out TowerController tower) && debuffTypes.TryGetValue(pair.Key, out int debuffType))
            {
                tower.SetBaseStat(debuffType);
            }

            Destroy(pair.Value);
        }

        activeVines.Clear();
        debuffTypes.Clear();
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
    
    private void AnimateVineNotification(GameObject targetSpot)
    {
        var notificationUI = UIManager.Instance?.vineEntangleNotificationPanel;
        if (notificationUI == null) return;

        TextMeshProUGUI notificationText = notificationUI.GetComponentInChildren<TextMeshProUGUI>();
        if (notificationText != null && debuffTypes.TryGetValue(targetSpot, out int debuffType))
        {
            notificationText.text = debuffType == 1 ? "Tower Entangled: Attack Speed Debuffed!" : "Tower Entangled: Damage Debuffed!";
        }
        else
        {
            Debug.LogWarning("No TextMeshProUGUI found in vineEntangleNotificationPanel or debuff type not found for tower.");
        }

        notificationUI.transform.localScale = Vector3.zero;
        notificationUI.SetActive(true);

        notificationUI.transform.DOScale(1f, 0.5f).SetEase(Ease.OutBack).OnComplete(() =>
        {
            DOVirtual.DelayedCall(2f, () =>
            {
                notificationUI.transform.DOScale(0f, 0.3f).SetEase(Ease.InBack).OnComplete(() =>
                {
                    notificationUI.SetActive(false);
                });
            });
        });
    }
}