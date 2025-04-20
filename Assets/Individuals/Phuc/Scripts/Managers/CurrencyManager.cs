using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
public class CurrencyManager : MonoBehaviour
{
    public static CurrencyManager Instance;

    private GameObject enemyCheck;

    public int currentCurrency;
    GameObject go;
    [SerializeField] private TextMeshProUGUI currencyText; // UI Text element to display currency
    public SceneField tutorialScene;
    public SceneField Level1;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        if (SceneManager.GetActiveScene().name == Level1.SceneName)
        {
            InitializeCurrency(50);
        }
    }

    // Initializes the starting currency and updates the UI
    public void InitializeCurrency(int startingAmount)
    {
        currentCurrency = startingAmount;
        UpdateCurrencyUI();
    }

    // Updates the currency UI
    private void UpdateCurrencyUI()
    {
        if (currencyText != null)
        {
            currencyText.text = $"Coins: {currentCurrency}";
        }
        else
        {
            Debug.LogWarning("Currency Text UI is not assigned.");
        }
       
    }

    public int GetCurrency()
    {
        return currentCurrency;
    }

    public bool HasEnoughCurrency(int amount)
    {
        return currentCurrency >= amount;
    }

    public void DeductCurrency(int amount)
    {
        if (HasEnoughCurrency(amount))
        {
            currentCurrency -= amount;
            UpdateCurrencyUI(); // Refresh UI after deduction
        }
    }
}