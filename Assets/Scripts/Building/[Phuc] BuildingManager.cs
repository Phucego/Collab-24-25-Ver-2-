using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering.VirtualTexturing;
using UnityEngine.UI;

public class BuildingManager : MonoBehaviour
{
    public GameObject pendingObj;
    [SerializeField] private GameObject m_SpawnTowerButtonPrefab;
    [SerializeField] private Transform m_SpawnTowerButtonParent;
    [SerializeField] private Material[] placementMats;

    public delegate void OnCountCompleted(int i);
    public OnCountCompleted onCountCompleted;

    private Vector3 towerPos;
    RaycastHit hit;
    [SerializeField] private LayerMask placeableLayer;
    [SerializeField] private LayerMask unplaceableLayer; // Renamed for clarity

    public float snapHeight = 1.5f;
    public bool canPlace;

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

            /*HandlePlacement();*/
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

            // Check if the hit object has a PlacementCheck component
            PlacementCheck placementCheck = hitObject.GetComponent<PlacementCheck>();

            if (placementCheck != null)
            {
                // Use the PlacementCheck component's decision
                if (placementCheck.CanPlace())
                {
                    Debug.Log($"Valid placement on {hitObject.name}");
                    towerPos = hit.point + Vector3.up * snapHeight;
                    pendingObj.transform.position = towerPos;
                    canPlace = true;
                }
                else
                {
                    Debug.Log($"Invalid placement on {hitObject.name}: PlacementCheck denied.");
                    towerPos = hit.point + Vector3.up * snapHeight;
                    pendingObj.transform.position = towerPos;
                    canPlace = false;
                }
            }
            else if ((placeableLayer & (1 << hitObject.layer)) != 0)
            {
                // Check the placeable layer only if no PlacementCheck component is found
                Debug.Log($"Placeable surface hit: {hitObject.name}");
                towerPos = hit.point + Vector3.up * snapHeight;
                pendingObj.transform.position = towerPos;
                canPlace = true;
            }
            else
            {
                // Unplaceable surface or other case
                Debug.Log($"Unplaceable Hit: {hitObject.name}");
                towerPos = hit.point + Vector3.up * snapHeight;
                pendingObj.transform.position = towerPos;
                canPlace = false;
            }
        }
        else
        {
            Debug.Log("Raycast hit nothing.");
            canPlace = false;
        }


    }


    public void PlaceObject()
    {
        if (pendingObj == null || !canPlace) return;

        pendingObj.GetComponent<MeshRenderer>().material = placementMats[2]; // Set to default material
        pendingObj = null;
    }

    public void SelectObject(GameObject prefab)
    {
        // Instantiate the pending object at the default position
        pendingObj = Instantiate(prefab, Vector3.zero, Quaternion.identity);
        pendingObj.GetComponent<MeshRenderer>().material = placementMats[1]; // Start with invalid placement material
    }

    void MaterialUpdate()
    {
        if (pendingObj == null) return;

        // Update material based on placement validity
        if (canPlace)
        {
            pendingObj.GetComponent<MeshRenderer>().material = placementMats[0]; // Green for valid placement
        }
        else
        {
            pendingObj.GetComponent<MeshRenderer>().material = placementMats[1]; // Red for invalid placement
        }
    }
#endregion


    void ButtonSpawner()
    {
        // Spawning buttons for each tower type
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
