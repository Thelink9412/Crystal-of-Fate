using UnityEngine;

//Script base per i nemici: ogni nemico avrà una caratteristica diversa, una tra 3 rarità e alla morte
//droppa una moneta dal valore che dipende dalla rarità
//Un problema che non riesco a risolvere è che quando un qualsiasi nemico droppa una moneta, il suo valore 
//è corretto, ma spawna sempre con lo sprite della moneta d'oro
public class EnemyBase : MonoBehaviour
{
    #region Variables
    [SerializeField] protected Healthbar hb;
    [SerializeField] protected float minRangedDistance = 2f;
    [Header("Loot Prefabs")]
    [SerializeField] private GameObject copperCoinPrefab;
    [SerializeField] private GameObject silverCoinPrefab;
    [SerializeField] private GameObject goldCoinPrefab;
    [SerializeField] private EnemyRarity rarity;

    private bool active = true;
    private Vector3 direction;
    public EnemyStats stats;
    [HideInInspector] public EnemyStats runtimeStats;
    protected float currentHealth;
    protected Transform target;
    protected RoomManager roomManager;
    #endregion
    #region Init and Update
    protected void Start()
    {
        target = roomManager.GetPlayer()?.transform;
        SetRuntimeStats();
        Debug.Log("Enemy initialized with stats:\nHealth" + runtimeStats.health + "\nSpeed: " + runtimeStats.speed + "\nDamage: " + runtimeStats.damage);
        currentHealth = runtimeStats.health;
        hb.UpdateHealthbar(runtimeStats.health, currentHealth);
        roomManager?.RegisterEnemy(this);
    }

    //Aggiungo un piccolo offset verticale alla direzione dei nemici in modo da non farli convergere tutti nello
    //stesso punto e avere un po' meno confusione visiva
    public void Init(Vector3 targetPosition)
    {
        float verticalOffset = Random.Range(-1f, 1f);
        Vector3 offset = new Vector3(0f, verticalOffset, 0f);
        Vector3 finalTarget = targetPosition + offset;
        direction = (finalTarget - transform.position).normalized;
    }

    protected void Update()
    {
        float distance = Vector3.Distance(transform.position, target.position);
        if (target != null && distance >= minRangedDistance && !GameManager.Instance.IsPaused)
            transform.position += direction * runtimeStats.speed * Time.deltaTime;
    }
    #endregion
    #region Damage and Death
    public virtual void TakeDamage(float amount)
    {
        currentHealth -= amount;
        hb.UpdateHealthbar(runtimeStats.health, currentHealth);
        if (currentHealth <= 0) Die();
    }

    //Metodo e relativa Coroutine per il danno dell'abilità del Fire Wizard
    public void TakeFireSpecialDamage(float duration, float tick, float damagePerTick)
    {
        StartCoroutine(SpecialFireCoroutine(duration, tick, damagePerTick));
    }

    private System.Collections.IEnumerator SpecialFireCoroutine(float duration, float tick, float dmg)
    {
        float timer = 0f;

        while (timer < duration)
        {
            TakeDamage(dmg);
            yield return new WaitForSeconds(tick);
            timer += tick;
        }
    }

    protected virtual void Die()
    {
        DropLoot();
        roomManager?.UnregisterEnemy(this);
        Destroy(gameObject);
    }

    //Metodo di morte per la botola
    public void DieByHatch()
    {
        roomManager?.UnregisterEnemy(this);
        Destroy(gameObject);
    }

    //Metodo per verificare se si entra in contatto con una botola aperta
    void OnTriggerEnter2D(Collider2D other)
    {
        Hatch hatch = other.GetComponent<Hatch>();
        if (hatch != null && hatch.IsOpen())
            DieByHatch();
    }

    #endregion
    private void DropLoot()
    {
        GameObject coinToSpawn = null;

        switch (rarity)
        {
            case EnemyRarity.Common:
                coinToSpawn = copperCoinPrefab;
                break;
            case EnemyRarity.Rare:
                coinToSpawn = silverCoinPrefab;
                break;
            case EnemyRarity.Epic:
                coinToSpawn = goldCoinPrefab;
                break;
        }

        if (coinToSpawn != null)
        {
            Instantiate(coinToSpawn, transform.position, Quaternion.identity);
        }
    }

    private void SetRuntimeStats()
    {
        float multiplier = Mathf.Clamp(GameManager.Instance.Level, 1f, 1.5f);
        runtimeStats = new EnemyStats
        {
            health = stats.health * multiplier,
            speed = stats.speed * multiplier,
            damage = stats.damage * multiplier
        };
    }

    //Metodo per settare il nemico ad un RoomManager, necessario per permettere al player di attaccare
    //soltanto i nemici appartenenti alla stessa stanza; allo stesso modo i nemici attaccano il player
    //corrispondente
    public void SetRoomManager(RoomManager manager)
    {
        roomManager = manager;
    }

    #region Methods for Skeleton
    //Metodi per Skeleton: quando muore la prima volta il target del player passa al nemico successivo senza
    //rimuovere lo Skeleton dalla lista di nemici
    public void SetActiveTrue()
    {
        active = true;
    }

    public void SetActiveFalse()
    {
        active = false;
    }

    public bool GetActive()
    {
        return active;
    }
    #endregion
}
