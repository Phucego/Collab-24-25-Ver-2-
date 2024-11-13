using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class TowerController : MonoBehaviour
{
    // Start is called before the first frame update
    [Header("Tower Set Up")]
    [SerializeField] protected TowerDataSO TowerData;
    [SerializeField] protected GameObject MainPoint;
    [SerializeField] protected GameObject Head;
    [SerializeField] protected GameObject AimPoint;
    [SerializeField] protected GameObject[] PrefabProjectile;

    [Header("Tower Stats")]
    [SerializeField] protected int Level = 0;
    [SerializeField] protected TowerType TowerType;
    [SerializeField] protected List<TargetType> TargetType;
    [SerializeField] protected float Damage;
    [SerializeField] protected float Health;
    [SerializeField] protected float Radius;
    [SerializeField] protected float FireRate;
    [SerializeField] protected float ProjectileSpeed;
    [SerializeField] [Range(0, 100)] protected float CritChance;
    [SerializeField] protected float CritAmp;

    [Header("Tower Test")]
    [SerializeField] protected List<GameObject> EnemyList = new List<GameObject>();
    protected List<GameObject> ProjectileList = new List<GameObject>();
    //protected List<GameObject> ProjectileList = new List<GameObject>();
    protected TowerInteract InteractScript;
    protected SphereCollider RadiusDetector;
    protected GameObject Target;
    protected Vector3 TargetPos = new Vector3(0,0,0);
    protected LayerMask layerMask;
    protected float TimeBeforeFire = 0f;

    private void Awake()
    {
        MainPoint = GameObject.Find("MainPoint");
        layerMask = LayerMask.GetMask("Enemy");
        InteractScript = GetComponent<TowerInteract>();
        RadiusDetector = GetComponent<SphereCollider>();
    }

    private void OnEnable()
    {
        DeepCopyData();
        RadiusDetector.radius = Radius;
        InteractScript.ChangeStat(UpgradeType.Radius, Radius);
    }

    protected virtual void DeepCopyData()
    {
        TowerType = TowerData.TowerType;
        TargetType = TowerData.TargetType;
        Damage = TowerData.Damage;
        Health = TowerData.Health;
        Radius = TowerData.Radius;
        FireRate = TowerData.FireRate;
        ProjectileSpeed = TowerData.ProjectileSpeed;
        CritChance = TowerData.CritChance;
        CritAmp = TowerData.CritAmplifier;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (Target != null && TimeBeforeFire <= 0)
        {
            LOSCheck();
            TimeBeforeFire = FireRate;
        }
        else if (TimeBeforeFire >= 0)
        {
            TimeBeforeFire -= Time.deltaTime;
        }
        FindNearestEnemy();
    }

    protected virtual void LOSCheck()
    {
        TargetPos = Target.transform.position;
        Head.transform.LookAt(TargetPos);
        RaycastHit hit;
        // Does the ray intersect any objects excluding the player layer
        if (Physics.Raycast(AimPoint.transform.position, AimPoint.transform.TransformDirection(Vector3.forward), out hit, Mathf.Infinity, layerMask))
        {
            Debug.DrawRay(AimPoint.transform.position, AimPoint.transform.TransformDirection(Vector3.forward) * hit.distance, Color.yellow);
            StartCoroutine(FireProjectile(hit.transform.position - AimPoint.transform.position));
        }
        else
        {
            Debug.DrawRay(AimPoint.transform.position, AimPoint.transform.TransformDirection(Vector3.forward) * 100, Color.white);
        }
    }
    protected virtual IEnumerator FireProjectile(Vector3 direction)
    {
        GameObject Projectile = GetPooledObject();
        Projectile.transform.position = AimPoint.transform.position;
        Projectile.transform.rotation = AimPoint.transform.rotation;
        //Projectile.GetComponent<ProjectileController>().SetDirection(direction.normalized);
        Projectile.SetActive(true);
         
        yield return new WaitForSeconds(0f);
    }

    protected virtual GameObject GetPooledObject()
    {
        for (int i = 0; i < ProjectileList.Count; i++) {
            if (!ProjectileList[i].activeInHierarchy)
            {
                return ProjectileList[i];
            }
        }

        // Create more projectiles if no more objects are pooled
        GameObject NewProjectile = Instantiate(PrefabProjectile[0], transform.position, Quaternion.identity, GameObject.Find("_Projectiles").transform);
        ProjectileList.Add(NewProjectile);
        return NewProjectile;
    }

    // This function determine how the turret will target enemy, currently it target the first enemy in the line / nearest to the MAIN center
    // Will plan on expanding this if the first semester goes well
    public void FindNearestEnemy()
    {
        foreach (var enemy in EnemyList)
        {
            if (enemy != null)
            {
                float distance = Vector3.Distance(transform.position, enemy.transform.position);
                float lastdistance = Vector3.Distance(transform.position, TargetPos);

                float mainpointdistance = Vector3.Distance(MainPoint.transform.position, enemy.transform.position);
                float mainpointlastdistance = Vector3.Distance(MainPoint.transform.position, TargetPos);

                if (mainpointdistance < mainpointlastdistance)
                {
                    Target = enemy;
                    TargetPos = Target.transform.position;
                }
            }
            else
            {
                EnemyList.RemoveAll(gameobject => gameobject == null);
                TargetPos = new Vector3(0,0,0);
                Target = null;
            }
                

        }
    }

    public void UpgradeStat()
    {
        if (Level >= TowerData.listUpgrades.Count)
            return;

        Level += 1;
        UpgradeDataSO Data = TowerData.listUpgrades[Level-1];
        foreach (var item in Data.upgradeDatas)
        {
            switch (item.upgradeType)
            {
                case UpgradeType.Health:
                    Health += item.value;
                    break;
                case UpgradeType.Damage:
                    Damage += item.value;
                    break;
                case UpgradeType.FireRate:
                    FireRate += item.value;
                    break;
                case UpgradeType.Radius:
                    Radius += item.value;
                    break;
                case UpgradeType.AOE:
                    // No AOE
                    break;
            }
        }
     
    }

    private void OnTriggerEnter(Collider target)
    {
        if (target.gameObject.CompareTag("Enemy") && !EnemyList.Contains(target.gameObject))
        {
            EnemyList.Add(target.gameObject);
        }
    }

    private void OnTriggerExit(Collider target)
    {
        EnemyList.RemoveAll(gameobject => gameobject == null);
        if (target.gameObject.CompareTag("Enemy"))
        {
            EnemyList.Remove(target.gameObject);
            Target = null;
            TargetPos = new Vector3(0,0,0);
        }
    }
}
