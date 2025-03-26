using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    [SerializeField] GameObject _enemyPrefab;
    private List<BaseEnemySO> _enemyTypes;

    [Header("Level Data:")]
    [SerializeField] string _selectedLevel;
    private List<string> _lvList;

    void Awake()
    {
        _enemyTypes = new List<BaseEnemySO>(Resources.LoadAll<BaseEnemySO>("EnemySO"));
        _enemyTypes.AddRange(Resources.LoadAll<BaseEnemySO>("BossSO"));
    }


    void Update()
    {
        /*
        if (Input.GetKeyDown(KeyCode.Keypad1))
            SpawnEnemy("Tut_Path1", 0);
        else if (Input.GetKeyDown(KeyCode.Keypad2))
            SpawnEnemy("Tut_Path1", 1);
        else if (Input.GetKeyDown(KeyCode.Keypad3))
            SpawnEnemy("Tut_Path1", 2);
        else if (Input.GetKeyDown(KeyCode.Keypad4))
            SpawnEnemy("Tut_Path2", 0);
        else if (Input.GetKeyDown(KeyCode.Keypad5))
            SpawnEnemy("Tut_Path2", 1);
        else if (Input.GetKeyDown(KeyCode.Keypad6))
            SpawnEnemy("Tut_Path2", 2);
        */
    }

    private void SpawnEnemy(string path, int type)
    {
        GameObject anEnemy = Instantiate(_enemyPrefab);

        EnemyBehavior enemyBehavior = anEnemy.GetComponent<EnemyBehavior>();
        enemyBehavior.SetStats();
        enemyBehavior.SetPath(path);
        Capture(anEnemy);

        //Debug.Log("An Enemy has spawned at " + anEnemy.transform.position + "!");
    }

    private void Capture(GameObject obj) //Put the newly spawned enemy into the Pool which has this script!
    {
        obj.transform.parent = this.transform;
    }
}