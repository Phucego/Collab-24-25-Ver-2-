using System;
using UnityEngine;

public class MainTowerCheck : MonoBehaviour
{
    [SerializeField] private MainTowerHealthUI mainTower; // Reference to the main tower script
    [SerializeField] private float damageAmount = 10f;   // Damage caused by an enemy

    private void Start()
    {
        mainTower = GetComponentInParent<MainTowerHealthUI>();

        if (mainTower == null)
        {
            Debug.LogError("MainTowerHealthUI not found on parent object!");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            // Notify the main tower to apply damage
            if (mainTower != null)
            {
                mainTower.ApplyDamage(other.gameObject, damageAmount);
            }

            // Trigger the vignette effect if LevelManager is available
            if (LevelManager.instance != null)
            {
                LevelManager.instance.TriggerVignetteEffect();
            }
            else
            {
                Debug.LogError("LevelManager instance not found!");
            }

            // Play sound effect
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySoundEffect("GroundEnemy_SFX");
            }
            else
            {
                Debug.LogError("AudioManager instance not found!");
            }
        }
    }
}