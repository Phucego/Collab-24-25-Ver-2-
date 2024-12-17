using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using UnityEngine.Events;
using static UnityEditor.Progress;

public class TowerController : MonoBehaviour
{
    // Start is called before the first frame update
    [Header("Tower Set Up")]
    public TowerDataSO TowerData;
    public GameObject[] PrefabProjectile;
    public UnityEvent<UpgradeType, float> CallChangeStat;
    public bool TowerPlaced = false;

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
    protected GameObject MainPoint, Head, AimPoint, HeadParent, BodyParent, CurrentHead, CurrentBody;
    protected GameObject[] _HeadModel = new GameObject[] { };
    protected GameObject[] _BodyModel = new GameObject[] { };
    protected List<GameObject> _ProjectileList = new List<GameObject>();
    //protected List<GameObject> ProjectileList = new List<GameObject>();

    
    protected CapsuleCollider RadiusDetector;
    protected TowerDataSO _DeepCopyTowerData; 
    protected LayerMask layerMask;
    protected float TimeBeforeFire = 0f;

    [HideInInspector] public List<GameObject> _EnemyList = new List<GameObject>();
    [HideInInspector] public GameObject Target;
    [HideInInspector] public Vector3 TargetPos = new Vector3(0,0,0);

    
    private void Awake()
    {   
        // Set up references
        MainPoint = GameObject.Find("MainPoint");
        Head = gameObject.transform.GetChild(0).gameObject;
        AimPoint = Head.transform.GetChild(0).gameObject;
        HeadParent = Head.transform.GetChild(1).gameObject;
        BodyParent = gameObject.transform.GetChild(1).gameObject;
        RadiusDetector = gameObject.transform.GetChild(2).GetComponent<CapsuleCollider>();

        layerMask = LayerMask.GetMask("Enemy"); 
        _HeadModel = new GameObject[TowerData.listUpgrades.Count + 1];
        _BodyModel = new GameObject[TowerData.listUpgrades.Count + 1];
        
        
    }

    private void OnEnable()
    {
        //TowerPlaced = true;
        DeepCopyData();
        RadiusDetector.radius = Radius;
        RadiusDetector.height = Radius*3;
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
        _DeepCopyTowerData = Instantiate(TowerData);
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
        if (!TowerPlaced || _EnemyList.Count <= 0)
            return;

        if (Target != null && TimeBeforeFire <= 0)
        {
            StartCoroutine(LOSCheck());
            TimeBeforeFire = FireRate;
        }
        else if (TimeBeforeFire > 0)
        {
            TimeBeforeFire -= Time.deltaTime;
        }
        FindNearestEnemy();
    }

    // Check if enemy is within the sight of the aimpoint
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

    // Fire projectiles
    protected virtual IEnumerator FireProjectile(Vector3 direction)
    {
        GameObject Projectile = GetPooledObject();
        Projectile.transform.position = AimPoint.transform.position;
        Projectile.transform.rotation = AimPoint.transform.rotation;
        Projectile.SetActive(true);
        
        AudioManager.Instance.PlaySoundEffect("Cannon_SFX");
        //Projectile.GetComponent<ProjectileController>().SetDirection(direction.normalized);

        yield return null;
    }

    // Update stats for the projectiles
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

        // Create more projectiles if no more pooled objects are available
        GameObject NewProjectile = Instantiate(PrefabProjectile[0], AimPoint.transform.position, Quaternion.identity, GameObject.Find("_Projectiles").transform);
        _ProjectileList.Add(NewProjectile);
        SetStat(NewProjectile);
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
                {
                    if (TargetType.Contains(enemytype))
                        point += 1;
                    if (enemytype == TargetTypeEnum.Invisible) // If target has invisible and the tower has hidden detection
                        point -= 1;
                }
                    
                // Reset the enemy target if they dont match target type
                if (point <= 0)
                {
                    if (enemy == Target)
                    {
                        TargetPos = new Vector3(0,0,0);
                        Target = null;
                    }
                    continue;
                }

