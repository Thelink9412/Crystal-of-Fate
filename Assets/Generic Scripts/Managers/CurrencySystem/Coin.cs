using UnityEngine;

//Script che gestisce la collezione di monete; esistono 3 tipi di monete:
//copper = 1 
//silver = 5
//gold = 10
//Ogni moneta viene raccolta automaticamente 
public class Coin : MonoBehaviour
{
    public CoinType coinType;

    private void Start()
    {
        StartCoroutine(AutoCollectAfterDelay());
    }

    private System.Collections.IEnumerator AutoCollectAfterDelay()
    {
        yield return new WaitForSeconds(1f);
        CurrencyManager.Instance.AddCurrency(GetValue());
        Destroy(gameObject);
    }

    private int GetValue()
    {
        return coinType switch
        {
            CoinType.Copper => 1,
            CoinType.Silver => 5,
            CoinType.Gold => 10,
            _ => 0,
        };
    }
}
