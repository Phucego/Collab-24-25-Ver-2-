using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InteractController : MonoBehaviour
{
    private LayerMask layerMask;
    private I_Interactable LatestInteract;
    [SerializeField] private EventSystem eventSystem;

    void Start()
    {
        layerMask = LayerMask.GetMask("Tower", "Enemy");
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            bool targethit = false;  

            // Check available colliders
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit[] hit = Physics.RaycastAll(ray, 150f, layerMask);
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

            // Check available canvas buttons 
            PointerEventData pointerEventData = new PointerEventData(eventSystem) { position = Input.mousePosition };
            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerEventData, results);

            if (results.Count > 0)
            {
                targethit = true;
            }

            if (LatestInteract != null && !targethit)
            {
                LatestInteract.Deselect();
                LatestInteract = null;
            }

        }
    }

}
