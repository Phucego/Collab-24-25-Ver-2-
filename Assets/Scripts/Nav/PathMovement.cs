using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PathMovement : MonoBehaviour
{
    [SerializeField] Transform _destination;
    Vector3 goal;
    
    NavMeshAgent _agent;

    void Start()
    {
        _agent = this.GetComponent<NavMeshAgent>();

        goal = _destination.transform.position;
        _agent.SetDestination(goal);
    }

    void Update()
    {
        if (this.transform.position == goal)
        {
            Destroy(this);
        }
    }
}
