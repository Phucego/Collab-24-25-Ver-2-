using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelData", menuName = "Data/LevelData")]
public class LevelDataSO : ScriptableObject
{
    public TowerDataSO[] towerData;
}