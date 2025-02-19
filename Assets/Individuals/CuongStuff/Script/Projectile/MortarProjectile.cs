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

    private float momentum = 1f;
    private float dist, fracCurrent;
    private float blastradius = 5f;

    private bool Flying = false;

    //TEST
    private float maxCalculateDistance = 60f;
    private float minCalculateDistance = 5f;

    private float angleMin = -90f;
    private float angleMax = 90f;
    private float projectileMaxTime = 1.5f;
    private float projectileMinTime = 0.5f;

    private const float gravity = 9.81f;
    public bool test;
    private void Start()
    {
        layerMask = LayerMask.GetMask("Unplaceable");
    }

    // Flying along the trajectory
    private void FixedUpdate()
    {
        if (!test) return;
        Debug.Log(rb.velocity);
        if (!Flying) { return; }

        /*if (Vector3.Distance(transform.position, endPos) >= 5f)
        {
            Vector3 center = centerPos;
            float curve = 6 - (Mathf.Clamp(dist / 10f, 1f, 5f));
            center -= new Vector3(0, curve, 0);

            Vector3 riseRelCenter = originalPos - center;
            Vector3 setRelCenter = endPos - center;

            fracCurrent += (Speed + momentum) * Time.deltaTime;
            float fracComplete = fracCurrent / dist;
            
            // Clamp to avoid overshooting
            fracComplete = Mathf.Clamp01(fracComplete);

            Vector3 newPos = Vector3.Slerp(riseRelCenter, setRelCenter, fracComplete);
            Vector3 direction = (newPos + center) - endPos;

            if (fracComplete >= 0.4f)
                momentum += Speed * Time.deltaTime;

            if (direction != Vector3.zero)
            {
                Vector3 angle = direction.normalized;
                transform.rotation = Quaternion.LookRotation(-angle);
            }

            transform.position = newPos + center;
        }
        else
        {
            rb.velocity = transform.forward * (momentum + Speed);
        }*/

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
        /*RaycastHit hit;
        if (Physics.Raycast(endPos, Vector3.down, out hit, Mathf.Infinity, layerMask))
            endPos = hit.point;
            
        centerPos = (original + destination) * 0.5f;*/

        float distance = Vector3.Distance(originalPos, endPos);

        // Calculate angle and time based on distance
        float angle = CalculateAngle(distance);
        float time = CalculateTime(distance);

        // Compute velocity
        Vector3 velocity = CalculateVelocity(original, destination, angle, time);

        // Apply velocity to the Rigidbody
        rb.velocity = velocity;
        Debug.Log(rb.velocity);
    }

    private void OnDisable()
    {
        momentum = 1f;
        fracCurrent = 0f;
        rb.velocity = Vector3.zero;
        Flying = true;
    }

    private void SetExplosion()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, blastradius);
        
        AudioManager.Instance.PlaySoundEffect("Explosion_SFX");
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
            SetExplosion();
            transform.gameObject.SetActive(false);
            Flying = false;
        }
    }

    // DEBUG 
    private void OnDrawGizmos()
    {
        foreach (var point in EvaluateSlerpPoints(originalPos, endPos, centerPos, 20))
        {
            Gizmos.DrawSphere(point, 0.1f);
        }

        Gizmos.color = Color.red;
        Gizmos.DrawSphere(centerPos, 0.2f);
        Gizmos.DrawWireSphere(endPos, blastradius);
    }

    IEnumerable<Vector3> EvaluateSlerpPoints(Vector3 start, Vector3 end, Vector3 center, int count = 10)
    {
        float curve = 6 - (Mathf.Clamp(dist / 10f, 1f, 5f));
        center -= new Vector3(0, curve, 0);

        Vector3 riseRelCenter = originalPos - center;
        Vector3 setRelCenter = endPos - center;

        /*float fracComplete = Speed*Time.deltaTime;
        Vector3 newPos = Vector3.Slerp(riseRelCenter, setRelCenter, fracComplete);*/

        var f = 1f / count;

        for (var i = 0f; i < 1 + f; i += f)
        {
            yield return Vector3.Slerp(riseRelCenter, setRelCenter, i) + center;
        }
    }
}
