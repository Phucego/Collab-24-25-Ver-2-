using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyDrops : MonoBehaviour
{
    public delegate void OnDead(int score);
    private OnDead onDead;
    public int coin;

    //TODO: Initialize call back to update currencies
    public void InitEnemy(OnDead deadCallback)
    {
        onDead = deadCallback;
        DeathBehaviour();
    }
    //TODO: Add score before destroy the enemy
    private void DeathBehaviour()
    {
        onDead?.Invoke(coin);
        Destroy(gameObject);
    }
    
}
