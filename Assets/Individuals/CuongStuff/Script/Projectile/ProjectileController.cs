using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileController : MonoBehaviour, I_TowerProjectile
{
    protected Rigidbody rb;
    protected float Damage = 10f;
    protected float CritChance = 0f;
    protected float CritAmp = 1f;
    [SerializeField] protected float Speed = 10f;

    protected bool collisionCount = false;

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
        collisionCount = false;
        rb.velocity = Vector3.zero;
    }

    private void OnTriggerEnter(Collider collision)
    {
        if (collision != null && !collisionCount && !collision.gameObject.CompareTag("MainCamera"))
        {
            collisionCount = true;
            if (collision.gameObject.CompareTag("Enemy"))
            {
                ApplyDamage(collision.gameObject);
            }
            GameObject particle = ParticlesManager.Instance.SpawnParticles(1, "CannonExplode");
            particle.transform.position = collision.ClosestPoint(transform.position);
            particle.SetActive(true);

            Pooling.Despawn("CannonBall", gameObject);
            transform.gameObject.SetActive(false);     
        }
    }

    protected virtual void ApplyDamage(GameObject target)
    {

        target.GetComponent<I_Damagable>().TakeDamage(Damage);
    }

    public virtual void SetDamage(float dmg)
    {
        Damage = dmg;
    }

    public virtual void SetDebuff(float duration)
    {
        
    }

    public virtual void SetRadius(float radius)
    {
        
    }
}
