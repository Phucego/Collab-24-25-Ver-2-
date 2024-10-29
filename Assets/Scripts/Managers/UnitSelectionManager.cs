using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitSelectionManager : MonoBehaviour
{
    public static UnitSelectionManager instance { get; set; }

    public List<GameObject> allUnitsList      = new List<GameObject>();
    public List<GameObject> selectedUnitsList = new List<GameObject>();
    
    
    
    private Camera cam;

    [SerializeField] private LayerMask clickableLayer;

    [SerializeField] private LayerMask groundLayer;

    public GameObject movementIndicator;
  
 
   
    //TODO: Prevent creating multiple instances of a manager
    void Awake()
    {
        //If there are multiple instances, destroy the copy of the manager if there is any in the scene
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
        }
    }

    private void Start()
    {
        cam = Camera.main;

        DisableUnitMovementWhenStart();
        movementIndicator.SetActive(false);
    }

    private void Update()
    {
        //TODO: Actions when the player hits the left mouse button
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            //Shoot a ray with the infinite + distance, if it hits the correct layer, the unit is selected
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, clickableLayer))
            {
                //If the player holds shift while choosing units, the player can choose multiple units
                if (Input.GetKey(KeyCode.LeftShift))
                {
                    MultipleUnitSelection(hit.collider.gameObject);
                }
                //If not, then the player can choose only one per click
                else
                {
                    DeselectAllUnits(); //Added this so that the list of the selected units does not stack up before
                                        //adding a new unit to the list
                    SelectUnitOnClick(hit.collider.gameObject);
                }
            }
            //TODO: Deselect the units when clicking outside 
            else
            {
                if (!Input.GetKey(KeyCode.LeftShift))
                {
                    DeselectAllUnits();
                }
            }
            
        }

        //TODO: Create an indicator to show where the unit(s) will move (if there are any units)
        if (Input.GetMouseButtonDown(1) && selectedUnitsList.Count > 0)
        {
            RaycastHit hit;
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);

            //Set the indicator to the point that the player presses 
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, groundLayer))
            {
                movementIndicator.transform.position = hit.point;
                movementIndicator.SetActive(false);
                movementIndicator.SetActive(true);
            }
        }


        #region Unit Selection and Deselection Functions

        void MultipleUnitSelection(GameObject unit)
        {
            //Add the unit to the list if selected, and there are no units in the list
            if (selectedUnitsList.Contains(unit) == false)
            {
                selectedUnitsList.Add(unit);
                TriggerUnitFunctions(unit, true);
            }
            //Deselect when press on the unit that has been selected
            else
            {
                TriggerUnitFunctions(unit, false);
                selectedUnitsList.Remove(unit);
            }
        }

        //TODO: This section is for select and deselect units
        void SelectUnitOnClick(GameObject unit)
        {
            selectedUnitsList.Add(unit);
            TriggerUnitFunctions(unit, true);
        }
        #endregion
    }
    public void DragSelect(GameObject unit)
    {
        //TODO: Check if there is any unit in the list, if not, add it to the list 
        if (selectedUnitsList.Contains(unit) == false)
        {
            selectedUnitsList.Add(unit);
            TriggerUnitFunctions(unit, true);
        }
    }
    public void DeselectAllUnits()
    {
        //TODO: Check through all the units in the list, and disable all the movements
        foreach (var unit in selectedUnitsList)
        {
            
            TriggerUnitFunctions(unit, false);
                
        }
        movementIndicator.SetActive(false);
            
        //Then clear the list
        selectedUnitsList.Clear();
    }
    #region Unit Functions
    void EnableUnitMovement(GameObject unit, bool canMove)
    {
        //TODO: Getting the movement script when the boolean is true
        unit.GetComponent<UnitMovement>().enabled = canMove;
    }
    
    void DisableUnitMovementWhenStart()
    {
        //TODO: Check the units in the total lists and disable movements when start the game
        foreach (var unit in allUnitsList)
        {
            TriggerSelectionIndicator(unit, false);
            EnableUnitMovement(unit, false);
        }
    }
    
    void TriggerSelectionIndicator(GameObject unit, bool isVisible)
    {
        unit.transform.GetChild(0).gameObject.SetActive(isVisible);
    }

    void TriggerUnitFunctions(GameObject unit, bool isSelected)
    {
        TriggerSelectionIndicator(unit, isSelected);
        EnableUnitMovement(unit, isSelected);
    }

   
        
    #endregion

}
