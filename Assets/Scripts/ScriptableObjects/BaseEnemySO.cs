using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum eType
{
    Peon,
    Airborne,
    Invisible,
    Retaliate,
    Mini_Boss,
    Boss
}

[CreateAssetMenu(fileName = "EnemyData_DB", menuName = "New Enemy Data")]
public class BaseEnemySO : ScriptableObject
{
    public Color color = Color.white;
    public new string name;
    public string description;
    public eType[] type;

    public int maxHealth;
    public float maxSpeed;

    private void OnEnable()
    {
        if (type == null || type.Length == 0)
            type = new eType[] { eType.Peon };
    }
}
