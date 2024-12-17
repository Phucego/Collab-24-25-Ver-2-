using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MortarProjectile : ProjectileController
{
    private Vector3 originalPos;
    private Vector3 endPos;
    private Vector3 centerPos;

    private float momentum = 1f;
    private float dist, fracCurrent;
    private float blastradius = 5f;

    private bool Flying = false;

    // Flying along the trajectory
    private void FixedUpdate()
    {
        if (!Flying) { return; }

        if (Vector3.Distance(transform.position, endPos) >= 1f)
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
                momentum += (Speed*2) * Time.deltaTime;

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
        }
    }

    // Set start and end point of the lerp
    public void SetPositionLerp(Vector3 original, Vector3 destination)
    {
        originalPos = original;
        endPos = destination;
        centerPos = (original + destination) * 0.5f;
        
        dist = Vector3.Distance(original, destination);
    }

    private void OnEnable()
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
            // Some other AOE shit here    
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
