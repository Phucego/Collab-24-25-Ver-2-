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

    public float rotationAMT;
    public delegate void OnCountCompleted(int i);
    public OnCountCompleted onCountCompleted;

    private Vector3 pos;
    RaycastHit hit;
    [SerializeField] private LayerMask placeableLayer;

    private void Start()
    {
        for(int i = 0; i < LevelManager.instance.LevelDataSO.towerData.Length; i++)
        {
            int count = i;
            GameObject go = Instantiate(m_SpawnTowerButtonPrefab, m_SpawnTowerButtonParent);
            go.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Tower_" + count;
            go.GetComponent<Button>().onClick.AddListener(() =>
            {
                Debug.Log(count);
                SelectObject(LevelManager.instance.LevelDataSO.towerData[count].towerPrefab);
            });
        }

        StartCoroutine(CheckUpdate(UpdateCompleted));
    }

    private void UpdateCompleted(int i)
    {
        Debug.Log("Count Completed " + i);
        StartCoroutine(CheckUpdate(UpdateCompleted));
    }

    private IEnumerator CheckUpdate(OnCountCompleted onCompleted)
    {
        yield return new WaitForSeconds(5);
        onCompleted?.Invoke(3);
    }

    // Update is called once per frame
    void Update()
    {
        if (pendingObj != null)
        {
            pendingObj.transform.position = pos;

            if (Input.GetMouseButtonDown(0))
            {
                PlaceObject();

            }

        }
        //Take the middle point of the screen instead of the mouse pos
        Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));

        if(Physics.Raycast(ray, out hit, Mathf.Infinity, placeableLayer))
        {
            pos = hit.point;    
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            RotateObj();
        }
    }
    public void PlaceObject()
    {
        pendingObj = null; 
    }

    public void SelectObject(/*int index*/ GameObject prefab)
    {
        pendingObj = Instantiate(/*gameobjects[index]*/prefab, pos, transform.rotation);
    }

    public void RotateObj()
    {
        pendingObj.transform.Rotate(Vector3.up, rotationAMT);
    }
}
