using System;
using UnityEngine;

public class MainTowerCheck : MonoBehaviour
{
    [SerializeField] private MainTowerHealthUI mainTower; // Reference to the main tower script
    [SerializeField] private float damageAmount = 10f;   // Damage caused by an enemy

    private void Start()
    {
        mainTower = GetComponentInParent<MainTowerHealthUI>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            // Notify the main tower to apply damage
            mainTower.ApplyDamage(other.gameObject, damageAmount);
            
            AudioManager.Instance.PlaySoundEffect("GroundEnemy_SFX");
        }
    }
}