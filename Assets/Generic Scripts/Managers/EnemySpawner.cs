using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;

//Script che gestisce lo spawn dei nemici; ogni stanza avrà un pool locale di 4 nemici generato randomicamente; per
//questo si avrà la possibilità di attivare una UI per mostrare i nemici che possono essere spawnati
//nella stanza e le relative percentuali
public class EnemySpawner : MonoBehaviour
{
    [System.Serializable]
    public class EnemyEntry
    {
        public GameObject prefab;
        public EnemyRarity rarity;
    }

    private Transform enemiesParent;

    [Header("Percentages")]
    //Percentuali di spawn delle varie rarità
    [Range(0, 100)] public float commonChance;
    [Range(0, 100)] public float rareChance;
    [Range(0, 100)] public float epicChance;
    #region Variables
    [SerializeField] private float spawnInterval;
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private List<Sprite> globalEnemySpriteList;

    //Elementi delle UI
    [SerializeField] private GameObject enemySpawnStatsUI;
    [SerializeField] private SpriteRenderer[] sprites;
    [SerializeField] private TextMeshProUGUI[] probs;

    private bool enemyStatsEnabled = false;
    private int spawnedEnemies = 0;
    private RoomManager roomManager;
    private List<EnemyEntry> localEnemyPool = new List<EnemyEntry>();       //pool locale generato randomicamente dal GameManager
    private List<Sprite> enemySpriteList = new List<Sprite>();
    #endregion
    #region Initialization
    public void SetEnemyPool(List<EnemyEntry> pool)
    {
        localEnemyPool = pool;
        SetEnemySprites();
        SetEnemySpawnStatsUI();
    }

    //Collego lo spawner ad una stanza e setto il GameObject parent dove istanziare i nemici
    public void SetRoomManager(RoomManager manager)
    {
        roomManager = manager;
        enemiesParent = manager.transform.Find("Enemies");

        if (enemiesParent == null)
            Debug.LogError("Enemies container non trovato nella stanza!");
    }
    #endregion
    #region UI Management
    private void SetEnemySprites()
    {
        foreach (EnemyEntry e in localEnemyPool)
        {
            EnemyBase enemy = e.prefab.GetComponent<EnemyBase>();
            if (enemy != null)
                enemySpriteList.Add(globalEnemySpriteList[GetEnemySpriteIndex(enemy)]);

        }
    }

    private int GetEnemySpriteIndex(EnemyBase enemy)
    {
        return enemy switch
        {
            Werewolf => 0,
            SkeletonWarrior => 1,
            Plent => 2,
            Karasu => 3,
            Yamabushi => 4,
            Gorgon => 5,
            FireSpirit => 6,
            Minotaur => 7,
            _ => -1,
        };
    }

    private void SetEnemySpawnStatsUI()
    {
        for (int i = 0; i < localEnemyPool.Count; i++)
        {
            sprites[i].sprite = enemySpriteList[i];
            float prob = (global::System.Object)localEnemyPool[i].rarity switch
            {
                EnemyRarity.Common => commonChance,
                EnemyRarity.Rare => rareChance,
                _ => epicChance,
            };
            probs[i].text = prob.ToString();
        }
    }

    public void ShowEnemySpawnStatsUI()
    {
        if (enemyStatsEnabled) DisableEnemySpawnStatsUI();
        else EnableEnemySpawnStatsUI();
    }

    public void EnableEnemySpawnStatsUI()
    {
        enemySpawnStatsUI.SetActive(true);
        enemyStatsEnabled = true;
    }

    public void DisableEnemySpawnStatsUI()
    {
        enemySpawnStatsUI.SetActive(false);
        enemyStatsEnabled = false;
    }
    #endregion
    #region Spawn Logic
    public void StartSpawning(int round, System.Action onComplete)
    {
        StartCoroutine(SpawnEnemies(round, onComplete));
    }

    private IEnumerator SpawnEnemies(int round, System.Action onComplete)
    {
        int numEnemies = 2 + round;
        for (int i = 0; i < numEnemies; i++)
        {
            while (GameManager.Instance.IsPaused)
                yield return null;

            if (GameManager.Instance.IsGameOver)
                yield break;

            GameObject enemyPrefab = GetRandomEnemyByRarity();
            int index = Random.Range(0, spawnPoints.Length);
            GameObject enemyObj = Instantiate(enemyPrefab, spawnPoints[index].position, Quaternion.identity, enemiesParent);
            EnemyBase enemy = enemyObj.GetComponent<EnemyBase>();
            enemy.SetRoomManager(roomManager);
            roomManager.RegisterEnemy(enemy);
            spawnedEnemies++;
            yield return new WaitForSeconds(spawnInterval);
        }

        onComplete?.Invoke();
    }

    private GameObject GetRandomEnemyByRarity()
    {
        float roll = Random.Range(0f, 100f);

        EnemyRarity selectedRarity;

        if (roll <= epicChance)
            selectedRarity = EnemyRarity.Epic;
        else if (roll <= rareChance + epicChance)
            selectedRarity = EnemyRarity.Rare;
        else
            selectedRarity = EnemyRarity.Common;

        List<GameObject> candidates = new List<GameObject>();   //Prendo tutti nemici di quella rarità;

        foreach (var entry in localEnemyPool)
        {
            if (entry.rarity == selectedRarity)
                candidates.Add(entry.prefab);
        }

        if (candidates.Count == 0)
            return localEnemyPool[0].prefab;

        return candidates[Random.Range(0, candidates.Count)];
    }
    #endregion
}
