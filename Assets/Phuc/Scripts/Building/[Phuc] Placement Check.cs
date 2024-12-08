using System;
using UnityEngine;

public class PlacementCheck : MonoBehaviour
{
    [SerializeField]
    private BuildingManager buildingManager;

    public float rayDistance = 10f; // Distance each ray should travel
    public int numberOfRays = 10;   // Total rays
    private float angleStep;        // Angle between each ray
    public bool allowPlacement = true; // Indicates if placement is allowed

    void Start()
    {
        buildingManager = GameObject.Find("BuildingManager").GetComponent<BuildingManager>();
        angleStep = 360f / numberOfRays; // Divide 360 degrees by the number of rays
    }

    void Update()
    {
        allowPlacement = PerformRayCheck();
    }

    private bool PerformRayCheck()
    {
        for (int i = 0; i < numberOfRays; i++)
        {
            float angle = i * angleStep; // Calculate angle for each ray
            Vector3 direction = Quaternion.Euler(0, angle, 0) * transform.forward; // Rotate forward vector by angle
            Ray ray = new Ray(transform.position, direction);

            if (Physics.Raycast(ray, out RaycastHit hit, rayDistance))
            {
                Debug.DrawLine(transform.position, hit.point, Color.red);

                // Check if the hit object is a tower
                if (hit.collider.CompareTag("Tower")) // Assumes towers are tagged as "Tower"
                {
                    return buildingManager.canPlace = false; // If any ray hits a tower, placement is not allowed
                }
            }
            else
            {
                Debug.DrawRay(transform.position, direction * rayDistance, Color.green);
            }
        }
        return true; // No rays hit a tower, placement is allowed
    }

    public bool CanPlace()
    {
        return buildingManager.canPlace; // Return the current placement status
    }
}