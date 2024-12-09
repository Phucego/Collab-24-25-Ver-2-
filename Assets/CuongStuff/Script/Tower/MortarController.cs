using System.Collections;
using UnityEngine;

public class MortarController : TowerController
{
    protected override IEnumerator FireProjectile(Vector3 direction)
    {
        GameObject Projectile = GetPooledObject();
        Projectile.transform.position = AimPoint.transform.position;
        Projectile.transform.rotation = AimPoint.transform.rotation;
        SetStat(Projectile);
        Projectile.GetComponent<MortarProjectile>().SetPositionLerp(AimPoint.transform.position, TargetPos);
        Projectile.SetActive(true);

        yield return null;
    }

    protected override IEnumerator LOSCheck()
    {
        TargetPos = Target.transform.position;
        Head.transform.LookAt(TargetPos);

        StartCoroutine(FireProjectile(new Vector3(0,0,0)));
        yield return null;
    }

}
