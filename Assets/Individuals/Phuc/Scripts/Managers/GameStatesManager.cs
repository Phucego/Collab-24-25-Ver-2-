using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStatesManager : MonoBehaviour
{
    public static GameStatesManager Instance;
    private GameStates currentState;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        { 
            Destroy(gameObject);
        }
    }

    // Get the current game state
    public GameStates GetCurrentState()
    {
        return currentState;
    }

    // Use this to change the state of the game in other scripts as well
    public void ChangeState(GameStates newState)
    {
        currentState = newState;
        StopAllCoroutines();

        switch (currentState)
        {
            case GameStates.WaveSetup:
                BuildingManager.Instance.enabled = true;
                FreeFlyCamera.instance.enabled = true;
                break;
            
            case GameStates.WaveActive:
                BuildingManager.Instance.enabled = false;
                FreeFlyCamera.instance.enabled = true;
                break; 
            
            case GameStates.WaveCompleted:
                // TODO: Add logic
                break; 
            
            case GameStates.Pause:
                BuildingManager.Instance.enabled = false;
                FreeFlyCamera.instance.enabled = false;
                break; 
            
            case GameStates.GameOver:
                // TODO: Add logic
                break;
        }
    }
}