using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MortarProjectile : ProjectileController
{
    private Vector3 originalPos;
    private Vector3 endPos;
    private Vector3 centerPos;

    private LayerMask layerMask;

    private float distt;
    private float blastradius = 5f;

    private bool Flying = false;

    //TEST
    private float maxCalculateDistance = 24f;
    private float minCalculateDistance = 2f;

    private float angleMin = -270f;
    private float angleMax = 270;
    private float projectileMaxTime = 1.5f;
    private float projectileMinTime = 0.5f;

    private const float gravity = 9.81f;
    public bool test;
    private void Start()
    {
        layerMask = LayerMask.GetMask("Unplaceable");
    }

    // Drop down physics
    private void FixedUpdate()
    {
        if (!Flying) { return; }

        rb.AddForce(Vector3.down * gravity * rb.mass, ForceMode.Force);

    }

    /// <summary>
    /// Calculates the launch angle based on the target distance.
    /// </summary>
    private float CalculateAngle(float distance)
    {
        if (distance >= maxCalculateDistance)
            return angleMin;
        if (distance <= minCalculateDistance)
            return angleMax;

        float t = (distance - minCalculateDistance) / (maxCalculateDistance - minCalculateDistance);
        return Mathf.Lerp(angleMax, angleMin, t);
    }

    /// <summary>
    /// Calculates the time for the projectile to reach the target.
    /// </summary>
    private float CalculateTime(float distance)
    {
        if (distance >= maxCalculateDistance)
            return projectileMaxTime;
        if (distance <= minCalculateDistance)
            return projectileMinTime;

        float t = (distance - minCalculateDistance) / (maxCalculateDistance - minCalculateDistance);
        return Mathf.Lerp(projectileMinTime, projectileMaxTime, t);
    }

    /// <summary>
    /// Calculates the initial velocity required for the projectile.
    /// </summary>
    private Vector3 CalculateVelocity(Vector3 start, Vector3 target, float angle, float time)
    {
        Vector3 direction = (target - start).normalized;
        float horizontalDistance = Vector3.Distance(new Vector3(start.x, 0, start.z), new Vector3(target.x, 0, target.z));

        float verticalVelocity = (gravity * time) / 2f;
        float horizontalVelocity = horizontalDistance / time;

        Vector3 velocity = direction * horizontalVelocity;
        velocity.y = verticalVelocity;

        return velocity;
    }


    // Set start and end point of the lerp
    public void SetPositionLerp(Vector3 original, Vector3 destination)
    {        
        originalPos = original;
        endPos = destination;

        float distance = Vector3.Distance(originalPos, endPos);

        // Calculate angle and time based on distance
        float angle = CalculateAngle(distance);
        float time = CalculateTime(distance);

        // Compute velocity
        Vector3 velocity = CalculateVelocity(original, destination, angle, time);

        // Apply velocity to the Rigidbody
        rb.velocity = velocity;
    }

    private void OnEnable()
    {
        rb.velocity = Vector3.zero;
        Flying = true;
    }

    private void SetExplosion()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, blastradius);
        
        AudioManager.Instance.PlaySoundEffect("Explosion_SFX");
        GameObject particle = ParticlesManager.Instance.SpawnParticles(3, "MortarExplosion");
        particle.transform.position = transform.position;
        particle.SetActive(true);
        foreach (var collider in colliders)
        {
            if (collider.gameObject.CompareTag("Enemy"))
            {
                ApplyDamage(collider.gameObject);
            }
        }
    }

    public override void SetRadius(float radius)
    {
        blastradius = radius;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision != null)
        {
            Flying = false;
            SetExplosion();
            Pooling.Despawn("MortarBall", gameObject);
            transform.gameObject.SetActive(false);
        }
    }

    // DEBUG 
    private void OnDrawGizmos()
    {
        foreach (var point in EvaluateSlerpPoints(originalPos, endPos, 20))
        {
            Gizmos.DrawSphere(point, 0.1f);
        }

        Gizmos.color = Color.red;
        Gizmos.DrawSphere(centerPos, 0.2f);
        Gizmos.DrawWireSphere(endPos, blastradius);
    }

    IEnumerable<Vector3> EvaluateSlerpPoints(Vector3 start, Vector3 end, int count = 10)
    {
        float distance = Vector3.Distance(start, end);

        // Calculate angle and time based on distance
        float angle = CalculateAngle(distance);
        float time = CalculateTime(distance);

        // Compute velocity
        Vector3 velocity = CalculateVelocity(start, end, angle, time);

        var f = 1f / count;

        for (var i = 0f; i < 1 + f; i += f)
        {
            yield return velocity;
        }
    }
}
