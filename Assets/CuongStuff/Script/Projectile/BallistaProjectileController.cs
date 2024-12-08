using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallistaProjectileController : ProjectileController
{
    private TrailRenderer trailRenderer;
    private MeshRenderer meshRenderer;

    private bool Moving = true;

    protected override void Awake()
    {
        rb = GetComponent<Rigidbody>();
        trailRenderer = GetComponent<TrailRenderer>();
        meshRenderer = GetComponent<MeshRenderer>();
    }

    private void FixedUpdate()
    {
        if (Moving)
            rb.velocity = transform.forward * Speed;
    }

    private void OnEnable()
    {
        trailRenderer.Clear();
        Moving = true;
        meshRenderer.enabled = true;
        rb.isKinematic = false;
        rb.velocity = Vector3.zero;
    }

    private IEnumerator DisableObject()
    {
        Moving = false;
        meshRenderer.enabled = false;
        rb.isKinematic = true;
        yield return new WaitForSeconds(1);
        transform.gameObject.SetActive(false);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision != null)
        {
            if (collision.gameObject.CompareTag("Enemy"))
            {
                ApplyDamage(collision.gameObject);
            }
            StartCoroutine(DisableObject());   
        }
    }
}
