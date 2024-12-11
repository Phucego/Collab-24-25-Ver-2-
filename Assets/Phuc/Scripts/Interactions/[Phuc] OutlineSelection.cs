using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class OutlineSelection : MonoBehaviour
{
    private Transform highlight;
    private Transform selection;
    private RaycastHit raycastHit;
    private Camera cam;

    private BuildingManager _buildingManager;
    private bool hasPlacedTower;

    void Start()
    {
        cam = Camera.main;
        _buildingManager = FindObjectOfType<BuildingManager>();
        UpdatePlacementStatus();
    }

    void Update()
    {
        // Update hasPlacedTower status dynamically in case it changes during gameplay
        UpdatePlacementStatus();

        // Reset highlight
        if (highlight != null)
        {
            highlight.gameObject.GetComponent<OutlineScript>().enabled = false;
            highlight = null;
        }

       
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (!EventSystem.current.IsPointerOverGameObject() && Physics.Raycast(ray, out raycastHit))
        {
            Transform hitTransform = raycastHit.transform;

            // Only highlight towers that have been placed
            if (hitTransform.CompareTag("Tower") && hitTransform != selection)
            {
                TowerInteract towerInteract = hitTransform.GetComponent<TowerInteract>();
                if (towerInteract != null && towerInteract.isPlaced)
                {
                    highlight = hitTransform;

                    OutlineScript outline = highlight.gameObject.GetComponent<OutlineScript>();
                    if (outline == null)
                    {
                        outline = highlight.gameObject.AddComponent<OutlineScript>();
                        outline.OutlineColor = Color.white;
                        outline.OutlineWidth = 9.0f;
                    }
                    outline.enabled = true;
                }
            }
        }

        // Handle selection with the left mouse button if a tower is placed
        if (Input.GetMouseButtonDown(0) && hasPlacedTower)
        {
            if (highlight)
            {
                // Deselect previous selection
                if (selection != null)
                {
                    selection.gameObject.GetComponent<OutlineScript>().enabled = false;
                }

                // Select new object
                selection = highlight;
                selection.gameObject.GetComponent<OutlineScript>().enabled = true;
                highlight = null;
            }
            else
            {
                // Deselect current selection if clicking on empty space
                if (selection != null)
                {
                    selection.gameObject.GetComponent<OutlineScript>().enabled = false;
                    selection = null;
                }
            }
        }
    }


    // Updates the placement status based on the Building Manager conditions.
    private void UpdatePlacementStatus()
    {
        hasPlacedTower = _buildingManager != null && _buildingManager.pendingObj == null;
    }
}
