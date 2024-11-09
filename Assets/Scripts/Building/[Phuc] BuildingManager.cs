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

    public List<Vector3> placedTowers = new List<Vector3>();
    public float snapHeight = 1.5f;
    public bool canPlace = true;

    private void Start()
    {
        //TODO: Spawning buttons on the screen based on the number of towers put in the level data
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

        //StartCoroutine(CheckUpdate(UpdateCompleted));
    }

    /*private void UpdateCompleted(int i)
    {
        Debug.Log("Count Completed " + i);
        StartCoroutine(CheckUpdate(UpdateCompleted));
    }*/

    /*private IEnumerator CheckUpdate(OnCountCompleted onCompleted)
    {
        yield return new WaitForSeconds(5);
        onCompleted?.Invoke(3);
    }*/

    // Update is called once per frame
    void Update()
    {
        if (pendingObj != null)
        {
            pendingObj.transform.position = pos;

            if (Input.GetMouseButtonDown(0) && canPlace)
            {
                PlaceObject();
            }
            MaterialUpdate();
        }

        // Take the middle point of the screen instead of the mouse pos
        Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, placeableLayer))
        {
            pos = hit.point + Vector3.up * snapHeight;

            // Check if the current position is occupied
            if (IsOccupied())
            {
                canPlace = false;
            }
            else
            {
                canPlace = true;
            }
        }
        else if (Physics.Raycast(ray, out hit, Mathf.Infinity, otherLayer))
        {
            canPlace = false;
            pos = hit.point + Vector3.up * snapHeight;
        }
    }

    public void PlaceObject()
    {
        pendingObj.GetComponent<MeshRenderer>().material = placementMats[2];
        placedTowers.Add(pos);
        pendingObj = null;
    }

    public void SelectObject(GameObject prefab)
    {
        pendingObj = Instantiate(prefab, pos, transform.rotation);
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

    bool IsOccupied()
    {
        // Check if a tower is already placed at the given position
        foreach (Vector3 towerPosition in placedTowers)
        {
            // Tolerance for matching positions (1f can be adjusted for precision)
            if (Vector3.Distance(towerPosition, pos) < 1f)
            {
                return true; // Position is occupied
            }
        }
        return false; // Position is free
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawSphere(new Vector3(Screen.width / 2, Screen.height / 2, 0), 3f);
    }
}
