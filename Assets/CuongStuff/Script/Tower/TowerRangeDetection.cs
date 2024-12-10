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
        }
    }

    private void OnTriggerExit(Collider target)
    {
        towerController._EnemyList.RemoveAll(gameobject => gameobject == null);
        if (target.gameObject.CompareTag("Enemy"))
        {
            towerController._EnemyList.Remove(target.gameObject);
            towerController.Target = null;
            towerController.TargetPos = new Vector3(0, 0, 0);
        }
    }
}
