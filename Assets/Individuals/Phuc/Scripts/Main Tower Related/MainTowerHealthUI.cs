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

    [Header("Castle Fire Effects")]
    [SerializeField] private GameObject castle;
    [SerializeField] private GameObject fireEffect50;

    private enum TowerState
    {
        Healthy,
        Damaged50,
        Damaged30,
        Destroyed
    }

    private TowerState currentState = TowerState.Healthy;

    void Start()
    {
        currentHealth = maxHealth;

        // Find and cache references if not set manually
        if (castle == null)
            castle = GameObject.Find("CASTLE");
        if (fireEffect50 == null)
            fireEffect50 = GameObject.Find("TOWER CONDITION - 50%");

        fireEffect50.SetActive(false);
    }

    void Update()
    {
        // Update health sliders
        if (healthSlider.value != currentHealth)
        {
            healthSlider.value = currentHealth;
        }

        if (easeHealthSlider.value != currentHealth)
        {
            easeHealthSlider.value = Mathf.Lerp(easeHealthSlider.value, currentHealth, lerpSpeed * Time.deltaTime);

            if (Mathf.Abs(easeHealthSlider.value - currentHealth) < 0.01f)
            {
                easeHealthSlider.value = currentHealth;
            }
        }
        
        // Clamp current health
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        // Calculate percentage
        float healthPercent = (currentHealth / maxHealth) * 100f;

        // State machine simulation
        TowerState newState = GetTowerState(healthPercent);

        if (newState != currentState)
        {
            currentState = newState;
            HandleStateChange(currentState);
        }
    }
    
    TowerState GetTowerState(float healthPercent)
    {
        Debug.Log(healthPercent);
        if (healthPercent <= 30f)
            return TowerState.Damaged30;
        else if (healthPercent <= 50f)
            return TowerState.Damaged50;
        else
            return TowerState.Healthy;
    }

    void HandleStateChange(TowerState state)
    {
        switch (state)
        {
            case TowerState.Healthy:
                fireEffect50.SetActive(false);
                break;

            case TowerState.Damaged50:
                fireEffect50.SetActive(true);
                break;

            case TowerState.Damaged30:
                fireEffect50.SetActive(false);
                //TRIGGER 30%
                
                break;

            case TowerState.Destroyed:
                //WELL, DESTROYED
                break;
        }
    }

    public void ApplyDamage(GameObject enemy, float damage)
    {
        if (!damagedEnemies.Contains(enemy))
        {
            currentHealth -= damage;
            damagedEnemies.Add(enemy);
        }
    }
}
