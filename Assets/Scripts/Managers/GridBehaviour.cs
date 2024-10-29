using UnityEngine;

public class GridManager : MonoBehaviour
{
    public int width;
    public int height;
    public float cellSize;
    private GameObject[,] gridArray;
    public float offset;
    
    public LayerMask clickableLayer;
    void Start()
    {
        CreateGrid();
        
    }

    void CreateGrid()
    {
        gridArray = new GameObject[width, height];
        
        //TODO: Nested loop for grid creation
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                //TODO: Apply an offset to alternate rows
                float xOffset = (x % 2 == 0) ? 0 : offset;
                float zOffset = (z % 2 == 0) ? 0 : offset;
                
                //TODO: Adding offset to the cell size calculation for spaces between cells
                Vector3 worldPosition = new Vector3((x * cellSize) + xOffset, 0, (z * cellSize) + zOffset);
                
                //TODO: Create a grid cell at this position
                CreateGridCell(worldPosition, x, z);
                
            }
        }
    }

    void CreateGridCell(Vector3 worldPosition, int x, int z)
    {
        //TODO: Instantiate or create a visual for the grid cell 
        GameObject gridCell = GameObject.CreatePrimitive(PrimitiveType.Cube);
        gridCell.transform.position = worldPosition;
        gridCell.transform.localScale = new Vector3(cellSize, 0.1f, cellSize);
        
        //TODO: Assign the layer to the cells
        gridCell.layer = clickableLayer;
        
        gridArray[x, z] = gridCell;
    }
}