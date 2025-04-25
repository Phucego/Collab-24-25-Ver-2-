using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WizardController : TowerController
{
    private List<HitscanController> hitscanControllers = new List<HitscanController>();
    private float DamageInterval = 0.7f;
    private float intervalCooldown = 0f;
    private bool Firing = false;
    private bool UnlockBeam = false;

    protected override void Update()
    {
        if (!TowerPlaced || !UnlockBeam || _EnemyList.Count <= 0)
            return;

        if (Target != null && TimeBeforeFire <= 0 && Target.activeSelf)
        {
            float distance = Vector3.Distance(transform.position, Target.transform.position);
            if (distance > Radius * 3f) { return; }

            StartCoroutine(FireProjectile(new Vector3(0,0,0).normalized));
            StartCoroutine(Cooldown());
            TimeBeforeFire = FireRate;
            intervalCooldown = 0f;
            Firing = true;
        }
        else if (TimeBeforeFire > 0)
        {
            TimeBeforeFire -= Time.deltaTime;
        }
        FindNearestEnemy();

        if (!Firing)
            return;

        // Some fucking bullshit
        if (intervalCooldown <= 0f)
        {
            intervalCooldown = DamageInterval;
            for (int i = 0; i < _ProjectileList.Count; i++)
            {
                if (_ProjectileList[i].activeInHierarchy)
                {
                    float distance = Vector3.Distance(transform.position, Target.transform.position);
                    if (distance > Radius * 3f) { FindNearestEnemy(); };
                    if (Target != null)
                        hitscanControllers[i].SetTarget(Target);
                }
            }
            
        }
        else if (intervalCooldown > 0f)
        {
            intervalCooldown -= Time.deltaTime;
        }  
    }

    protected override IEnumerator FireProjectile(Vector3 direction)
    {
        GameObject Projectile = GetPooledObjectWizard();
        Projectile.transform.position = AimPoint.transform.position;
        Projectile.transform.rotation = AimPoint.transform.rotation;
        Projectile.GetComponent<HitscanController>().SetTarget(Target);

        Projectile.SetActive(true);  
        
        AudioManager.Instance.PlaySoundEffect("Zap_SFX");
        yield return new WaitForSeconds(0f);
    }

    // Pooling objects
    protected virtual GameObject GetPooledObject()
    {
        for (int i = 0; i < _ProjectileList.Count; i++)
        {
            if (!_ProjectileList[i].activeInHierarchy)
            {
                return _ProjectileList[i];
            }
        }

        // Create more projectiles if no more pooled objects are available
        GameObject NewProjectile = Instantiate(PrefabProjectile[0], AimPoint.transform.position, Quaternion.identity, GameObject.Find("_Projectiles").transform);
        _ProjectileList.Add(NewProjectile);
        SetStat(NewProjectile);
        return NewProjectile;
    }

    // Pooling objects
    protected virtual GameObject GetPooledObjectWizard()
    {
        GameObject projectile = GetPooledObject();
        for (int i = 0; i < _ProjectileList.Count; i++)
        {
            if (!_ProjectileList[i].activeInHierarchy)
            {
                return _ProjectileList[i];
            }
        }

        hitscanControllers.Add(projectile.GetComponent<HitscanController>());
        return projectile;
    }

    private IEnumerator Cooldown()
    {
        yield return new WaitForSeconds(3f);
        Firing = false;
    }

    public override void UpgradeTower()
    {
        base.UpgradeTower();
        if (Level == 1)
            UnlockBeam = true;
    }

    public override string GetCurrentStats()
    {
        string basecall = base.GetCurrentStats();
        basecall += "Damage Interval: " + DamageInterval + "\n";
        basecall += "Hidden Detection Buff: Provide all towers nearby hidden detection \n";

        return basecall;
    }

    public override string GetUpgradeStats()
    {
        string basecall = base.GetUpgradeStats();
        if (Level == 0)
        {
            basecall += "Unlock: Lightning Beam \n";
            basecall += "Lightning Beam: Fire an electric beam that rapidly deals damage to a single target \n";
        }
        return basecall;
    }

    public override string ScanUpgrades(UpgradeData type, bool returnstring)
    {
        string basecall = base.ScanUpgrades(type, returnstring);

        switch (type.upgradeType)
        {
            case UpgradeType.Interval:
                if (!returnstring)
                    DamageInterval += type.value;
                basecall += "Damage Interval: " + DamageInterval + " -> " + (DamageInterval + type.value) + "s \n";
                break;
        }     

        if (returnstring)
        {
            return basecall;
        }
        return string.Empty;
    }
}
