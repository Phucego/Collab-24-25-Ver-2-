using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractController : MonoBehaviour
{
    private LayerMask LayerMask;
    private I_Interactable LatestInteract;

    void Start()
    {
        LayerMask = LayerMask.GetMask("Tower", "Enemy");
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            bool targethit = false;  
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit[] hit = Physics.RaycastAll(ray, 150f, LayerMask);
            for (int i = 0; i < hit.Length; i++) 
            {
                if (hit[i].collider == null)
                    continue;
                if (hit[i].collider.isTrigger)
                    continue;
                if (hit[i].collider.GetComponent<I_Interactable>() == null)
                    continue;

                if (LatestInteract != hit[i].collider.GetComponent<I_Interactable>())
                {
                    if (LatestInteract != null)
                    {
                        LatestInteract.Deselect();
                        LatestInteract = null;
                    }
                    LatestInteract = hit[i].collider.GetComponent<I_Interactable>();
                    LatestInteract.Interact(Camera.main);
                    targethit = true;
                }
                
                break;
            }
            if (LatestInteract != null && !targethit)
            {
                LatestInteract.Deselect();
                LatestInteract = null;
            }
        }
    }

}
