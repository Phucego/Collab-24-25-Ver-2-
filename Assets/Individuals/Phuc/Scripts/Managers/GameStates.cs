using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameStates
{
    WaveSetup,
    WaveActive,
    WaveCountdown, // New state for wave countdown
    WaveCompleted,
    Pause,
    GameOver
}

