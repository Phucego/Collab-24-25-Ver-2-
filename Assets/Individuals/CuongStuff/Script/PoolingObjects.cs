using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PoolingObjects : MonoBehaviour
{
    public static class Pooling
    {
        public Dictionary<string, PoolingData> poolingDictionary = new Dictionary<string, PoolingData>();

        public static List<GameObject> activeList = new List<GameObject>();
        public static List<GameObject> deactiveList = new List<GameObject>();

        public static void Spawn(string category, GameObject go)
        {
            // check logic de spawn them object hoac active object

            // active
            activeList.Add(go);
            deactiveList.Remove(go);

            // instantiate
            GameObject spawnGO = GameObject.Instantiate(go);
            activeList.Add(spawnGO);
        }

        public static void Despawn(GameObject go)
        {
            activeList.Remove(go);
            deactiveList.Add(go);
        }
    }
    public struct PoolingData
    {
        public List<GameObject> activeList = new List<GameObject>();
        public List<GameObject> deactiveList = new List<GameObject>();
    }
}
