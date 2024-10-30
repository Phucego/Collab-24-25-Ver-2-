using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager instance;
    [SerializeField] private LevelDataSO m_LevelDataSO;
    public LevelDataSO LevelDataSO => m_LevelDataSO;
    [SerializeField] private GameObject m_TestEnemy;

    private void Awake()
    {
        instance = this;
    }

    private int score = 0;

    private void Start()
    {
        StartCoroutine(SpawnEnemyRoutine());
    }

    private IEnumerator SpawnEnemyRoutine()
    {
        yield return new WaitForSeconds(4);
        GameObject go = Instantiate(m_TestEnemy, Vector3.zero, Quaternion.identity);
        go.GetComponent<TestEnemy>().InitEnemy((a) => {
            this.score += a;
            Debug.Log(this.score);
        });
        StartCoroutine(SpawnEnemyRoutine());
    }
}
