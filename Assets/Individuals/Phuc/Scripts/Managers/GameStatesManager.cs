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
            DontDestroyOnLoad(gameObject);
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

    // Use this to change the state of the game in other scripts
    public void ChangeState(GameStates newState)
    {
        currentState = newState;
        StopAllCoroutines();

        switch (currentState)
        {
            case GameStates.WaveSetup:
                if (BuildingManager.Instance != null)
                    BuildingManager.Instance.enabled = true;
                if (FreeFlyCamera.instance != null)
                    FreeFlyCamera.instance.enabled = true;
                break;

            case GameStates.WaveActive:
                if (BuildingManager.Instance != null)
                    BuildingManager.Instance.enabled = false;
                if (FreeFlyCamera.instance != null)
                    FreeFlyCamera.instance.enabled = true;
                break;

            case GameStates.WaveCountdown:
                if (BuildingManager.Instance != null)
                    BuildingManager.Instance.enabled = true;
                if (FreeFlyCamera.instance != null)
                    FreeFlyCamera.instance.enabled = true;
                break;

            case GameStates.WaveCompleted:
                if (BuildingManager.Instance != null)
                    BuildingManager.Instance.enabled = true;
                if (FreeFlyCamera.instance != null)
                    FreeFlyCamera.instance.enabled = true;
                break;

            case GameStates.Pause:
                if (BuildingManager.Instance != null)
                    BuildingManager.Instance.enabled = false;
                if (FreeFlyCamera.instance != null)
                    FreeFlyCamera.instance.enabled = false;
                break;

            case GameStates.GameOver:
                if (BuildingManager.Instance != null)
                    BuildingManager.Instance.enabled = false;
                if (FreeFlyCamera.instance != null)
                    FreeFlyCamera.instance.enabled = false;
                break;
        }
    }
}