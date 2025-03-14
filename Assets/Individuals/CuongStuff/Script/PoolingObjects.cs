using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static PoolingObjects;


public class PoolingObjects : MonoBehaviour
{
    
    
}

public class Pooling
{
    public static Dictionary<string, PoolingData> poolingDictionary = new Dictionary<string, PoolingData>();

    public static GameObject Spawn(string category, GameObject go)
    {
        PoolingData poolData;

        // Create new pool data if it has not existed
        if (!poolingDictionary.ContainsKey(category)) 
            poolingDictionary[category] = new PoolingData();

        poolData = poolingDictionary[category];
        
        // Find if game object already existed
        for (int i = 0; i < poolData.deactiveList.Count; i++)
        {
            if (!poolData.deactiveList[i].activeInHierarchy)
            {
                return poolData.deactiveList[i];
            }
        }

        GameObject newObject = GameObject.Instantiate(go, go.transform.position, Quaternion.identity, GameObject.Find("_Projectiles").transform);
        poolData.activeList.Add(newObject);
        return newObject;
        
    }

    public static void Despawn(string category, GameObject go)
    {
        PoolingData poolData;

        poolData = poolingDictionary[category];
        poolData.activeList.Remove(go);
        poolData.deactiveList.Add(go);
    }
}

public class PoolingData
{
    public List<GameObject> activeList = new List<GameObject>();
    public List<GameObject> deactiveList = new List<GameObject>();
}
