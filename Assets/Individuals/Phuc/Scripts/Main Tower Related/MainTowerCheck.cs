using UnityEngine;
using System.Collections.Generic;

public class MainTowerCheck : MonoBehaviour
{
    [SerializeField] private float damageAmount = 10f;
    private HashSet<GameObject> damagedEnemies = new HashSet<GameObject>();

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            if (!damagedEnemies.Contains(other.gameObject))
            {
                damagedEnemies.Add(other.gameObject);

                if (MainTowerStateManager.Instance != null)
                {
                    MainTowerHealthUI.Instance.ApplyDamage(other.gameObject, damageAmount);
                }

                if (LevelManager.instance != null)
                {
                    LevelManager.instance.TriggerVignetteEffect();
                }

                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlaySoundEffect("GroundEnemy_SFX");
                }
            }
        }
    }
}