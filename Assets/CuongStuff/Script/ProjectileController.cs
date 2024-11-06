using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class ProjectileController : MonoBehaviour
{
    protected Rigidbody rb;
    protected float Speed = 10f;
    protected float Damage = 10f;
    private Vector3 shootDirection;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //rb.AddForce(forwardVector * Speed, ForceMode.Impulse);
        rb.velocity = transform.forward * Speed;
    }

    public void SetDirection(Vector3 direction)
    {
        shootDirection = direction;
    }


    protected void OnCollisionEnter(Collision collision)
    {
        if (collision != null)
        {
            transform.gameObject.SetActive(false);
        }
    }
}
