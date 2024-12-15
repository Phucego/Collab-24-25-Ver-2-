using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum eType
{
    Normal,
    Airborne,
    Invisible,
    Retaliate,
    Mini_Boss,
    Boss
}

[CreateAssetMenu(fileName = "EnemyData_DB", menuName = "New Enemy Data")]
public class BaseEnemySO : ScriptableObject
{
    [Tooltip("Temporary Decor")]
    public Color color = Color.white;

    [Tooltip("Name")]
    public new string name;
    [Tooltip("Description")]
    public string description;
    [Tooltip("Enemy Type")]
    public eType[] type;

    [Tooltip("Health the enemy spawns with")]
    public int maxHealth;
    [Tooltip("How fast the enemy can run")]
    public float maxSpeed;
    [Tooltip("How fast can enemy start running to 'maxSpeed' and turn around corner!")]
    public float acceleration;
    [Tooltip("How fast does the enemy fall down to the ground")]
    public float gravity;

    private void OnEnable()
    {
        if (type == null || type.Length == 0)
            type = new eType[] { eType.Normal };
    }
}
