using UnityEngine;

public class MainTowerStateManager : MonoBehaviour
{
    [Header("Health Reference")]
    [SerializeField] private MainTowerHealthUI towerHealth;

    [Header("Castle Fire Effects")]
    [SerializeField] private GameObject fireEffect50;
    [SerializeField] private GameObject fireEffect30;
    [SerializeField] private GameObject destroyedEffect;

    private TowerState currentState = TowerState.Healthy;
    public static MainTowerStateManager Instance; 
    private enum TowerState
    {
        Healthy,
        Damaged50,
        Damaged30,
        Destroyed
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

    private void Start()
    {
        // Auto-find the tower health reference if not set
        if (towerHealth == null)
            towerHealth = GetComponent<MainTowerHealthUI>();

        // Auto-find effect objects if not manually assigned
        if (fireEffect50 == null)
            fireEffect50 = GameObject.Find("TOWER CONDITION - 50%");

        if (fireEffect30 == null)
            fireEffect30 = GameObject.Find("TOWER CONDITION - 30%");

        if (destroyedEffect == null)
            destroyedEffect = GameObject.Find("TOWER DESTROYED EFFECT");

        // Disable all effects initially
        fireEffect50?.SetActive(false);
        fireEffect30?.SetActive(false);
        destroyedEffect?.SetActive(false);
    }

    private void Update()
    {
        if (towerHealth == null) return;

        float healthPercent = (towerHealth.currentHealth / towerHealth.maxHealth) * 100f;
        TowerState newState = GetTowerState(healthPercent);

        if (newState != currentState)
        {
            currentState = newState;
            HandleStateChange(currentState);
        }
    }

    private TowerState GetTowerState(float percent)
    {
        if (towerHealth.currentHealth <= 0f)
            return TowerState.Destroyed;
        else if (percent <= 30f)
            return TowerState.Damaged30;
        else if (percent <= 50f)
            return TowerState.Damaged50;
        else
            return TowerState.Healthy;
    }

    private void HandleStateChange(TowerState state)
    {
        // Turn off all effects before setting the new one
        fireEffect50?.SetActive(false);
        fireEffect30?.SetActive(false);
        destroyedEffect?.SetActive(false);

        switch (state)
        {
            case TowerState.Damaged50:
                fireEffect50?.SetActive(true);
                break;

            case TowerState.Damaged30:
                fireEffect30?.SetActive(true);
                break;

            case TowerState.Destroyed:
                destroyedEffect?.SetActive(true);
                Debug.Log("Tower has been destroyed!");
                // Trigger losing sequence in UIManager
                if (UIManager.Instance != null)
                {
                    UIManager.Instance.StartLoseSequence();
                }
                break;
        }
    }
}