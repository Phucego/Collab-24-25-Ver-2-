using UnityEngine;

public class CameraCrosshair : MonoBehaviour
{
    public Camera cam; 
    public LayerMask towerLayer; 

    [SerializeField] private TowerInteract selectedTower; 

    // Initialize the camera reference
    private void Start()
    {
        cam = Camera.main;
    }

    // Handle player input for tower interaction
    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
            TowerInteraction();
    }

   
    /// Handles interactions with towers when the player clicks.
    /// Ensures only placed towers can be interacted with.
    private void TowerInteraction()
    {
        // Cast a ray from the center of the screen
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, towerLayer))
        {
            TowerInteract tower = hit.collider.GetComponent<TowerInteract>();

            // Check if the hit object is a placed tower
            if (tower == null || !tower.isPlaced)
            {
                Debug.Log(tower == null ? "No tower hit!" : "Tower is pending placement!");
                ClearSelection(); // Deselect any currently selected tower
                return;
            }

            // Toggle selection if the same tower is clicked, or switch selection to a new tower
            if (selectedTower == tower)
            {
                ToggleSelection(tower, false);
                selectedTower = null;
            }
            else
            {
                ClearSelection();
                ToggleSelection(tower, true);
                selectedTower = tower;
            }
        }
        else
        {
            Debug.Log("No tower hit!");
            ClearSelection(); // Deselect if no tower is hit
        }
    }

   
    /// Toggles the selection state of a tower.
    private void ToggleSelection(TowerInteract tower, bool state)
    {
        tower.ToggleOutline(state); 
        tower.TowerInfo(state);    
    }

  
    //Clears the currently selected tower, if any.
    private void ClearSelection()
    {
        if (selectedTower != null)
        {
            ToggleSelection(selectedTower, false);
            selectedTower = null;
        }
    }
}
