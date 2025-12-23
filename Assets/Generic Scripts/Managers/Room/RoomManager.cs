using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

//Script per gestire una singola stanza
public class RoomManager : MonoBehaviour
{
    #region Variables
    [Header("Player and enemies settings")]
    [SerializeField] private Transform spawnPoint;  //Spawnpoint del player

    //Prefab globali dei PG
    [SerializeField] private GameObject[] playerPrefabs;
    [SerializeField] private EnemySpawner spawner;
    //UI di selezione del PG
    [SerializeField] private GameObject pgUI;
    [SerializeField] private Button pgButton;
    [SerializeField] private Button closePgButton;

    private GameObject playerToSpawn;
    private PlayerBase player;
    public UpgradeStatsUI upgradeStatsUI;
    private UpgradeStatsUI upgradeCopy;
    private Transform spawnUI;
    private List<EnemyBase> activeEnemies = new List<EnemyBase>();
    private bool roundActive = false;
    private bool isSpawningEnded = false;
    protected int currentRound;
    #endregion
    #region Room Management
    protected virtual void Start()
    {
        pgUI.GetComponent<ChoosePGUI>()?.Initialize(this);
        pgButton.onClick.AddListener(() => ShowPGUI());
        closePgButton.onClick.AddListener(() => ClosePGUI());
        List<EnemySpawner.EnemyEntry> roomEnemyPool = GameManager.Instance.GetRandomEnemyPool();
        spawner.SetEnemyPool(roomEnemyPool);

        spawner.SetRoomManager(this);   //Collego lo spawner con la stanza
        spawnUI = transform.Find("SpawnUI");
    }

    public void StartRound(int cr)
    {
        currentRound = cr;
        roundActive = true;
        activeEnemies.Clear();
        spawner.StartSpawning(currentRound, OnSpawningComplete);
    }

    private void OnSpawningComplete()
    {
        isSpawningEnded = true;
    }

    public bool IsRoomActive()
    {
        return roundActive;
    }

    //Controllo se tutti i nemici della stanza sono morti
    private void CheckRoomCleared()
    {
        if (roundActive && isSpawningEnded && !HasEnemies())
        {
            roundActive = false;
            GameManager.Instance.NotifyRoomCleared(this);
        }
    }
    
    #endregion
    #region Player and Enemies Management
    //Istanzio il player
    public void SpawnPlayer()
    {
        GameObject p = Instantiate(playerToSpawn, spawnPoint.position, Quaternion.identity, transform);
        player = p.GetComponent<PlayerBase>();
        player.AssignRoom(this);
    }

    public PlayerBase GetPlayer()
    {
        return player;
    }

    public void RegisterEnemy(EnemyBase enemy)
    {
        if (!activeEnemies.Contains(enemy))
            activeEnemies.Add(enemy);
    }

    public void UnregisterEnemy(EnemyBase enemy)
    {
        activeEnemies.Remove(enemy);
        CheckRoomCleared();
    }

    public bool HasEnemies()
    {
        return activeEnemies.Count > 0;
    }

    public EnemyBase GetFirstActiveEnemy()
    {
        foreach (var e in activeEnemies)
        {
            if (e.GetActive()) return e;
        }
        return null;
    }

    //Setto il player in base a quanto scelto tramite la UI
    public void SetPlayer(string playerName, string previousPG)
    {
        playerToSpawn = playerName switch
        {
            "Fire Wizard" => playerPrefabs[0],
            "Knight" => playerPrefabs[1],
            "Archer" => playerPrefabs[2],
            "Satyr" => playerPrefabs[3],
            "Kitsune" => playerPrefabs[4],
            _ => playerPrefabs[5],
        };
        //Mando le notifiche al GameManager per aggiornare le UI di tutte le altre stanze
        GameManager.Instance.NotifyPGSelected(playerName, previousPG);
    }

    public void PlayerHasBeenSelected(string name, string previousPG)
    {
        pgUI.GetComponent<ChoosePGUI>()?.PlayerSelected(name, previousPG);
    }

    public List<EnemyBase> GetActiveEnemies()
    {
        return activeEnemies;
    }

    public EnemySpawner GetEnemySpawner()
    {
        return spawner;
    }
    #endregion
    #region UI Methods
    public void ShowUpgradeUI()
    {
        if (upgradeStatsUI != null && player != null)
        {
            upgradeCopy = Instantiate(upgradeStatsUI, spawnUI.position, Quaternion.identity, spawnUI);
            upgradeCopy.Initialize(player);
        }
    }

    private void ShowPGUI()
    {
        pgUI.SetActive(true);
    }

    private void ClosePGUI()
    {
        pgUI.SetActive(false);
    }

    public void DestroyUI()
    {
        if (currentRound == 0)
        {
            Destroy(pgButton.gameObject);
            Destroy(pgUI);
        }

        if (upgradeCopy != null)
            Destroy(upgradeCopy.gameObject);
    }
    #endregion
}
