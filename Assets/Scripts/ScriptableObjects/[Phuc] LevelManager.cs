using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    
    public static LevelManager instance;
    public LevelDataSO m_LevelDataSO;
    public LevelDataSO LevelDataSO => m_LevelDataSO;
    private int score = 0;
    [SerializeField] private GameObject m_TestEnemy;

    private void Awake()
    {
        instance = this;
    }

    
    //TODO: Call back the score when the enemy dies
    private IEnumerator AddScoreAfterEnemyDies()
    {
        yield return new WaitForSeconds(4);
        GameObject go = Instantiate(m_TestEnemy, Vector3.zero, Quaternion.identity);
        
        
        go.GetComponent<EnemyDrops>().InitEnemy((a) => {
            this.score += a;
            Debug.Log(this.score);
        });
        StartCoroutine(AddScoreAfterEnemyDies());
    }
}
