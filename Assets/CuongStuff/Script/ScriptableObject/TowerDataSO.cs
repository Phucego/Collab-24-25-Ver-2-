using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum TowerTypeEnum
{
    Normal, // apply for all tower
    AOE, // Apply for AOE tower
    Chain
}

public enum TargetTypeEnum
{
    Grounded,
    Airborne,
    Invisible
}

public class TowerBaseStat { }


[CreateAssetMenu(fileName = "TowerDataSO", menuName = "Data/TowerDataSO")]
public class TowerDataSO : ScriptableObject
{
    public GameObject towerPrefab;
    public TowerTypeEnum TowerType;
    public List<TargetTypeEnum> TargetType;
    public float Damage;
    public float Health;
    public float Radius;
    public float FireRate;
    public float ProjectileSpeed;
    public float ProjectileRadius;
    public float ProjectileInterval;
    [Range(0,100)] public float CritChance;
    public float CritAmplifier = 1f;
    public int Cost;

    public List<UpgradeDataSO> listUpgrades;
}
