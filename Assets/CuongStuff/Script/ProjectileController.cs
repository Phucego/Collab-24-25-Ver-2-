using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class ProjectileController : MonoBehaviour
{
    protected Rigidbody rb;
    protected float Damage = 10f;
    [SerializeField] protected float Speed = 10f;

    private Vector3 shootDirection;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        rb.velocity = transform.forward * Speed;
    }

    public void SetDirection(Vector3 direction)
    {
        shootDirection = direction;
    }

    private void OnEnable()
    {
        rb.velocity = Vector3.zero;
    }


    private void OnCollisionEnter(Collision collision)
    {
        if (collision != null)
        {
            transform.gameObject.SetActive(false);
        }
    }
}
