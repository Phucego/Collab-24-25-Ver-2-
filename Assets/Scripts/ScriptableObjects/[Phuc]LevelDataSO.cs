using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "LevelData", menuName = "Data/LevelData")]
public class LevelDataSO : ScriptableObject
{
    public TowerDataSO[] towerData;

    public GameObject coinCounterParent;
    public GameObject waveProgressParent;
    public GameObject PauseandWaveParent;
    
    public Text coinCounterText;
    public Text waveProgressText;

    public Button startWaveButton;
    public Button pauseButton;

}