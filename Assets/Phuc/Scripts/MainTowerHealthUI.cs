using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class MainTowerHealthUI : MonoBehaviour, I_Damagable
{   
    public float currentHealth;
    public float maxHealth;
    
    [SerializeField] private float lerpSpeed;
    
    public Slider healthSlider;
    public Slider easeHealthSlider;
    
 
    void Start()
    {
        currentHealth = maxHealth;  
    }
    void Update()
    {
        
        if (healthSlider.value != currentHealth)
        {
            healthSlider.value = currentHealth;
        }

        if (healthSlider.value != easeHealthSlider.value)
        {
            easeHealthSlider.value = Mathf.Lerp(easeHealthSlider.value, currentHealth, lerpSpeed);
        }

   
    }
    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
    }

    public void ApplyDebuff(float smth)
    {
        
    }
}
