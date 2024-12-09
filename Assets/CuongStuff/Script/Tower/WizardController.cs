using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WizardController : TowerController
{
    [SerializeField] private List<GameObject> _TowerList = new List<GameObject>();
    private List<HitscanController> hitscanControllers = new List<HitscanController>();
    private bool Firing = false;

    protected override void Update()
    {
        if (!TowerPlaced)
            return;

        if (Target != null && TimeBeforeFire <= 0)
        {
            StartCoroutine(FireProjectile(new Vector3(0,0,0).normalized));
            StartCoroutine(Cooldown());
            TimeBeforeFire = FireRate;
            Firing = true;
        }
        else if (TimeBeforeFire >= 0)
        {
            TimeBeforeFire -= Time.deltaTime;
        }
        FindNearestEnemy();

        if (!Firing)
            return;

        for (int i = 0; i < _ProjectileList.Count; i++)
        {
            if (_ProjectileList[i].activeInHierarchy)
            {
                hitscanControllers[i].SetTarget(Target);
            }
        }

    }

    protected override IEnumerator FireProjectile(Vector3 direction)
    {
        GameObject Projectile = GetPooledObject();
        Projectile.transform.position = AimPoint.transform.position;
        Projectile.transform.rotation = AimPoint.transform.rotation;
        SetStat(Projectile);
        Projectile.GetComponent<HitscanController>().SetTarget(Target);

        Projectile.SetActive(true);  

        yield return new WaitForSeconds(0f);
    }

    // Pooling objects
    protected override GameObject GetPooledObject()
    {
        for (int i = 0; i < _ProjectileList.Count; i++)
        {
            if (!_ProjectileList[i].activeInHierarchy)
            {
                return _ProjectileList[i];
            }
        }

        // Create more projectiles if no more objects are pooled
        GameObject NewProjectile = Instantiate(PrefabProjectile[0], AimPoint.transform.position, Quaternion.identity, GameObject.Find("_Projectiles").transform);
        hitscanControllers.Add(NewProjectile.GetComponent<HitscanController>());
        _ProjectileList.Add(NewProjectile);
        return NewProjectile;
    }

    private IEnumerator Cooldown()
    {
        yield return new WaitForSeconds(3f);
        Firing = false;
    }

    private void OnTriggerEnter(Collider target)
    {
        if (target.gameObject.CompareTag("Enemy") && !_EnemyList.Contains(target.gameObject))
        {
            _EnemyList.Add(target.gameObject);
        }
        else if (target.gameObject.CompareTag("Tower") && !_TowerList.Contains(target.gameObject))
        {
            _TowerList.Add(target.gameObject);
            target.gameObject.GetComponent<TowerController>().ConfigTargetType(TargetTypeEnum.Invisible,true);
        }
    }

    private void OnTriggerExit(Collider target)
    {
        _EnemyList.RemoveAll(gameobject => gameobject == null);
        _TowerList.RemoveAll(gameobject => gameobject == null);
        if (target.gameObject.CompareTag("Enemy"))
        {
            _EnemyList.Remove(target.gameObject);
            Target = null;
            TargetPos = new Vector3(0, 0, 0);
        }
        else if (target.gameObject.CompareTag("Tower"))
        {
            _TowerList.Remove(target.gameObject);
            target.gameObject.GetComponent<TowerController>().ConfigTargetType(TargetTypeEnum.Invisible, false);
        }
    }

    private void OnDestroy()
    {
        foreach (GameObject obj in _TowerList)
        {
            if (obj != null)
                obj.GetComponent<TowerController>().ConfigTargetType(TargetTypeEnum.Invisible, false);

        }
    }
}
