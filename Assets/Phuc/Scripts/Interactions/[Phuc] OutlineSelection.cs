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

        // Highlight management
        if (highlight != null)
        {
            highlight.gameObject.GetComponent<OutlineScript>().enabled = false;
            highlight = null;
        }

        // Fixed raycast from screen center
        Ray ray = cam.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        if (!EventSystem.current.IsPointerOverGameObject() && Physics.Raycast(ray, out raycastHit))
        {
            highlight = raycastHit.transform;

            if (highlight.CompareTag("Tower") && highlight != selection)
            {
                if (highlight.gameObject.GetComponent<OutlineScript>() != null)
                {
                    highlight.gameObject.GetComponent<OutlineScript>().enabled = true;
                }
                else
                {
                    OutlineScript outline = highlight.gameObject.AddComponent<OutlineScript>();
                    outline.enabled = true;
                    outline.OutlineColor = Color.white;
                    outline.OutlineWidth = 9.0f;
                }
            }
            else
            {
                highlight = null;
            }
        }

        // Toggle selection with the left mouse button if a tower is placed
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
                // Deselect current selection if clicking on the tower
                if (selection != null)
                {
                    selection.gameObject.GetComponent<OutlineScript>().enabled = false;
                    selection = null;
                }
            }
        }
    }

    //TODO: Update status based on the Building Manager conditions
    private void UpdatePlacementStatus()
    {
        hasPlacedTower = _buildingManager != null && _buildingManager.pendingObj == null;
    }
}
