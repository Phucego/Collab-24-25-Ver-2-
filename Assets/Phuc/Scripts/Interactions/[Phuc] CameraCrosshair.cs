using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UI;

public class CameraCrosshair : MonoBehaviour
{
    public Camera cam;

    public LayerMask towerLayer;

    [SerializeField] private TowerInteract selectedTower;

    private void Start()
    {
        cam = Camera.main;
    }
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            TowerInteraction();
        }
    }

    void TowerInteraction()
    {
        // Create a ray from the center of the screen
        Ray ray = cam.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        RaycastHit hit;

        // Check if the ray hits a tower within range
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, towerLayer))
        {
            TowerInteract tower = hit.collider.GetComponent<TowerInteract>();

            if (tower != null)
            {
                Debug.Log("Interacted with tower: " + tower.name);

                // Toggle outline
                if (selectedTower == tower)
                {
                    tower.ToggleOutline(false); // Deselect if the same tower is clicked
                    selectedTower.TowerInfo(false);
                    selectedTower = null;
                    
                }
                else
                {
                    if (selectedTower != null)
                    {
                        selectedTower.ToggleOutline(false); // Turn off the previous selection
                        selectedTower.TowerInfo(false);
                    }

                    tower.ToggleOutline(true); // Highlight the new tower
                    selectedTower = tower;
                    selectedTower.TowerInfo(true);
                }
            }
        }
        else
        {
            Debug.Log("No tower hit!");

            // Clear selection and remove outline
            if (selectedTower != null)
            {
                selectedTower.ToggleOutline(false);
                selectedTower = null;
            }
        }

    }

 
}


