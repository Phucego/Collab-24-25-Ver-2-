using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloudSpawner : MonoBehaviour
{
    [Header("Cloud Settings")]
    public GameObject[] cloudPrefabs;
    public int cloudCount = 10;             // Maximum number of active clouds
    public float spawnInterval = 2f;        // Time between each cloud spawn

    [Header("Cloud Movement")]
    public Transform target;                // Clouds will move toward this object
    public float minSpeed = 5f;
    public float maxSpeed = 8f;

    private List<GameObject> activeClouds = new List<GameObject>();

    void Start()
    {
        StartCoroutine(SpawnCloudsRoutine());
    }

    IEnumerator SpawnCloudsRoutine()
    {
        while (true)
        {
            if (activeClouds.Count < cloudCount)
            {
                SpawnCloud();
            }
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    public void SpawnCloud()
    {
        if (cloudPrefabs.Length == 0 || target == null) return;

        GameObject cloudPrefab = cloudPrefabs[Random.Range(0, cloudPrefabs.Length)];

        // Spawn at the manager’s position
        Vector3 spawnPos = transform.position;

        GameObject newCloud = Instantiate(cloudPrefab, spawnPos, Quaternion.identity);
        newCloud.transform.parent = transform;

        CloudMover mover = newCloud.AddComponent<CloudMover>();
        mover.Initialize(Random.Range(minSpeed, maxSpeed), target, this);

        activeClouds.Add(newCloud);
    }

    public void RemoveCloud(GameObject cloud)
    {
        activeClouds.Remove(cloud);
        Destroy(cloud);
    }
}