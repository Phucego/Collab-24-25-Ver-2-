using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class CameraCrosshair : MonoBehaviour
{
    public Camera cam;

    public LayerMask towerLayer;

    [SerializeField] private TowerLogic selectedTower;

    private void Start()
    {
        cam = Camera.main;
    }
    // Update is called once per frame
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
            TowerLogic tower = hit.collider.GetComponent<TowerLogic>();

            if (tower != null)
            {
                Debug.Log("Interacted with tower: " + tower.name);
                selectedTower = tower;
               
                tower.OnInteract();
            }
        }
        else
        {
            Debug.Log("No tower hit!");
            selectedTower = null; // Clear selection if nothing was hit
        }
    }
}
