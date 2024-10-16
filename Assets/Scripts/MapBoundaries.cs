using UnityEngine;

public class MapBoundary : MonoBehaviour
{
    [Header("Boundary Settings")]
    public Vector3 center = Vector3.zero; // Center of the boundary
    public Vector3 size = new Vector3(50f, 10f, 50f); // Width, Height, Depth of the boundary
    private Vector3 halfSize; 
   
    private void OnDrawGizmos()
    {
        // Visualize the boundary box in the Scene view
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(center, size);
    }

    private void Start()
    {
        CreateBoundaryWalls();
        
        halfSize = size / 2f;
      
    }


    #region Functions

    private void CreateBoundaryWalls()
    {
        Vector3 halfSize = size / 2;

        // Create 4 walls (Top, Bottom, Left, Right)
        CreateWall("TopWall", new Vector3(center.x, center.y, center.z + halfSize.z), 
            new Vector3(size.x, size.y, 1f));
        CreateWall("BottomWall", new Vector3(center.x, center.y, center.z - halfSize.z), 
            new Vector3(size.x, size.y, 1f));
        CreateWall("LeftWall", new Vector3(center.x - halfSize.x, center.y, center.z), 
            new Vector3(1f, size.y, size.z));
        CreateWall("RightWall", new Vector3(center.x + halfSize.x, center.y, center.z), 
            new Vector3(1f, size.y, size.z));
    }

    private void CreateWall(string name, Vector3 position, Vector3 scale)
    {
        GameObject wall = new GameObject(name);
        wall.transform.position = position;
        wall.transform.localScale = scale;

        // Add collider to make it solid
        BoxCollider collider = wall.AddComponent<BoxCollider>();
        collider.isTrigger = false;

        // Optional: Add a visible mesh (if you want to see the walls)
        MeshRenderer renderer = wall.AddComponent<MeshRenderer>();
        renderer.material = new Material(Shader.Find("Standard"));
        renderer.material.color = Color.red;
    }

    #endregion
  
}