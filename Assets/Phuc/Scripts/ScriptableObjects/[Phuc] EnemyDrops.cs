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
        StartCoroutine(DeadCountDown());
    }
    //TODO: Add score before destroy the enemy
    IEnumerator DeadCountDown()
    {
        yield return new WaitForSeconds(5);
        onDead?.Invoke(coin);
        Destroy(gameObject);
    }
}
