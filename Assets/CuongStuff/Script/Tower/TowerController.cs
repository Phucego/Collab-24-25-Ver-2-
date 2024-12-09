using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TowerController : MonoBehaviour
{
    // Start is called before the first frame update
    [Header("Tower Set Up")]
    public TowerDataSO TowerData;
    public GameObject MainPoint, Head, AimPoint, HeadParent, BodyParent;
    public GameObject[] PrefabProjectile;
    public UnityEvent<UpgradeType, float> CallChangeStat;
    [HideInInspector] public bool TowerPlaced = false;

    [Header("Tower Stats")]
    [SerializeField] protected int Level = 0;
    [SerializeField] protected TowerTypeEnum TowerType;
    [SerializeField] protected List<TargetTypeEnum> TargetType;
    [SerializeField] protected float Damage;
    [SerializeField] protected float Health;
    [SerializeField] protected float Radius;
    [SerializeField] protected float FireRate;
    [SerializeField] protected float ProjectileSpeed;
    [SerializeField] [Range(0, 100)] protected float CritChance;
    [SerializeField] protected float CritAmp;

    //[Header("Tower Test")]
    protected List<GameObject> _EnemyList = new List<GameObject>();
    protected GameObject[] _HeadModel = new GameObject[] { };
    protected GameObject[] _BodyModel = new GameObject[] { };
    protected List<GameObject> _ProjectileList = new List<GameObject>();
    //protected List<GameObject> ProjectileList = new List<GameObject>();
    protected SphereCollider RadiusDetector;
    protected GameObject Target, CurrentHead, CurrentBody;
    protected Vector3 TargetPos = new Vector3(0,0,0);
    protected LayerMask layerMask;
    protected float TimeBeforeFire = 0f;

    private void Awake()
    {
        MainPoint = GameObject.Find("MainPoint");
        layerMask = LayerMask.GetMask("Enemy");
        RadiusDetector = GetComponent<SphereCollider>();
        _HeadModel = new GameObject[TowerData.listUpgrades.Count + 1];
        _BodyModel = new GameObject[TowerData.listUpgrades.Count + 1];
    }

    private void OnEnable()
    {
        //TowerPlaced = true;
        DeepCopyData();
        RadiusDetector.radius = Radius;
        CallChangeStat.Invoke(UpgradeType.Radius, Radius);
        GetAllModels(HeadParent, 1);
        GetAllModels(BodyParent, 2);
        CurrentHead = _HeadModel[0];
        CurrentBody = _BodyModel[0];

    }

    protected virtual void GetAllModels(GameObject parent, int index)
    {
        for (int i = 0; i < parent.transform.childCount; i++)
        {
            GameObject model = parent.transform.GetChild(i).gameObject;
            switch (index)
            {
                case 1: //Get Head
                    _HeadModel[i] = model;
                    break;
                case 2: //Get Model
                    _BodyModel[i] = model;
                    break;
            }
        }
    }

    // Get data from the scriptable object and get a copy of it
    protected virtual void DeepCopyData()
    {
        TowerDataSO _DeepCopyTowerData = Instantiate(TowerData);
        TowerType = _DeepCopyTowerData.TowerType;
        TargetType = _DeepCopyTowerData.TargetType;
        Damage = _DeepCopyTowerData.Damage;
        Health = _DeepCopyTowerData.Health;
        Radius = _DeepCopyTowerData.Radius;
        FireRate = _DeepCopyTowerData.FireRate;
        ProjectileSpeed = _DeepCopyTowerData.ProjectileSpeed;
        CritChance = _DeepCopyTowerData.CritChance;
        CritAmp = _DeepCopyTowerData.CritAmplifier;
        
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        if (!TowerPlaced)
            return;

        if (Target != null && TimeBeforeFire <= 0)
        {
            StartCoroutine(LOSCheck());
            TimeBeforeFire = FireRate;
        }
        else if (TimeBeforeFire >= 0)
        {
            TimeBeforeFire -= Time.deltaTime;
        }
        FindNearestEnemy();

    }

    // Check if enemy in within the sight of the aimpoint
    protected virtual IEnumerator LOSCheck()
    {
        TargetPos = Target.transform.position;
        Head.transform.LookAt(TargetPos);
        RaycastHit hit;
        // Does the ray intersect any objects excluding the player layer
        if (Physics.Raycast(AimPoint.transform.position, AimPoint.gameObject.transform.TransformDirection(Vector3.forward), out hit, Mathf.Infinity, layerMask))
        {
            Debug.DrawRay(AimPoint.transform.position, AimPoint.transform.TransformDirection(Vector3.forward) * hit.distance, Color.yellow);
            StartCoroutine(FireProjectile(hit.transform.position - AimPoint.transform.position));
        }
        yield return null;
    }
    protected virtual IEnumerator FireProjectile(Vector3 direction)
    {
        GameObject Projectile = GetPooledObject();
        Projectile.transform.position = AimPoint.transform.position;
        Projectile.transform.rotation = AimPoint.transform.rotation;
        SetStat(Projectile);
        Projectile.SetActive(true);
        //Projectile.GetComponent<ProjectileController>().SetDirection(direction.normalized);

        yield return null;
    }

    protected virtual void SetStat(GameObject projectile)
    {
        projectile.GetComponent<I_TowerProjectile>().SetDamage(Damage);
    }

    // Pooling objects
    protected virtual GameObject GetPooledObject()
    {
        for (int i = 0; i < _ProjectileList.Count; i++) {
            if (!_ProjectileList[i].activeInHierarchy)
            {
                return _ProjectileList[i];
            }
        }

        // Create more projectiles if no more objects are pooled
        GameObject NewProjectile = Instantiate(PrefabProjectile[0], AimPoint.transform.position, Quaternion.identity, GameObject.Find("_Projectiles").transform);
        _ProjectileList.Add(NewProjectile);
        return NewProjectile;
    }

    // This function determine how the turret will target enemy, currently it target the first enemy in the line / nearest to the MAIN center
    // Will plan on expanding this if the first semester goes well
    public virtual void FindNearestEnemy()
    {
        foreach (var enemy in _EnemyList)
        {  
            if (enemy != null)
            {
                int point = 0;
                TargetTypeEnum[] enemyType = enemy.GetComponent<I_GetType>().GetTargetType();
                // Check if they have matching target type
                foreach (TargetTypeEnum enemytype in enemyType)
                    if (TargetType.Contains(enemytype))
                        point += 1;
                // Reset the enemy target if they dont match target type
                if (point == 0)
                {
                    if (enemy == Target)
                    {
                        TargetPos = new Vector3(0, 0, 0);
                        Target = null;
                    }
                    continue;
                }

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
                

        }
    }

    // Upgrade tower call success
    public void UpgradeTower()
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

        bool HasModelHead = CheckModelValid(_HeadModel, Level);
        bool HasModelBody = CheckModelValid(_BodyModel, Level); 

        if (HasModelHead)
        {
            CurrentHead.SetActive(false);
            CurrentHead = _HeadModel[Level];
            CurrentHead.SetActive(true);
        }

        if (HasModelBody)
        {
            CurrentBody.SetActive(false);
            CurrentBody = _BodyModel[Level];
            CurrentBody.SetActive(true);
        }

    }

    public void ConfigTargetType(TargetTypeEnum type, bool action)
    {
        if (action)
            TargetType.Add(type);
        else 
            TargetType.Remove(type);

    }

    // Check if model within the tower exist in the array
    protected bool CheckModelValid(GameObject[] model, int index)
    {
        if (model[index] != null)
            return true;
        return false;
    }

    private void OnTriggerEnter(Collider target)
    {
        if (target.gameObject.CompareTag("Enemy") && !_EnemyList.Contains(target.gameObject))
        {
            _EnemyList.Add(target.gameObject);
        }
    }

    private void OnTriggerExit(Collider target)
    {
        _EnemyList.RemoveAll(gameobject => gameobject == null);
        if (target.gameObject.CompareTag("Enemy"))
        {
            _EnemyList.Remove(target.gameObject);
            Target = null;
            TargetPos = new Vector3(0,0,0);
        }
    }
}
