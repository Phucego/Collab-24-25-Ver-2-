using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        //TODO: Add all the existing units in the scene to the list
        UnitSelectionManager.instance.allUnitsList.Add(gameObject);
        
        
      
    }

    private void OnDestroy()
    {
        //TODO: Remove the unit from the list when destroyed
        UnitSelectionManager.instance.allUnitsList.Remove(gameObject);
        
    }
}
