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
    public static MainTowerHealthUI Instance; 
    void Start()
    {
        currentHealth = maxHealth;

    }   
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;
    }

    void Update()
    {
        // Clamp current health
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

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
