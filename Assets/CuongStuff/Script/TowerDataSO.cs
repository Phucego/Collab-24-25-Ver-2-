using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum TowerType
{
    Normal, // apply for all tower
    AOE, // Apply for AOE tower
    Chain
}

public enum TargetType
{
    Grounded,
    Flying,
    Invisible
}

[CreateAssetMenu(fileName = "TowerDataSO", menuName = "Data/TowerDataSO")]
public class TowerDataSO : ScriptableObject
{
    // attributes of tower
    // damage, health, range, AOE
    public TowerType TowerType;
    public List<TargetType> TargetType;
    public float Damage;
    public float Health;
    public float Radius;
    public float FireRate;
    public float ProjectileSpeed;

    public List<UpgradeDataSO> listUpgrades;
}
