using System.Collections.Generic;
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
            if (Input.GetMouseButtonDown(0) && canPlace)
            {
                PlaceObject();
            }

            MaterialUpdate();
        }
    }

    #region Building System Logics
    void HandlePlacement()
    {
        Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, placeableLayer | unplaceableLayer))
        {
            GameObject hitObject = hit.collider.gameObject;

            PlacementCheck placementCheck = hitObject.GetComponent<PlacementCheck>();
            if (placementCheck != null)
            {
                towerPos = hit.point + Vector3.up * snapHeight;
                pendingObj.transform.position = towerPos;
                canPlace = placementCheck.CanPlace() && IsFarEnoughFromOthers(towerPos);
            }
            else if ((placeableLayer & (1 << hitObject.layer)) != 0)
            {
                // Object is on the placeable layer, but check additional conditions
                towerPos = hit.point + Vector3.up * snapHeight;
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

        pendingObj.GetComponent<MeshRenderer>().material = greenMaterial; // Set the material to green

        if (placementIndicator != null)
        {
            placementIndicator.SetActive(false); // Disable the placement indicator
        }

        pendingObj.GetComponent<TowerController>().TowerPlaced = true;
        pendingObj.GetComponent<TowerInteract>().isPlaced = true;

        placedTowers.Add(pendingObj); // Add the placed tower to the list
        pendingObj = null;
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
        placementIndicator = pendingObj.transform.Find("PlacementIndicator").gameObject;

        // Set the initial placement indicator material to red
        if (placementIndicator != null)
        {
            placementIndicator.GetComponent<MeshRenderer>().material = redMaterial;
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
    #endregion

    void ButtonSpawner()
    {
        for (int i = 0; i < LevelManager.instance.LevelDataSO.towerData.Length; i++)
        {
            int count = i;
            GameObject go = Instantiate(m_SpawnTowerButtonPrefab, m_SpawnTowerButtonParent);
            go.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = LevelManager.instance.LevelDataSO.towerData[count].towerPrefab.name;
            go.GetComponent<Button>().onClick.AddListener(() =>
            {
                SelectObject(LevelManager.instance.LevelDataSO.towerData[count].towerPrefab);
            });
        }
    }
}
