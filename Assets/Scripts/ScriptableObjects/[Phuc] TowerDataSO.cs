using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "Tower_DB", menuName = "Tower Data/Tower") ]
public class TowerDataSO : ScriptableObject
{
    public int towerId;
    public string towerName;
    public int towerHp;
    public GameObject towerPrefab;
}
