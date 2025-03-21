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

    public static GameObject Spawn(string category, GameObject go, string name)
    {
        PoolingData poolData;
        string goFolderName = name;
        // Put object into a specific folder
        if (goFolderName == "")
            goFolderName = "_Fill";

        // Create new pool data if it has not existed
        if (!poolingDictionary.ContainsKey(category)) 
            poolingDictionary[category] = new PoolingData();

        poolData = poolingDictionary[category];
        
        // Find if game object already existed and ready for pooling
        for (int i = 0; i < poolData.deactiveList.Count; i++)
        {
            if (!poolData.deactiveList[i].activeInHierarchy)
            {
                poolData.activeList.Add(poolData.deactiveList[i]);
                poolData.deactiveList.Remove(poolData.deactiveList[i]);
                return poolData.deactiveList[i];
            }
        }

        GameObject newObject = GameObject.Instantiate(go, go.transform.position, Quaternion.identity, GameObject.Find(goFolderName).transform);
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
