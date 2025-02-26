using System.Collections;
using UnityEngine;

public class MortarController : TowerController
{
    [SerializeField] private float ProjectileRadius = 5f;
    protected override IEnumerator FireProjectile(Vector3 direction)
    {
        GameObject Projectile = GetPooledObject();
        Projectile.SetActive(true);
        Projectile.transform.position = AimPoint.transform.position;
        Projectile.transform.rotation = new Quaternion(0, 0, 0, 0);
        Projectile.GetComponent<MortarProjectile>().SetPositionLerp(AimPoint.transform.position, TargetPos);
        
        AudioManager.Instance.PlaySoundEffect("Mortar_SFX");
        
        
        yield return null;
    }

    protected override IEnumerator LOSCheck()
    {
        if (_EnemyList.Count <= 0) { yield return null; }
        float TargetSpd = Target.GetComponent<I_GetType>().GetSpeed();
        Vector3 PredictedPos = Target.transform.position + (Target.transform.forward * TargetSpd);
        TargetPos = Vector3.Slerp(Target.transform.position, PredictedPos, 1f);
        Head.transform.LookAt(TargetPos);

        StartCoroutine(FireProjectile(new Vector3(0,0,0)));
        yield return null;
    }

    protected override void SetStat(GameObject projectile)
    {
        base.SetStat(projectile);
        projectile.GetComponent<I_TowerProjectile>().SetRadius(ProjectileRadius); 
    }

    public override string GetCurrentStats()
    {
        string basecall = base.GetCurrentStats();
        basecall += "Blast AOE: " + ProjectileRadius + "m \n";

        return basecall;
    }

    public override string ScanUpgrades(UpgradeData type, bool returnstring)
    {
        string basecall = base.ScanUpgrades(type, returnstring);

        switch (type.upgradeType)
        {
            case UpgradeType.AOE:
                if (!returnstring)
                    ProjectileRadius += type.value;
                basecall += "Blast AOE: " + ProjectileRadius + " -> " + (ProjectileRadius + type.value) + "m \n";
                break;
        }
        if (returnstring)
        {
            return basecall;
        }
        return string.Empty;
    }

}
