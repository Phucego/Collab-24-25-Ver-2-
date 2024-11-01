using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyBehavior : MonoBehaviour
{
    [SerializeField] private BaseEnemySO data;

    // [ ENEMY'S UI ] //
    private Enemy_HPBar bar;

    // [ ENEMY'S DATA] //
    private float health, speed;

    void Start()
    {
        health = data.maxHealth;
        speed = data.maxSpeed;

        bar = GetComponentInChildren<Enemy_HPBar>();

        GetComponent<Renderer>().material.color = data.color;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Z))
            health -= 5;

        if (health > 0)
            bar.setHealth(health, data.maxHealth);
        else
            Death();
    }

    public void Death()
    {
        Destroy(gameObject);
    }
}
