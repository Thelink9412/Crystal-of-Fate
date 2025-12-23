using UnityEngine;

//Singleton che gestisce il sistema di currency
public class CurrencyManager : MonoBehaviour
{
    #region Variables
    public static CurrencyManager Instance;
    public event System.Action<int> OnCurrencyChanged;
    private LevelUIList ui;
    private int totalCurrency;
    #endregion
    #region Awake and SetUI
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        totalCurrency = 0;
    }

    public void SetUI(LevelUIList uiList)
    {
        ui = uiList;
    }
    #endregion
    #region Currency Management
    public void AddCurrency(int amount)
    {
        totalCurrency += amount;
        Debug.Log("Monete totali: " + totalCurrency);
        OnCurrencyChanged?.Invoke(totalCurrency);
    }

    public int GetCurrency()
    {
        return totalCurrency;
    }

    public void ResetCurrency()
    {
        totalCurrency = 0;
        OnCurrencyChanged?.Invoke(totalCurrency);
    }

    //Metodo per spendere i soldi e potenziare le statistiche
    public bool SpendCurrency(int amount)
    {
        if (amount > totalCurrency) return false;
        totalCurrency -= amount;
        OnCurrencyChanged?.Invoke(totalCurrency);
        return true;
    }
    #endregion
    #region SaveLoad
    public void Save(ref CurrencySaveData data)
    {
        data.saveCurrency = totalCurrency;
    }

    public void Load(CurrencySaveData data)
    {
        totalCurrency = data.saveCurrency;
        if (ui == null)
        {
            Debug.LogWarning("Currency UI not set. Cannot update currency display.");
            return;
        }
        if (ui.currencyUI == null)
        {
            Debug.LogWarning("Currency UI GameObject is null. Cannot update currency display.");
            return;
        }
        if (ui.currencyUI.GetComponent<CurrencyUI>() == null)
        {
            Debug.LogWarning("Currency UI component not found. Cannot update currency display.");
            return;
        }
        ui?.currencyUI.GetComponent<CurrencyUI>()?.UpdateCoinUI(totalCurrency);
        OnCurrencyChanged?.Invoke(totalCurrency);
    }
    #endregion
}

[System.Serializable]
public struct CurrencySaveData
{
    public int saveCurrency;
}