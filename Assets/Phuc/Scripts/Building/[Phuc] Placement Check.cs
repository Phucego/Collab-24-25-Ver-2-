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

    public bool isRayHit;
    
    // Start is called before the first frame update
    void Start()
    {
        buildingManager = GameObject.Find("BuildingManager").GetComponent<BuildingManager>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Tower"))
        {
            buildingManager.canPlace = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Tower"))
        {
            buildingManager.canPlace = false;
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

                // Check if the hit object is a tower
                if (hit.collider.CompareTag("Tower")) // Assumes towers are tagged as "Tower"
                {
                    Debug.Log("Ray hit a tower. Placement not allowed.");
                    buildingManager.canPlace = false;
                }
                else
                {
                    buildingManager.canPlace = true;
                }
            }
            else
            {
                Debug.DrawRay(transform.position, direction * rayDistance, Color.green);
                buildingManager.canPlace = true; // No obstruction
            }
        }
    }
    
    public bool allowPlacement = true;

    public bool CanPlace()
    {
        // Custom logic for placement rules
        return allowPlacement;
    }
}
