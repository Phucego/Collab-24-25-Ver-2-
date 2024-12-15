using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class BuildingManager : MonoBehaviour
{
    public GameObject pendingObj;
    [SerializeField] private GameObject m_SpawnTowerButtonPrefab;
    [SerializeField] private Transform m_SpawnTowerButtonParent;

    private Material greenMaterial; // Material for valid placement
    private Material redMaterial;   // Material for invalid placement

    public float minimumPlacementDistance = 3.0f; // Minimum distance between towers
    private List<GameObject> placedTowers = new List<GameObject>(); // List of placed towers

    public GameObject placementIndicator; // Reference to the indicator box
    public float snapHeight = 1.5f;
    public bool canPlace;

    public BuildingManager Instance;

    [SerializeField] private LayerMask placeableLayer;
    [SerializeField] private LayerMask unplaceableLayer;

    private Vector3 towerPos;

    private void Awake()
    {
        Instance = this;

        // Find the green and red materials by name
        greenMaterial = Resources.Load<Material>("Materials/GreenIndicator");
        redMaterial = Resources.Load<Material>("Materials/RedIndicator");
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

            // Place the object on left mouse click if placement is valid
            if (Input.GetMouseButtonDown(0) && canPlace)
            {
                PlaceObject();
            }

            // Delete the pending object on pressing Escape
            if (Input.GetKeyDown(KeyCode.Escape)) 
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
                // Snapping logic for grid placement
                float snapValue = 1.0f; // The snap grid size (you can adjust this to your needs)
                float snappedX = Mathf.Round(hit.point.x / snapValue) * snapValue;
                float snappedZ = Mathf.Round(hit.point.z / snapValue) * snapValue;
                float snappedY = hit.point.y + snapHeight; // Keep the Y as is with an offset

                towerPos = new Vector3(snappedX, snappedY, snappedZ);
                pendingObj.transform.position = towerPos;
                canPlace = placementCheck.CanPlace() && IsFarEnoughFromOthers(towerPos);
            }
            else if ((placeableLayer & (1 << hitObject.layer)) != 0)
            {
                // Snapping to the grid on placeable layer
                float snapValue = 1.0f; // The snap grid size (you can adjust this to your needs)
                float snappedX = Mathf.Round(hit.point.x / snapValue) * snapValue;
                float snappedZ = Mathf.Round(hit.point.z / snapValue) * snapValue;
                float snappedY = hit.point.y + snapHeight; // Keep the Y as is with an offset

                towerPos = new Vector3(snappedX, snappedY, snappedZ);
                pendingObj.transform.position = towerPos;

                // Verify no nearby towers and that placement is allowed
                canPlace = IsFarEnoughFromOthers(towerPos);
            }
            else
            {
                // Unplaceable layer or invalid object
                towerPos = hit.point + Vector3.up * snapHeight;
                pendingObj.transform.position = towerPos;
                canPlace = false;
            }
        }
        else
        {
            // No valid hit detected
            canPlace = false;
        }
    }

    bool IsFarEnoughFromOthers(Vector3 position)
    {
        // Check distance to all placed towers
        foreach (GameObject tower in placedTowers)
        {
            if (Vector3.Distance(position, tower.transform.position) < minimumPlacementDistance)
            {
                return false; // Too close to another tower
            }
        }
        return true; // Far enough from other towers
    }

    public void PlaceObject()
    {
        if (pendingObj == null || !canPlace) return;

        // Get the cost of the tower from its TowerDataSO
        TowerController towerController = pendingObj.GetComponent<TowerController>();
        if (towerController == null)
        {
            Debug.LogWarning("TowerController is missing on the pending object.");
            return;
        }

        int towerCost = towerController.TowerData.Cost; // Assuming TowerDataSO is referenced in TowerController

        // Check if the player has enough currency
        if (!CurrencyManager.Instance.HasEnoughCurrency(towerCost))
        {
            Debug.Log("Not enough currency to place this tower.");
            return; // Exit if the player can't afford the tower
        }

        // Deduct the currency
        CurrencyManager.Instance.DeductCurrency(towerCost);

        // Place the tower at the snapped position
        pendingObj.GetComponent<MeshRenderer>().material = greenMaterial;

        if (placementIndicator != null)
        {
            placementIndicator.SetActive(false); // Disable the placement indicator
        }

        pendingObj.GetComponent<TowerController>().TowerPlaced = true; // Mark as placed
        pendingObj.GetComponent<TowerInteract>().isPlaced = true;     // Allow interaction

        placedTowers.Add(pendingObj); // Add to placed towers list
        pendingObj = null;            // Clear the pending object

        Debug.Log("Tower placed successfully and currency deducted.");
    }

    public void SelectObject(GameObject prefab)
    {
        // If the selected tower is the same as the pending one, don't change the pending object
        if (pendingObj != null && prefab == pendingObj)
        {
            return; // Do nothing if the same tower is selected again
        }

        // If there is already a pending object, discard it
        if (pendingObj != null)
        {
            Destroy(pendingObj); // Destroy the previous pending object
            if (placementIndicator != null)
            {
                placementIndicator.SetActive(false); // Hide the previous indicator
            }
        }

        // Instantiate the new tower and set it as the pending object
        pendingObj = Instantiate(prefab, Vector3.zero, Quaternion.identity);
        placementIndicator = pendingObj.transform.Find("PlacementIndicator")?.gameObject;

        if (placementIndicator == null)
        {
            Debug.LogWarning("PlacementIndicator not found in the prefab.");
            return; // Exit early to avoid further errors
        }

        // Set the initial placement indicator material to red
        MeshRenderer renderer = placementIndicator.GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            renderer.material = redMaterial;
        }
        else
        {
            Debug.LogWarning("MeshRenderer not found on PlacementIndicator.");
        }

        // Immediately update the position to clip to the surface
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, placeableLayer | unplaceableLayer))
        {
            towerPos = hit.point + Vector3.up * snapHeight;
            pendingObj.transform.position = towerPos;

            // Check placement validity
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

        // Update material for the placement indicator
        MaterialUpdate();
    }

    void DeletePendingObject()
    {
        if (pendingObj != null)
        {
            Destroy(pendingObj); // Destroy the pending tower object
            pendingObj = null;

            // Hide the placement indicator if it exists
            if (placementIndicator != null)
            {
                placementIndicator.SetActive(false);
            }
        }
    }

    void MaterialUpdate()
    {
        if (pendingObj == null || placementIndicator == null) return;

        Material newMaterial = canPlace ? greenMaterial : redMaterial;

        MeshRenderer indicatorRenderer = placementIndicator.GetComponent<MeshRenderer>();
        indicatorRenderer.material = newMaterial;

        // Adjust transparency
        Color color = newMaterial.color;
        color.a = 0.3f; // Set low opacity
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

            go.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = sortedTowerData[count].towerPrefab.name;
            go.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = $"Cost: {sortedTowerData[count].Cost}";

            Image buttonImage = go.transform.GetChild(2).GetComponent<Image>();
            buttonImage.sprite = sortedTowerData[count].towerSprite;

            go.GetComponent<Button>().onClick.AddListener(() =>
            {
                SelectObject(sortedTowerData[count].towerPrefab);
                Debug.Log(sortedTowerData[count].towerPrefab.name);
            });
        }
    }
}
