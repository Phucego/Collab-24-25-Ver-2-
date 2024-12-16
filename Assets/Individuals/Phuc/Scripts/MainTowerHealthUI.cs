using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainTowerHealthUI : MonoBehaviour
{   
    public float currentHealth;
    public float maxHealth;

    [SerializeField] private float lerpSpeed;

    public Slider healthSlider;
    public Slider easeHealthSlider;

    private HashSet<GameObject> damagedEnemies = new HashSet<GameObject>();

    void Start()
    {
        currentHealth = maxHealth;  
    }

    void Update()
    {
        // Update health slider instantly
        if (healthSlider.value != currentHealth)
        {
            healthSlider.value = currentHealth;
        }

        // Smoothly update eased health slider
        if (easeHealthSlider.value != currentHealth)
        {
            easeHealthSlider.value = Mathf.Lerp(easeHealthSlider.value, currentHealth, lerpSpeed * Time.deltaTime);

            // Snap when close to avoid jittering
            if (Mathf.Abs(easeHealthSlider.value - currentHealth) < 0.01f)
            {
                easeHealthSlider.value = currentHealth;
            }
        }

        // Clamp current health
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
    }

    // This method is called by the checks when an enemy enters
    public void ApplyDamage(GameObject enemy, float damage)
    {
        // Only apply damage once per enemy
        if (!damagedEnemies.Contains(enemy))
        {
            currentHealth -= damage;
            damagedEnemies.Add(enemy);
        }
    }
}