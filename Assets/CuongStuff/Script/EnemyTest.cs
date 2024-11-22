using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyTest : MonoBehaviour, I_GetType
{
    public EnemyDataSO EnemyData;
    public TargetType[] EnemyType = new TargetType[] { };

    public void Awake()
    {
        EnemyType = EnemyData.type;
    }
    public TargetType[] GetTargetType() { return EnemyType; }
}
