using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TowerRangeDetection : MonoBehaviour
{
    protected TowerController towerController;
    private void Awake()
    {
        towerController = GetComponentInParent<TowerController>();
    }

    private void OnTriggerEnter(Collider target)
    {
        if (target.gameObject.CompareTag("Enemy") && !towerController._EnemyList.Contains(target.gameObject))
        {
            towerController._EnemyList.Add(target.gameObject);
            /*if (towerController._EnemyList.Count == 1) 
                towerController.TargetPos = target.gameObject.transform.position;*/
        }
    }

    private void OnTriggerExit(Collider target)
    {
        towerController._EnemyList.RemoveAll(gameobject =>  gameobject == null);
        towerController._EnemyList.RemoveAll(gameobject => !gameobject.activeSelf);
        if (target.gameObject.CompareTag("Enemy"))
        {
            towerController._EnemyList.Remove(target.gameObject);
            if (towerController._EnemyList.Count <= 0)
            {
                towerController.Target = null;
                towerController.TargetPos = new Vector3(0,0,0);
            }
            
        }
    }
}
