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

    private Vector3 towerPos;
    RaycastHit hit;
    [SerializeField] private LayerMask placeableLayer;
    [SerializeField] private LayerMask otherLayer;

    public float snapHeight = 1.5f;
    public bool canPlace;

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
   
        Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, placeableLayer))
        {
            towerPos = hit.point /*+ Vector3.up * snapHeight*/;
            pendingObj.transform.position = towerPos;    
           
        }
        else if(Physics.Raycast(ray, out hit, Mathf.Infinity, otherLayer))
        {
            towerPos = hit.point /*+ Vector3.up * snapHeight*/;
            canPlace = false;

        }
    }

    public void PlaceObject()
    {
        if (pendingObj == null) return;
        
        pendingObj.GetComponent<MeshRenderer>().material = placementMats[2];
        pendingObj = null;
    }

    public void SelectObject(GameObject prefab)
    {
        // Instantiate the pending object at the valid position
        pendingObj = Instantiate(prefab, towerPos, Quaternion.identity);
    }

    void MaterialUpdate()
    {
        if (pendingObj == null) return;

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
