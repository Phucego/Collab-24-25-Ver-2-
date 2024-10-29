using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class UnitMovement : MonoBehaviour
{
    private Camera cam;

    private NavMeshAgent _navMeshAgent;

    public LayerMask groundLayer;

   
    // Start is called before the first frame update
    void Start()
    {
        cam = Camera.main;
        _navMeshAgent = GetComponent<NavMeshAgent>();
        
    }

    // Update is called once per frame
    void Update()
    {
        //TODO: Actions when the player hits the right mouse button
        if (Input.GetMouseButtonDown(1))
        {
            RaycastHit hit;
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            
            //Shoot a ray with the infinite + distance, if it hits the correct layer, the unit moves
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, groundLayer))
            {
                _navMeshAgent.SetDestination(hit.point);
            }
        }
    }
}
