using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "EnemyData_DB", menuName = "New Enemy Data")]
public class BaseEnemySO : ScriptableObject
{
    public Color color = Color.white;
    public new string name;
    public string description;
    public bool towerTarget = false;
    public bool airborneType = false;

    public int maxHealth;
    public float maxSpeed;
    
}
