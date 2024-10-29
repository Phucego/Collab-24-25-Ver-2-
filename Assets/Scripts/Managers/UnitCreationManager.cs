using System;
using System.Collections;
using System.Collections.Generic;
using System.Transactions;
using UnityEngine;
using Random = UnityEngine.Random;


public class UnitCreationManager : MonoBehaviour
{
    private UnitManager _unitManager;
    private UnitSelectionManager _unitSelectionManager;
    
    public GameObject farmer_GO;
    public GameObject scout_GO;
    public GameObject builder_GO;

    public void OnGUI()
    {
        
        if (GUILayout.Button("Add Farmer"))
        {
            Instantiate(farmer_GO, transform.position, Quaternion.identity);
        }
        if (GUILayout.Button("Add Scout"))
        {
            Instantiate(scout_GO, transform.position, Quaternion.identity);
        }
        if (GUILayout.Button("Add Builder"))
        {
            Instantiate(builder_GO, transform.position, Quaternion.identity);
        }
    }
}
