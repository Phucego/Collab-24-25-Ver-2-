using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BalistaController : TowerController
{
    public int bulletIndex = 0;
    private bool lockedIn = false;

    protected override void Update()
    {
        base.Update();
        if (lockedIn)
        {
            TargetPos = Target.transform.position;
            Vector3 dir = Head.transform.position - TargetPos;
            Quaternion desireddir = Quaternion.LookRotation(-dir);
            Head.transform.rotation = Quaternion.Slerp(Head.transform.rotation, desireddir, Time.deltaTime * 20f);
        }
    }

    protected override IEnumerator FireProjectile(Vector3 direction)
    {
        yield return new WaitForSeconds(0.5f);

        lockedIn = false;
        GameObject Projectile = GetPooledObject();
        Projectile.transform.position = AimPoint.transform.position;
        Projectile.transform.rotation = AimPoint.transform.rotation;
        SetStat(Projectile);
        if (Projectile.GetComponent<HitscanController>() != null)
        {
            Projectile.GetComponent<HitscanController>().SetTarget(Target);
        }
        
        Projectile.SetActive(true);

    }

    protected override IEnumerator LOSCheck()
    {
        TargetPos = Target.transform.position;
        lockedIn = true;
        bool targetFaced = false;
        RaycastHit hit;
        // Does the ray intersect any objects excluding the player layer
        while (!targetFaced)
        {
            if (Physics.Raycast(AimPoint.transform.position, AimPoint.transform.TransformDirection(Vector3.forward), out hit, Mathf.Infinity, layerMask))
            {
                //Debug.DrawRay(AimPoint.transform.position, AimPoint.transform.TransformDirection(Vector3.forward) * hit.distance, Color.yellow);
                StartCoroutine(FireProjectile(hit.transform.position - AimPoint.transform.position));
                targetFaced = true;
            }
            yield return new WaitForSeconds(0.05f);
        }
        

        yield return new WaitForSeconds(0f);
    }
}
