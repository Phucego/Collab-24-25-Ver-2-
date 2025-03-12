using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStatesManager : MonoBehaviour
{
    public static GameStatesManager Instance;
    GameStates currentState;

    private void Awake()
    {
        Instance = this;

        if (Instance == null)
        {
            Instance = this;
        }
        else
        { 
            Destroy(gameObject);
        }
        
    }
    private void Start()
    {
        
    }
    public void ChangeState(GameStates newState)
    {
        currentState = newState;
        StopAllCoroutines();

        switch (currentState)
        {
            case GameStates.WaveSetup:
                BuildingManager.Instance.enabled = true;
                break;
            case GameStates.WaveActive:
                //TODO: LOGIC
                break; 
            case GameStates.WaveCompleted:
                //TODO: LOGIC
                break; 
            case GameStates.Pause:
                //TODO: LOGIC
                break; 
            case GameStates.GameOver:
                //TODO: LOGIC
                break;
        }
    }
}

