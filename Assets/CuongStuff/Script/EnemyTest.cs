using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyTest : MonoBehaviour, I_GetType, I_Damagable
{
    public EnemyDataSO EnemyData;
    public TargetTypeEnum[] EnemyType = new TargetTypeEnum[] { };

    public void ApplyDebuff(float smth)
    {
        throw new System.NotImplementedException();
    }

    public void Awake()
    {
        EnemyType = EnemyData.type;
    }
    public TargetTypeEnum[] GetTargetType() { return EnemyType; }

    public void TakeDamage(float damage)
    {
        Debug.Log("Damage taken: " +damage);
    }
}
