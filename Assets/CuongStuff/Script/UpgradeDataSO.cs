using System;
using System.Collections.Generic;
using UnityEngine;

public enum UpgradeType
{
    Health,
    Damage,
    Radius,
    FireRate,
    AOE
}

[CreateAssetMenu(fileName = "UpgradeDataSO", menuName = "Data/UpgradeDataSO")]
public class UpgradeDataSO : ScriptableObject
{
    public string upgradeName;
    public List<UpgradeData> upgradeDatas;
}

[Serializable]
public class UpgradeData
{
    public UpgradeType upgradeType;
    public float value;
}