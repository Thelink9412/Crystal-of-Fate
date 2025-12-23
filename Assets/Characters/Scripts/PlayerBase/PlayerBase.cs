using UnityEngine;

//Script per i personaggi del player, l'idea è quella di aggiungere ad ogni PG un'abilità unica che si attiva 
//quando una relativa barra sarà piena e quando il player clicca su una futura sidebar sull'icona del relativo PG
//Ci sono due tipi di PG:
//a distanza => attaccano un solo nemico per volta da qualsiasi range
//melee => attaccano tutti i nemici vicini 
public class PlayerBase : MonoBehaviour
{
    #region Variables
    [SerializeField] private Healthbar hb;
    protected AbilityUI abilityUI;
    protected RoomManager roomManager;
    public PlayerStats stats;
    [HideInInspector] public PlayerStats runtimeStats;
    protected float currentHealth;
    protected Transform target;
    [HideInInspector] public EnemyBase currentEnemy;
    private bool isPoisoned = false;
    private bool isStunned = false;
    #endregion
    #region Init and Target
    protected virtual void Start()
    {
        FindTarget();
    }

    protected void Init()
    {
        runtimeStats = Instantiate(stats);
        UpdateCurrentHealth(runtimeStats.maxHealth);
    }

    public void AssignRoom(RoomManager manager)
    {
        roomManager = manager;
    }

    public RoomManager GetRoom()
    {
        return roomManager;
    }

    //Metodo per gestire l'acquisizione di un nuovo target
    protected void FindTarget()
    {
        EnemyBase enemy = roomManager.GetFirstActiveEnemy();
        if (enemy != null && enemy.GetActive())
        {
            currentEnemy = enemy;
            target = currentEnemy.transform;
        }
        else
        {
            currentEnemy = null;
            target = null;
        }
    }

    #endregion
    #region Damage and Status
    public virtual void TakeDamage(float amount)
    {
        UpdateCurrentHealth(-amount);
        if (currentHealth <= 0) Die();
    }

    public void TakePoisonDamage(float poisonTick, float poisonDuration, float poisonDamage)
    {
        if (!isPoisoned)
            StartCoroutine(PoisonCoroutine(poisonTick, poisonDuration, poisonDamage));
    }

    private System.Collections.IEnumerator PoisonCoroutine(float tick, float duration, float dmg)
    {
        isPoisoned = true;
        float timer = 0f;

        while (timer < duration)
        {
            TakeDamage(dmg);
            yield return new WaitForSeconds(tick);
            timer += tick;
        }

        isPoisoned = false;
    }

    public void ApplyStun(float stunDuration)
    {
        if (!isStunned)
            StartCoroutine(StunCoroutine(stunDuration));
    }

    private System.Collections.IEnumerator StunCoroutine(float duration)
    {
        isStunned = true;
        yield return new WaitForSeconds(duration);
        isStunned = false;
    }

    protected bool IsStunned()
    {
        return isStunned;
    }

    public void UpdateCurrentHealth(float amount)
    {
        currentHealth += amount;
        if (currentHealth > runtimeStats.maxHealth)
            currentHealth = runtimeStats.maxHealth;

        hb.UpdateHealthbar(runtimeStats.maxHealth, currentHealth);
    }

    protected virtual void Die()
    {
        GameManager.Instance.GameOver();
        Destroy(gameObject);
    }
    #endregion
    #region Special Ability
    
    public bool TryActivateSpecial()
    {
        if (roomManager.IsRoomActive() && !IsStunned() && !GameManager.Instance.IsPaused)
        {
            StartCoroutine(ActivateSpecialAbility());
            return true;
        }
        Debug.Log("Non si è attivata l'abilità!");
        return false;
    }

    protected virtual System.Collections.IEnumerator ActivateSpecialAbility()
    {
        yield return new WaitForSeconds(0f);
    }
    #endregion
    #region Save and Load
    public string characterId;
    public void Save(ref PlayerSaveData data)
    {
        data.characterId = characterId;
        data.maxHealth = runtimeStats.maxHealth;
        data.attack = runtimeStats.attack;
        data.atkCd = runtimeStats.atkCd;
        data.abilityCd = runtimeStats.abilityCd;
    }

    public void Load(PlayerSaveData data)
    {
        if (runtimeStats == null)
        {
            Debug.LogError("Runtime stats non inizializzati!");
            return;
        }

        Debug.Log($"Caricamento {data.characterId} con vita: {data.maxHealth}, attacco: {data.attack}, attacco CD: {data.atkCd}, abilità CD: {data.abilityCd}");
        runtimeStats.maxHealth = data.maxHealth;
        runtimeStats.attack = data.attack;
        runtimeStats.atkCd = data.atkCd;
        runtimeStats.abilityCd = data.abilityCd;
        Debug.Log($"Caricato {characterId} con vita: {runtimeStats.maxHealth}, attacco: {runtimeStats.attack}");
    }

    #endregion
    public void SetAbilityUI(AbilityUI ui)
    {
        abilityUI = ui;
    }
}

[System.Serializable]
public struct PlayerSaveData
{
    public string characterId;
    public float maxHealth;
    public float attack;
    public float atkCd;
    public float abilityCd;
}