                /*float distance = Vector3.Distance(transform.position, enemy.transform.position);
                float lastdistance = Vector3.Distance(transform.position, TargetPos);*/
                if (Target == null)
                {
                    Target = enemy;
                    TargetPos = Target.transform.position;
                }
                //Get distance from the main point to the tower
                float mainpointdistance = Vector3.Distance(MainPoint.transform.position, enemy.transform.position);
                float mainpointlastdistance = Vector3.Distance(MainPoint.transform.position, TargetPos);

                if (mainpointdistance < mainpointlastdistance) //Change target if it is the closest one
                {
                    Target = enemy;
                    TargetPos = Target.transform.position;
                }
            }
        }
    }

    public virtual void UpgradeTower()
    {
        if (Level >= TowerData.listUpgrades.Count)
            return;

        // Determine the cost for this upgrade
        int upgradeCost = TowerData.listUpgrades[Level].Cost; // Assuming each UpgradeDataSO has a "cost" property

        // Check if the player has enough currency
        if (!CurrencyManager.Instance.HasEnoughCurrency(upgradeCost))
        {
            AudioManager.Instance.PlaySoundEffect("Insufficient_SFX");
            Debug.Log("Not enough currency to upgrade the tower.");
            return;
        }

        // Deduct the cost from the player's currency
        CurrencyManager.Instance.DeductCurrency(upgradeCost);

        // Proceed with the upgrade
        Level += 1;
        UpgradeDataSO Data = TowerData.listUpgrades[Level - 1];
        AudioManager.Instance.PlaySoundEffect("Upgrade_SFX");

        foreach (var item in Data.upgradeDatas)
        {
            ScanUpgrades(item, false);
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

        foreach (var projectile in _ProjectileList)
        {
            SetStat(projectile);
        }
    }


    // ========== GET INFO ========== \\
    // Return values as string to the tower canvas
    public virtual string GetCurrentStats()
    {
        string stats = "";
        stats += "Damage: " + Damage + "\n";
        stats += "Range: " + Radius + "m \n";
        stats += "Fire Rate: " + FireRate + "s\n";

        stats += "Crit Chance: " + CritChance + "% \n";
        stats += "Crit Amp: " + CritAmp + "x \n";

        return stats;
    }

    public virtual string GetName()
    {
        return TowerData.name;
    }

    public virtual string GetLevelString()
    {
        string level = "Level " + Level + " -> Level " + (Level + 1);
        if (Level + 1 == TowerData.listUpgrades.Count)
            level = "Level " + Level + " -> MAXED ";
        else if (Level >= TowerData.listUpgrades.Count)
            level = "MAXED";

        return level;
    }
    public virtual int GetLevelInt()
    {
        return Level;
    }

    public virtual string GetUpgradeStats()
    {
        string upgradestats = "";
        if (Level >= TowerData.listUpgrades.Count)
            return "YOU HAVE MAXED THIS TOWER";

        UpgradeDataSO Data = TowerData.listUpgrades[Level];
        foreach (var item in Data.upgradeDatas)
        {
            upgradestats += ScanUpgrades(item, true);
        }
        return upgradestats;
    }
    // ========== END ========== \\

    // Get data from the upgrade scriptable object
    public virtual string ScanUpgrades(UpgradeData type, bool returnstring)
    {
        string stat = "";
        switch (type.upgradeType)
        {
            case UpgradeType.Health:
                if (!returnstring)
                    Health += type.value;
                break;
            case UpgradeType.Damage:
                if (!returnstring)
                    Damage += type.value;
                stat = "Damage: " + Damage + " -> " + (Damage + type.value) + "\n";
                break;
            case UpgradeType.FireRate:
                if (!returnstring)
                    FireRate += type.value;
                stat = "Fire Rate: " + FireRate + " -> " + (FireRate + type.value) + "s\n";
                break;
            case UpgradeType.Radius:
                if (!returnstring)
                {
                    Radius += type.value;
                    CallChangeStat.Invoke(UpgradeType.Radius, Radius);
                    RadiusDetector.radius = Radius;
                    RadiusDetector.height = Radius * 3;
                }
                stat = "Range: " + Radius + " -> " + (Radius + type.value) + "m \n";
                break;
            case UpgradeType.AOE:
                // No AOE
                break;
        }
        if (returnstring)
        {
            return stat;
        }
        return string.Empty;
    }


    // Change target types
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

}
