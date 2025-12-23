using UnityEngine;
using TMPro;
using UnityEngine.UI;

//Script per gestire la UI dell'upgrade delle Statistiche alla fine di ogni round; ci saranno 4 stats da poter migliorare;
//eventualmente, si può scegliere di annullare dei miglioramenti guadagnando un po' di soldi e permettere al player
//di riassegnare i miglioramenti in maniera più dinamica; naturalmente ci sono dei valori limite sotto (o sopra) i quali
//non si possono più annullare i potenziamenti ed essi sono uguali alle stats iniziali di ciascun personaggio
public class UpgradeStatsUI : MonoBehaviour
{
    private PlayerBase player;
    #region UI Elements
    [SerializeField] private TextMeshProUGUI healthAmount;
    [SerializeField] private Button hpPlusButton;
    [SerializeField] private Button hpMinusButton;
    private float minHealthLimit;

    [SerializeField] private TextMeshProUGUI attackAmount;
    [SerializeField] private Button attackPlusButton;
    [SerializeField] private Button attackMinusButton;
    private float minAttackLimit;

    [SerializeField] private TextMeshProUGUI atkCdAmount;
    [SerializeField] private Button atkCdPlusButton;
    [SerializeField] private Button atkCdMinusButton;
    private float maxAtkCdLimit;

    [SerializeField] private TextMeshProUGUI abilityCdAmount;
    [SerializeField] private Button abilityCdPlusButton;
    [SerializeField] private Button abilityCdMinusButton;
    private float maxAbilityCdLimit;
    #endregion
    private enum StatType { HP, Attack, AtkCd, AbilityCd }
    #region Init and UpdateUI
    public void Initialize(PlayerBase p)
    {
        player = p;
        SetLimits();
        UpdateUI();

        hpPlusButton.onClick.AddListener(() => ChangeStat(StatType.HP, +5));

        if (player.runtimeStats.maxHealth > minHealthLimit)
            hpMinusButton.onClick.AddListener(() => ChangeStat(StatType.HP, -5));

        attackPlusButton.onClick.AddListener(() => ChangeStat(StatType.Attack, +2));

        if (player.runtimeStats.attack > minAttackLimit)
            attackMinusButton.onClick.AddListener(() => ChangeStat(StatType.Attack, -2));

        if (player.runtimeStats.atkCd < maxAtkCdLimit)
            atkCdPlusButton.onClick.AddListener(() => ChangeStat(StatType.AtkCd, +0.1f));

        atkCdMinusButton.onClick.AddListener(() => ChangeStat(StatType.AtkCd, -0.1f));

        if (player.runtimeStats.abilityCd < maxAbilityCdLimit)
            abilityCdPlusButton.onClick.AddListener(() => ChangeStat(StatType.AbilityCd, +0.1f));

        abilityCdMinusButton.onClick.AddListener(() => ChangeStat(StatType.AbilityCd, -0.1f));
    }

    private void SetLimits()
    {
        minAttackLimit = player.stats.attack;
        minHealthLimit = player.stats.maxHealth;
        maxAtkCdLimit = player.stats.atkCd;
        maxAbilityCdLimit = player.stats.abilityCd;
    }

    private void UpdateUI()
    {
        var stats = player.runtimeStats;

        healthAmount.text = $"{stats.maxHealth}";
        attackAmount.text = $"{stats.attack}";
        atkCdAmount.text = $"{stats.atkCd.ToString("F1")}";
        abilityCdAmount.text = $"{stats.abilityCd.ToString("F1")}";
    }
    #endregion
    #region ChangeStat
    private void ChangeStat(StatType type, float amount)
    {
        switch (type)
        {
            case StatType.HP:
                if (!CurrencyManager.Instance.SpendCurrency(amount > 0 ? 5 : -3))
                    return;

                player.runtimeStats.maxHealth += amount;
                player.UpdateCurrentHealth(amount);


                hpMinusButton.onClick.RemoveAllListeners();
                if (player.runtimeStats.maxHealth > minHealthLimit)
                    hpMinusButton.onClick.AddListener(() => ChangeStat(StatType.HP, -5));

                break;

            case StatType.Attack:
                if (!CurrencyManager.Instance.SpendCurrency(amount > 0 ? 5 : -3))
                    return;

                player.runtimeStats.attack += amount;
                attackMinusButton.onClick.RemoveAllListeners();
                if (player.runtimeStats.attack > minAttackLimit)
                    attackMinusButton.onClick.AddListener(() => ChangeStat(StatType.Attack, -2));

                break;

            case StatType.AtkCd:
                if (player.runtimeStats.atkCd + amount < 1f)
                {
                    player.runtimeStats.atkCd = 1f; //per evitare che la cd diventi negativa
                    break;
                }

                if (!CurrencyManager.Instance.SpendCurrency(amount < 0 ? 10 : -5))
                    return;

                player.runtimeStats.atkCd += amount;
                atkCdPlusButton.onClick.RemoveAllListeners();
                if (player.runtimeStats.atkCd < maxAtkCdLimit || player.runtimeStats.atkCd > 1f) //per evitare che la cd diventi negativa
                    atkCdPlusButton.onClick.AddListener(() => ChangeStat(StatType.AtkCd, +0.1f));

                break;

            case StatType.AbilityCd:
                if (player.runtimeStats.abilityCd + amount < 10f)
                {
                    player.runtimeStats.abilityCd = 10f; //per evitare che la cd diventi negativa
                    break;
                }

                if (!CurrencyManager.Instance.SpendCurrency(amount < 0 ? 10 : -5))
                    return;

                player.runtimeStats.abilityCd += amount;
                abilityCdPlusButton.onClick.RemoveAllListeners();
                if (player.runtimeStats.abilityCd < maxAbilityCdLimit)
                    abilityCdPlusButton.onClick.AddListener(() => ChangeStat(StatType.AbilityCd, +0.1f));

                break;

        }

        UpdateUI();
    }
    #endregion
}
