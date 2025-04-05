using UnityEngine;

public class MainTowerCheck : MonoBehaviour
{
    [SerializeField] private MainTowerHealthUI mainTower; // Reference to the main tower script
    [SerializeField] private float damageAmount = 10f;    // Damage caused by an enemy

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
            if (mainTower != null)
            {
                mainTower.ApplyDamage(other.gameObject, damageAmount);
            }

            if (LevelManager.instance != null)
            {
                LevelManager.instance.TriggerVignetteEffect();
            }
            else
            {
                Debug.LogError("LevelManager instance not found!");
            }

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