using System;
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

    //USE THIS TO CHANGE STATE OF THE GAME IN OTHER SCRIPTS AS WELL..
    public void ChangeState(GameStates newState)
    {
        currentState = newState;
        StopAllCoroutines();

        switch (currentState)
        {
            //WHEN SETUP PHASE => CAN BUILD, CAN PAUSE, CAN MOVE
            case GameStates.WaveSetup:
                BuildingManager.Instance.enabled = true;
                FreeFlyCamera.instance.enabled = true;
                break;
            
            //WHEN WAVE ACTIVE PHASE => CANNOT BUILD
            case GameStates.WaveActive:
                BuildingManager.Instance.enabled = false;
                FreeFlyCamera.instance.enabled = true;
                break; 
            
            //WHEN THE WAVE IS COMPLETED, PLAY A FEEDBACK OR SOMETHING,
            //THEN GO TO SETUP PHASE
            case GameStates.WaveCompleted:
                //TODO: LOGIC
                break; 
            
            //WHEN PAUSE, CANNOT BUILD, CANNOT MOVE, CAN ONLY INTERACT WITH PAUSE UI
            case GameStates.Pause:
                BuildingManager.Instance.enabled = false;
                FreeFlyCamera.instance.enabled = false;
                break; 
            case GameStates.GameOver:
                //TODO: LOGIC
                break;
        }
    }
}

