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

        // Slowly rotate torward enemies, only fire once the tower has full LOS of them
        if (lockedIn && Target != null)
        {
            float TargetSpd = Target.GetComponent<I_GetType>().GetSpeed();
            Vector3 PredictedPos = Target.transform.position + (Target.transform.forward * TargetSpd);
            TargetPos = Vector3.Slerp(Target.transform.position, PredictedPos, 0.1f);
            Vector3 dir = Head.transform.position - TargetPos;
            Quaternion desireddir = Quaternion.LookRotation(-dir);
            Head.transform.rotation = Quaternion.Slerp(Head.transform.rotation, desireddir, Time.deltaTime * 20f);
            
        }
    }

    protected override IEnumerator FireProjectile(Vector3 direction)
    {
        yield return new WaitForSeconds(0.5f);
        
        AudioManager.Instance.PlaySoundEffect("Ballista_SFX");
        lockedIn = false;
        GameObject Projectile = GetPooledObject();
        Projectile.transform.position = AimPoint.transform.position;
        Projectile.transform.rotation = AimPoint.transform.rotation;
        if (Projectile.GetComponent<HitscanController>() != null)
        {
            Projectile.GetComponent<HitscanController>().SetTarget(Target);
        }
        
        Projectile.SetActive(true);

    }

    protected override IEnumerator LOSCheck()
    {
        if (_EnemyList.Count <= 0) { yield return null; }
        lockedIn = true;
        bool targetFaced = false;
        RaycastHit hit;
        // Does the ray intersect any objects excluding the player layer
        while (!targetFaced)
        {
            StartCoroutine(FireProjectile(new Vector3(0,0,0)));
            targetFaced = true;
            yield return new WaitForSeconds(0.05f);
        }
        

        yield return new WaitForSeconds(0f);
    }
}
