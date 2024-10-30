using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyDrops : MonoBehaviour
{
    public delegate void OnDead(int score);
    private OnDead onDead;
    int score = 10;

    public void InitEnemy(OnDead deadCallback)
    {
        onDead = deadCallback;
        StartCoroutine(DeadCountDown());
    }

    IEnumerator DeadCountDown()
    {
        yield return new WaitForSeconds(5);
        onDead?.Invoke(score);
        Destroy(gameObject);
    }
}
