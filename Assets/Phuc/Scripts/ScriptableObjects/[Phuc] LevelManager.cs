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
    
  

  
    [SerializeField] private GameObject m_TestEnemy;

    private void Awake()
    {
        instance = this;
    }

    
    
    
    

}
