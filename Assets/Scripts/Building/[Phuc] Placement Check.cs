using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlacementCheck : MonoBehaviour
{
    private BuildingManager buildingManager;
    // Start is called before the first frame update
    void Start()
    {
        buildingManager = GameObject.Find("BuildingManager").GetComponent<BuildingManager>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Selectable"))
        {
            buildingManager.canPlace = false;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Selectable"))
        {
            buildingManager.canPlace = true;
        }
    }

}
