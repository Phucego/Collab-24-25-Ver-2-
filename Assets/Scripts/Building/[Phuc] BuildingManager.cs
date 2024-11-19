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
    [SerializeField] private Material[] placementMats;
    public float rotationAMT;
    public delegate void OnCountCompleted(int i);
    public OnCountCompleted onCountCompleted;

    private Vector3 pos;
    RaycastHit hit;
    [SerializeField] private LayerMask placeableLayer;
    [SerializeField] private LayerMask otherLayer;

    public float snapHeight = 1.5f;
    public bool canPlace = true;

    private void Start()
    {
        // Spawning buttons on the screen based on the number of towers in the level data
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

    private void HandlePlacement()
    {
        if (Camera.main != null)
        {
            Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));

            if (Physics.Raycast(ray, out hit, Mathf.Infinity, placeableLayer))
            {
                // Valid placement position
                pos = hit.point + Vector3.up * snapHeight;
                pendingObj.transform.position = pos; // Snap to the valid position
                canPlace = true; // Allow placement
            }
            else if (Physics.Raycast(ray, out hit, Mathf.Infinity, otherLayer))
            {
                // Non-placeable layer hit, prevent placement
                canPlace = false;
            }
            else
            {
                // No valid layers hit, maintain the current position
                canPlace = false;
            }
        }
    }

    public void PlaceObject()
    {
        pendingObj.GetComponent<MeshRenderer>().material = placementMats[2];
        pendingObj = null;
    }

    public void SelectObject(GameObject prefab)
    {
        // Instantiate the pending object and immediately set its position based on the snapped position
        pendingObj = Instantiate(prefab, pos, Quaternion.identity);
        pendingObj.transform.position = pos; // Ensure it snaps to the raycast position immediately
    }

    void MaterialUpdate()
    {
        // Placement materials order:
        // 0 = green, 1 = red, 2 = default
        if (canPlace)
        {
            pendingObj.GetComponent<MeshRenderer>().material = placementMats[0]; // Green for valid placement
        }
        else
        {
            pendingObj.GetComponent<MeshRenderer>().material = placementMats[1]; // Red for invalid placement
        }
    }
}
