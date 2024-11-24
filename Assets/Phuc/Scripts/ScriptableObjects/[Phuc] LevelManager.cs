using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class LevelManager : MonoBehaviour
{
    private int coinCounter = 0;
    public static LevelManager instance;
    public LevelDataSO m_LevelDataSO;
    public LevelDataSO LevelDataSO => m_LevelDataSO;
    
    public GameObject coinCounterParent; 
    public GameObject waveProgressParent;
    public GameObject PauseandWaveParent;
    public GameObject crosshair;
    
    public Button startWaveButton;
    public Button pauseButton;

  
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
        
        
        go.GetComponent<EnemyDrops>().InitEnemy((coin) => {
            this.coinCounter += coin;
            Debug.Log(this.coinCounter);
        });
        StartCoroutine(AddScoreAfterEnemyDies());
    }
}
