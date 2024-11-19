using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlacementCheck : MonoBehaviour
{
    private BuildingManager buildingManager;


    public float rayDistance = 10f; // Distance each ray should travel
    public int numberOfRays = 10;   // Total rays
    private float angleStep = 36f;  // Angle between each ray

    // Start is called before the first frame update
    void Start()
    {
        buildingManager = GameObject.Find("BuildingManager").GetComponent<BuildingManager>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Tower"))
        {
            buildingManager.canPlace = false;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Tower"))
        {
            buildingManager.canPlace = true;
        }
    }
    
    void Update()
    {
        for (int i = 0; i < numberOfRays; i++)
        {
            float angle = i * angleStep; // Calculate angle for each ray (0, 36, 72, etc.)
            Vector3 direction = Quaternion.Euler(0, angle, 0) * transform.forward; // Rotate forward vector by angle
            Ray ray = new Ray(transform.position, direction);

            // Cast the ray
            if (Physics.Raycast(ray, out RaycastHit hit, rayDistance))
            {
                Debug.DrawLine(transform.position, hit.point, Color.red);
                // Do something with the hit information
                buildingManager.canPlace = false;
            }
            else
            {
                Debug.DrawRay(transform.position, direction * rayDistance, Color.green);
                buildingManager.canPlace = true;
            }
        }
    }
}
