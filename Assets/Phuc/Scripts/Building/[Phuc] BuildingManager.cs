using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BuildingManager : MonoBehaviour
{
    public GameObject pendingObj;
    [SerializeField] private GameObject m_SpawnTowerButtonPrefab;
    [SerializeField] private Transform m_SpawnTowerButtonParent;
    
    private Material greenMaterial; // Material for valid placement
    private Material redMaterial;   // Material for invalid placement
    
    private TowerInteract _towerInteract;
    public delegate void OnCountCompleted(int i);
    public OnCountCompleted onCountCompleted;

    private Vector3 towerPos;
    RaycastHit hit;
    [SerializeField] private LayerMask placeableLayer;
    [SerializeField] private LayerMask unplaceableLayer;

    public GameObject placementIndicator; // Reference to the indicator box

    public float snapHeight = 1.5f;
    public bool canPlace;

    public BuildingManager Instance;

    private void Awake()
    {
        Instance = this;

        // Find the green and red materials by name
        greenMaterial = Resources.Load<Material>("Materials/GreenIndicator");
        redMaterial = Resources.Load<Material>("Materials/RedIndicator");

        if (greenMaterial == null || redMaterial == null)
        {
            Debug.LogError("Green or Red material not found! Ensure they are in the 'Resources/Materials' folder and named correctly.");
        }
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
                if (placementCheck.CanPlace())
                {
                    towerPos = hit.point + Vector3.up * snapHeight;
                    pendingObj.transform.position = towerPos;
                    canPlace = true;
                }
                else
                {
                    towerPos = hit.point + Vector3.up * snapHeight;
                    pendingObj.transform.position = towerPos;
                    canPlace = false;
                }
            }
            else if ((placeableLayer & (1 << hitObject.layer)) != 0)
            {
                towerPos = hit.point + Vector3.up * snapHeight;
                pendingObj.transform.position = towerPos;
                canPlace = true;
            }
            else
            {
                towerPos = hit.point + Vector3.up * snapHeight;
                pendingObj.transform.position = towerPos;
                canPlace = false;
            }
        }
        else
        {
            canPlace = false;
        }
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

        pendingObj = null;
    }

    public void SelectObject(GameObject prefab)
    {
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
