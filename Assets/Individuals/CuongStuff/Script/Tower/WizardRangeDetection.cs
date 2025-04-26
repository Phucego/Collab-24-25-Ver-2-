using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WizardRangeDetection : TowerRangeDetection
{
    private List<GameObject> _TowerList = new List<GameObject>();
    private void OnTriggerEnter(Collider target)
    {
        if (target.gameObject.CompareTag("Enemy") && !towerController._EnemyList.Contains(target.gameObject))
        {
            towerController._EnemyList.Add(target.gameObject);
            /*if (towerController._EnemyList.Count == 1) 
                towerController.TargetPos = target.gameObject.transform.position;*/
        }
        else if (target.gameObject.CompareTag("Tower") && !_TowerList.Contains(target.gameObject))
        {
            _TowerList.Add(target.gameObject);
            target.gameObject.GetComponent<TowerController>().ConfigTargetType(eType.Invisible, true);
        }

    }

    private void OnTriggerExit(Collider target)
    {
        _TowerList.RemoveAll(gameobject => gameobject == null);
        towerController._EnemyList.RemoveAll(gameobject => gameobject == null);
        towerController._EnemyList.RemoveAll(gameobject => !gameobject.activeSelf);
        if (target.gameObject.CompareTag("Enemy"))
        {
            towerController._EnemyList.Remove(target.gameObject);
            if (towerController._EnemyList.Count <= 0)
            {
                towerController.Target = null;
                towerController.TargetPos = new Vector3(0, 0, 0);
            }
            else if (target.gameObject == towerController.Target)
            {
                towerController.Target = null;
                towerController.TargetPos = new Vector3(0, 0, 0);
                towerController.FindNearestEnemy();
            }

        }
        else if (target.gameObject.CompareTag("Tower") && !target.isTrigger)
        {
            _TowerList.Remove(target.gameObject);
            target.gameObject.GetComponent<TowerController>().ConfigTargetType(eType.Invisible, false);
        }
    }

    private void OnDestroy()
    {
        foreach (GameObject obj in _TowerList)
        {
            if (obj != null)
                obj.GetComponent<TowerController>().ConfigTargetType(eType.Invisible, false);

        }
    }
}
