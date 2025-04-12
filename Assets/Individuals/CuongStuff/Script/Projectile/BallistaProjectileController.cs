using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallistaProjectileController : ProjectileController
{
    private TrailRenderer trailRenderer;
    private MeshRenderer meshRenderer;
    private SphereCollider sphereCollider;

    private bool Moving = true;

    protected override void Awake()
    {
        rb = GetComponent<Rigidbody>();
        trailRenderer = GetComponent<TrailRenderer>();
        meshRenderer = GetComponent<MeshRenderer>();
        sphereCollider = GetComponent<SphereCollider>();
        trailRenderer.Clear();
        Moving = false;
        meshRenderer.enabled = false;
        trailRenderer.enabled = false;
        rb.isKinematic = true;
    }

    private void FixedUpdate()
    {
        if (Moving)
            rb.velocity = transform.forward * Speed;
    }

    private void OnEnable()
    {
        collisionCount = false;
        trailRenderer.Clear();
        Moving = true;
        meshRenderer.enabled = true;
        trailRenderer.enabled = true;
        sphereCollider.enabled = true;
        rb.isKinematic = false;
        rb.velocity = Vector3.zero;
    }

    private IEnumerator DisableObject()
    {
        Moving = false;
        meshRenderer.enabled = false;
        sphereCollider.enabled = false;
        rb.isKinematic = true;
        yield return new WaitForSeconds(1);
        Pooling.Despawn("BallistaArrow", gameObject);
        transform.gameObject.SetActive(false);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision != null && !collisionCount)
        {
            StartCoroutine(DisableObject());
            collisionCount = true;
            if (collision.gameObject.CompareTag("Enemy"))
            {
                ApplyDamage(collision.gameObject);
            }
            GameObject Particle = ParticlesManager.Instance.SpawnParticles(0, "BallistaExplosion");
            Particle.transform.position = transform.position;
            Particle.SetActive(true);
        }
    }
}
