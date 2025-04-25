using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Events;
using DG.Tweening;
using UnityEngine.EventSystems;

public class BuildingManager : MonoBehaviour
{
    [Header("Tutorial Events")]
    public UnityEvent OnFirstTowerPlaced;
    private bool hasPlacedFirstTower = false;
    private TutorialGuidance tutorialGuidance;

    // Existing variables
    public GameObject pendingObj;
    [SerializeField] private GameObject m_SpawnTowerButtonPrefab;
    [SerializeField] private Transform m_SpawnTowerButtonParent;

    private Material greenMaterial;
    private Material redMaterial;

    public float minimumPlacementDistance = 3.0f;
    private List<GameObject> placedTowers = new List<GameObject>();

    public GameObject placementIndicator;
    public float snapHeight = 1.5f;
    public bool canPlace;

    public static BuildingManager Instance;

    [SerializeField] private LayerMask placeableLayer;
    [SerializeField] private LayerMask unplaceableLayer;

    private Vector3 towerPos;
    private GameObject lastHitPlaceholder; // New: Store the last hit placeholder

    public UnityEvent<GameObject> OnTowerPlaced = new UnityEvent<GameObject>();
    public UnityEvent<GameObject> OnTowerRemoved = new UnityEvent<GameObject>();

    private void Awake()
    {
        Instance = this;
        greenMaterial = Resources.Load<Material>("Materials/GreenIndicator");
        redMaterial = Resources.Load<Material>("Materials/RedIndicator");
        tutorialGuidance = FindObjectOfType<TutorialGuidance>();
    }

    private void Start()
    {
        ButtonSpawner();
    }

    void Update()
    {
        if (pendingObj != null)
        {
            HandlePlacement();
            
            if (Input.GetMouseButtonDown(0) && canPlace)
            {
                PlaceObject();
            }
            else if (Input.GetMouseButtonDown(0) && !canPlace)
            {
                AudioManager.Instance.PlaySoundEffect("Insufficient_SFX");
            }

            if (Input.GetKeyDown(KeyCode.Q))
            {
                DeletePendingObject();
            }

            MaterialUpdate();
        }
    }

