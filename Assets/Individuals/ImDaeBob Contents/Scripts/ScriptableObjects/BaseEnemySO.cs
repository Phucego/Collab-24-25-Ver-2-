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

[CreateAssetMenu(fileName = "Enemy_Type", menuName = "Data/EnemyDataSO")]
public class BaseEnemySO : ScriptableObject
{
    [Tooltip("Model")]
    public GameObject model;
    public Color color = Color.white;

    [Header("")]
    [Tooltip("Name")]
    public new string name;
    [Tooltip("Description")]
    public string description;
    [Tooltip("Enemy Type")]
    public List<eType> typing;

    [Header("")]
    [Tooltip("Currency dropped after defeating the enemy")]
    public int reward;

    [Header("")]
    [Tooltip("Health the enemy spawns with")]
    public int maxHealth;

    [Header("")]
    [Tooltip("How fast the enemy can run")]
    public float maxSpeed;
    [Tooltip("How fast can enemy start running to 'maxSpeed' and turn around corner!")]
    public float acceleration;
    [Tooltip("How fast does the enemy fall down to the ground")]
    public float gravity;
    [Tooltip("How far should the enemy be from ground")]
    public float levitation;

    private void OnEnable()
    {
        if (typing == null || typing.Count == 0)
            typing.Add(eType.Normal);
    }
}
