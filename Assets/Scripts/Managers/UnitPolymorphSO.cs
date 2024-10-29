using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
[CreateAssetMenu(menuName = "UnitSO/Unit")]
public class UnitPolymorphSO : ScriptableObject
{
    [SerializeReference]
    public List<UnitPolymorphicList> unitList = new List<UnitPolymorphicList>();

    
}