    void HandlePlacement()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, placeableLayer | unplaceableLayer))
        {
            GameObject hitObject = hit.collider.gameObject;

            PlacementCheck placementCheck = hitObject.GetComponent<PlacementCheck>();
            if (placementCheck != null)
            {
                lastHitPlaceholder = hitObject; // Store placeholder
                float snapValue = 1.0f;
                float snappedX = Mathf.Round(hit.point.x / snapValue) * snapValue;
                float snappedZ = Mathf.Round(hit.point.z / snapValue) * snapValue;
                float snappedY = hit.point.y + snapHeight;

                towerPos = new Vector3(snappedX, snappedY, snappedZ);
                pendingObj.transform.position = towerPos;
                canPlace = placementCheck.CanPlace() && IsFarEnoughFromOthers(towerPos);
            }
            else if ((placeableLayer & (1 << hitObject.layer)) != 0)
            {
                lastHitPlaceholder = hitObject; // Store placeholder
                float snapValue = 1.0f;
                float snappedX = Mathf.Round(hit.point.x / snapValue) * snapValue;
                float snappedZ = Mathf.Round(hit.point.z / snapValue) * snapValue;
                float snappedY = hit.point.y + snapHeight;

                towerPos = new Vector3(snappedX, snappedY, snappedZ);
                pendingObj.transform.position = towerPos;
                canPlace = IsFarEnoughFromOthers(towerPos);
            }
            else
            {
                lastHitPlaceholder = null; // Clear if not a valid placeholder
                towerPos = hit.point + Vector3.up * snapHeight;
                pendingObj.transform.position = towerPos;
                canPlace = false;
            }
        }
        else
        {
            lastHitPlaceholder = null; // Clear if no hit
            canPlace = false;
        }
    }

    bool IsFarEnoughFromOthers(Vector3 position)
    {
        foreach (GameObject tower in placedTowers)
        {
            if (tower != null && Vector3.Distance(position, tower.transform.position) < minimumPlacementDistance)
            {
                return false;
            }
        }
        return true;
    }

    public void PlaceObject()
    {
        if (pendingObj == null || !canPlace || lastHitPlaceholder == null) return;

        TowerController towerController = pendingObj.GetComponent<TowerController>();
        if (towerController == null)
        {
            Debug.LogWarning("TowerController is missing on the pending object.");
            return;
        }

        int towerCost = towerController.TowerData.Cost;

        if (!CurrencyManager.Instance.HasEnoughCurrency(towerCost))
        {
            AudioManager.Instance.PlaySoundEffect("Insufficient_SFX");
            Debug.Log("Not enough currency to place this tower.");
            return;
        }

        CurrencyManager.Instance.DeductCurrency(towerCost);
        pendingObj.GetComponent<MeshRenderer>().material = greenMaterial;

        if (placementIndicator != null)
        {
            placementIndicator.SetActive(false);
        }

        // Re-enable all colliders on the placed tower
        Collider[] colliders = pendingObj.GetComponentsInChildren<Collider>();
        foreach (Collider collider in colliders)
        {
            collider.enabled = true;
        }

        towerController.TowerPlaced = true;
        pendingObj.GetComponent<TowerInteract>().isPlaced = true;
        AudioManager.Instance.PlaySoundEffect("BuildTower_SFX");

        placedTowers.Add(pendingObj);
        OnTowerPlaced?.Invoke(lastHitPlaceholder); // Pass placeholder instead of pendingObj

        if (!hasPlacedFirstTower)
        {
            hasPlacedFirstTower = true;
            OnFirstTowerPlaced?.Invoke();

            if (tutorialGuidance != null)
            {
                tutorialGuidance.corruptedIntroDestination.SetActive(true);
            }
        }

        pendingObj = null;
        lastHitPlaceholder = null; // Clear after placement
        Debug.Log("Tower placed successfully and currency deducted.");
    }

    public bool HasPlacedFirstTower()
    {
        return hasPlacedFirstTower;
    }

    void DeletePendingObject()
    {
        if (pendingObj != null)
        {
            pendingObj.GetComponent<I_Interactable>().Deselect();
            Destroy(pendingObj);
            pendingObj = null;

            if (placementIndicator != null)
            {
                placementIndicator.SetActive(false);
            }
        }
        lastHitPlaceholder = null; // Clear on cancel
    }

    void MaterialUpdate()
    {
        if (pendingObj == null || placementIndicator == null) return;

        Material newMaterial = canPlace ? greenMaterial : redMaterial;

        MeshRenderer indicatorRenderer = placementIndicator.GetComponent<MeshRenderer>();
        indicatorRenderer.material = newMaterial;

        Color color = newMaterial.color;
        color.a = 0.3f;
        indicatorRenderer.material.color = color;
    }

    void ButtonSpawner()
    {
        var sortedTowerData = LevelManager.instance.LevelDataSO.towerData
            .OrderBy(tower => tower.Cost)
            .ToArray();

        for (int i = 0; i < sortedTowerData.Length; i++)
        {
            int count = i;
            GameObject go = Instantiate(m_SpawnTowerButtonPrefab, m_SpawnTowerButtonParent);

            TextMeshProUGUI nameText = go.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI costText = go.transform.GetChild(1).GetComponent<TextMeshProUGUI>();
            nameText.text = sortedTowerData[count].towerPrefab.name;
            costText.text = $"Cost: {sortedTowerData[count].Cost}";

            Image buttonImage = go.transform.GetChild(2).GetComponent<Image>();
            buttonImage.sprite = sortedTowerData[count].towerSprite;

            RectTransform imageRect = buttonImage.GetComponent<RectTransform>();
            imageRect.sizeDelta = new Vector2(28f, 44f);

            RectTransform nameRect = nameText.GetComponent<RectTransform>();
            RectTransform costRect = costText.GetComponent<RectTransform>();
            Vector2 imagePos = imageRect.anchoredPosition;

            nameRect.anchoredPosition = imagePos;
            costRect.anchoredPosition = imagePos + new Vector2(0, -20f);
            nameText.alpha = 0f;
            costText.alpha = 0f;

            EventTrigger trigger = go.AddComponent<EventTrigger>();

            EventTrigger.Entry pointerEnter = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
            pointerEnter.callback.AddListener((data) =>
            {
                nameRect.DOAnchorPosX(imagePos.x + 120f, 0.3f).SetEase(Ease.OutQuad);
                costRect.DOAnchorPosX(imagePos.x + 120f, 0.3f).SetEase(Ease.OutQuad);
                nameText.DOFade(1f, 0.3f);
                costText.DOFade(1f, 0.3f);
            });
            trigger.triggers.Add(pointerEnter);

            EventTrigger.Entry pointerExit = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
            pointerExit.callback.AddListener((data) =>
            {
                nameRect.DOAnchorPosX(imagePos.x, 0.3f).SetEase(Ease.InQuad);
                costRect.DOAnchorPosX(imagePos.x, 0.3f).SetEase(Ease.InQuad);
                nameText.DOFade(0f, 0.3f);
                costText.DOFade(0f, 0.3f);
            });
            trigger.triggers.Add(pointerExit);

            go.GetComponent<Button>().onClick.AddListener(() =>
            {
                SelectObject(sortedTowerData[count].towerPrefab);
                AudioManager.Instance.PlaySoundEffect("ButtonClick_SFX");
                Debug.Log(sortedTowerData[count].towerPrefab.name);
            });
        }
    }

    public void RemoveTowerFromList(GameObject tower)
    {
        if (placedTowers.Contains(tower))
        {
            placedTowers.Remove(tower);
            OnTowerRemoved?.Invoke(tower); // Pass tower for removal
        }
    }

    public void SelectObject(GameObject prefab)
    {
        if (pendingObj != null && prefab == pendingObj)
        {
            return;
        }

        if (pendingObj != null)
        {
            Destroy(pendingObj);
            if (placementIndicator != null)
            {
                placementIndicator.SetActive(false);
            }
        }

        pendingObj = Instantiate(prefab, Vector3.zero, Quaternion.identity);
        placementIndicator = pendingObj.transform.Find("PlacementIndicator")?.gameObject;

        // Disable all colliders on the pending object to prevent interactions
        Collider[] colliders = pendingObj.GetComponentsInChildren<Collider>();
        foreach (Collider collider in colliders)
        {
            collider.enabled = false;
        }

        if (placementIndicator == null)
        {
            Debug.LogWarning("PlacementIndicator not found in the prefab.");
            return;
        }

        MeshRenderer renderer = placementIndicator.GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            renderer.material = redMaterial;
        }
        else
        {
            Debug.LogWarning("MeshRenderer not found on PlacementIndicator.");
        }

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, placeableLayer | unplaceableLayer))
        {
            towerPos = hit.point + Vector3.up * snapHeight;
            pendingObj.transform.position = towerPos;

            GameObject hitObject = hit.collider.gameObject;
            PlacementCheck placementCheck = hitObject.GetComponent<PlacementCheck>();
            if (placementCheck != null)
            {
                canPlace = placementCheck.CanPlace() && IsFarEnoughFromOthers(towerPos);
            }
            else if ((placeableLayer & (1 << hitObject.layer)) != 0)
            {
                canPlace = IsFarEnoughFromOthers(towerPos);
            }
            else
            {
                canPlace = false;
            }
        }
        else
        {
            canPlace = false;
        }

        MaterialUpdate();
    }
}