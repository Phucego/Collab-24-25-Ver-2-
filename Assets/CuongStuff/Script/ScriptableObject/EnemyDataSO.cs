using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyDataSO", menuName = "Data/EnemyDataSO")]
public class EnemyDataSO : ScriptableObject
{
    public Color color = Color.white;
    public new string name;
    public string description;
    public TargetType[] type;

    public int maxHealth;
    public float maxSpeed;

    private void OnEnable()
    {
        if (type == null || type.Length == 0)
            type = new TargetType[] { TargetType.Grounded };
    }

}
