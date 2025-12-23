using UnityEngine;
using UnityEngine.UI;
using TMPro;

//Script che gestisce la UI della currency in alto a destra dello schermo
public class CurrencyUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI coinText;

    private void Start()
    {
        UpdateCoinUI(CurrencyManager.Instance.GetCurrency());
        CurrencyManager.Instance.OnCurrencyChanged += UpdateCoinUI;
    }

    private void OnDestroy()
    {
        if (CurrencyManager.Instance != null)
            CurrencyManager.Instance.OnCurrencyChanged -= UpdateCoinUI;
    }

    public void UpdateCoinUI(int total)
    {
        coinText.text = total.ToString();
    }
}
