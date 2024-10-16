using UnityEngine;

[RequireComponent(typeof(Camera))] // Ensure the script is attached to a camera
public class CameraBounds : MonoBehaviour
{
    [SerializeField] private MapBoundary mapBoundary; // Reference to MapBoundary script
    private Vector3 halfSize; // Half-size of the boundary for clamping

    private void Start()
    {
        
        if (mapBoundary == null)
        {
            Debug.LogError("No MapBoundary found in the scene.");
            return;
        }

        // Calculate the half size of the boundary for clamping
        halfSize = mapBoundary.size / 2;
    }
    /// <summary>
    /// Make sure the clamping happens after
    /// </summary>
    private void LateUpdate()
    {
        if (mapBoundary != null)
            ClampCameraPosition();
    }

    private void ClampCameraPosition()
    {
        Vector3 pos = transform.position;

        // Clamp the camera's position within the MapBoundary
        pos.x = Mathf.Clamp(pos.x, 
            mapBoundary.center.x - halfSize.x, 
            mapBoundary.center.x + halfSize.x);
        
        pos.y = Mathf.Clamp(pos.y, 
            mapBoundary.center.y - halfSize.y, 
            mapBoundary.center.y + halfSize.y);
        
        pos.z = Mathf.Clamp(pos.z, 
            mapBoundary.center.z - halfSize.z, 
            mapBoundary.center.z + halfSize.z);

        // Apply the clamped position
        transform.position = pos;
    }
}