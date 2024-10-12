using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "EnemyData_DB", menuName = "Enemy/EnemyData")]
public class BaseEnemySO : ScriptableObject
{
    public int health;
    public int maxHealth;
    
    public int moveSpeed;
    
    
}
