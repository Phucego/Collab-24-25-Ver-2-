using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    [Header("Spawnpoint:")]
    [Tooltip("[OR] Get spawn location via Object")]
    [SerializeField] Transform _spawnLocation;
    [Tooltip("[OR] Get spawn location via direct coordinate")]
    [SerializeField] Vector3 _spawnLocationViaCoord = new Vector3(0, 0, 0);

    [Header("Enemy Data:")]
    [SerializeField] GameObject _enemyPrefab;
    private List<GameObject> eList = new List<GameObject>();
    [SerializeField] BaseEnemySO[] _dataList;

    [Header("Camera")]
    [SerializeField] private Camera mainCamera;

    void Start()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        if (_spawnLocation == null)
        {
            GameObject tempSpawnPoint = new GameObject("TempSpawnPoint");
            tempSpawnPoint.transform.position = _spawnLocationViaCoord;
            _spawnLocation = tempSpawnPoint.transform;
        }

        if (mainCamera == null)
            mainCamera = Camera.main;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Z))
        {
            GameObject anEnemy = Instantiate(_enemyPrefab, _spawnLocation.position, _spawnLocation.rotation);

            EnemyBehavior enemyBehavior = anEnemy.GetComponent<EnemyBehavior>();
            if (enemyBehavior != null && _dataList.Length > 0)
                enemyBehavior._data = _dataList[Random.Range(0, _dataList.Length)];
            Capture(anEnemy);
            eList.Add(anEnemy);

            //Debug.Log("An Enemy has spawned at " + _spawnLocation.position + "!");
        }

        if (Input.GetMouseButtonDown(2))
        {
            CheckENull();
            SetDestination();
        }
    }

    private void Capture(GameObject obj)
    {
        obj.transform.parent = this.transform;
    }

    private void CheckENull()
    {
        for (int i = eList.Count - 1; i >= 0; i--)
        {
            if (eList[i] == null)
                eList.RemoveAt(i);
        }
    }

    private void SetDestination()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Vector3 targetPosition = hit.point;

            foreach (var e in eList)
                e.GetComponent<EnemyBehavior>().SetDestination(targetPosition);
        }
    }
}