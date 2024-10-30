using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "EnemyData_DB", menuName = "Enemy/EnemyData")]
public class BaseEnemySO : ScriptableObject
{
    public new string name;
    public string description;
    public bool towerTarget = false;
    public bool airborneType = false;

    public int maxHealth;
    public float moveSpeed;
    
}
