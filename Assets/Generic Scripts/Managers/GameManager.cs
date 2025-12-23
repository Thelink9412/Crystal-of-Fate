using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

//Singleton che gestisce l'andamento dei round; l'idea è quella di avere 4 livelli;
// il numero di stanze spawnate corrisponde al numero del livello
public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    #region Variables
    [Header("Player, rooms and enemies settings")]
    //Gruppo di template di stanze; le prime 4 si possono spawnare sempre, le ultime 2 dal livello 3 in poi
    [SerializeField] private GameObject[] roomsPrefab;

    //Lista globale dei prefab dei nemici per poter scegliere allo spawn della stanza quali nemici inserire nel pool locale
    [SerializeField] private List<EnemySpawner.EnemyEntry> globalEnemyList;

    //Lista globale delle icone delle abilità da poter spawnare 
    [SerializeField] private GameObject[] abilityIcons;
    [SerializeField] private AudioClip[] musics;
    private GameObject blur;

    private LevelUIList ui;
    private TitleScreenUI titleScreenUI;
    private TutorialUI tutorialUI;
    private int tutorialSlidesIndex = 0;
    private string currentScene;
    [SerializeField] private int maxRounds = 4;
    private int currentRound = 0;
    public int Level { get; private set; } = 0;
    private int choosenPG = 0;
    private List<RoomManager> roomsLoaded = new List<RoomManager>();    //Stanze caricate
    private HashSet<RoomManager> clearedRooms = new HashSet<RoomManager>();     //Stanze terminate
    private List<PlayerBase> activePlayers = new List<PlayerBase>();    //Player attivi
    private List<AbilityUI> abilityUItoSpawn = new List<AbilityUI>();   //Icone delle abilità da spawnare in base ai PG
    public bool IsGameOver { get; private set; } = false;
    public bool IsPaused { get; private set; } = false;
    #endregion
    #region Start and Update
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        SetCurrentScene(SceneManager.GetActiveScene().name);

        SceneManager.LoadSceneAsync("Title Screen", LoadSceneMode.Additive);
    }

    private void Update()
    {
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (IsPaused)
            {
                IsPaused = false;
                blur.SetActive(false);
                ui.pauseUI.SetActive(false);
            }
            else
            {
                IsPaused = true;
                blur.SetActive(true);
                ui.pauseUI.SetActive(true);
            }
        }
    }
    #endregion
    #region Scene Management
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SceneManager.SetActiveScene(scene);
        SetCurrentScene(scene.name);

        if (IsSceneLevel())
        {
            if (Level == 1)
                SaveManager.ClearSave();
            MusicManager.instance.PlayMusic(musics[1]);
            StartCoroutine(SetLevelListeners());
            SaveManager.LoadCurrencyAndLevel();
            Invoke("SpawnRooms", 0.3f);
        }

        else if (IsSceneTitleScreen())
        {
            StartCoroutine(SetTitleListeners());
            MusicManager.instance.PlayMusic(musics[0]);
        }

        else if (IsSceneTutorial())
            StartCoroutine(SetTutorialListeners());
        else
            MusicManager.instance.PlayMusic(musics[2]);

    }

    private System.Collections.IEnumerator StartTutorialRoutine()
    {
        string tutorialSceneName = "Tutorial";
        AsyncOperation unloadOp = SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene());
        yield return unloadOp;

        Debug.Log($"Caricamento della scena tutorial: {tutorialSceneName}");
        AsyncOperation loadOp = SceneManager.LoadSceneAsync(tutorialSceneName, LoadSceneMode.Additive);
        yield return loadOp;

        SetCurrentScene(tutorialSceneName);
    }

    private void SetCurrentScene(string newSceneName)
    {
        currentScene = newSceneName;
    }

    private bool IsSceneLevel()
    {
        return currentScene.StartsWith("Level");
    }

    private bool IsSceneTitleScreen()
    {
        return currentScene == "Title Screen";
    }

    private bool IsSceneTutorial()
    {
        return currentScene == "Tutorial";
    }
    
    /*public System.Collections.IEnumerator LoadMainMenu()
    {
        string mainMenuSceneName = "Title Screen";
        AsyncOperation unloadOp = SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene());
        yield return unloadOp;

        Debug.Log($"Caricamento del Main Menu: {mainMenuSceneName}");
        AsyncOperation loadOp = SceneManager.LoadSceneAsync(mainMenuSceneName, LoadSceneMode.Additive);
        yield return loadOp;

        SaveManager.ClearSave();
        ResetRoomsStats();
        SetCurrentScene(mainMenuSceneName);
    }*/

    private System.Collections.IEnumerator LoadEndCreditsRoutine()
    {
        string endCreditsSceneName = "End Credits";
        AsyncOperation unloadOp = SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene());
        yield return unloadOp;

        Debug.Log($"Caricamento delle End Credits: {endCreditsSceneName}");
        AsyncOperation loadOp = SceneManager.LoadSceneAsync(endCreditsSceneName, LoadSceneMode.Additive);
        yield return loadOp;

        SetCurrentScene(endCreditsSceneName);
    }
    #endregion

    #region Game Management
    public void StartNextRound()
    {
        currentRound++;
        clearedRooms.Clear();
        ui.roundPopupUI.SetActive(false);
        foreach (RoomManager room in roomsLoaded)
            room.StartRound(currentRound);

        foreach (AbilityUI a in abilityUItoSpawn)
            a.EnableAbilityCd();

    }

    private void StartNewLevel()
    {
        StartCoroutine(LoadNextLevelRoutine());
    }

    private void RestartLevel()
    {
        StartCoroutine(ReloadLevelRoutine());
    }

    public void GameOver()
    {
        IsGameOver = true;
        ui.gameOverPopupUI.SetActive(true);
        ui.gameOverButton.onClick.RemoveAllListeners();
        ui.gameOverButton.onClick.AddListener(() => RestartLevel());
    }
    #endregion
    #region Enemy pool Generation
    
    //Metodo per assegnare alle stanze un pool locale generato randomicamente di 4 nemici (2 comuni, 1 raro, 1 epico)
    public List<EnemySpawner.EnemyEntry> GetRandomEnemyPool()
    {
        List<EnemySpawner.EnemyEntry> pool = new List<EnemySpawner.EnemyEntry>();

        var commons = globalEnemyList.FindAll(e => e.rarity == EnemyRarity.Common);
        var rares = globalEnemyList.FindAll(e => e.rarity == EnemyRarity.Rare);
        var epics = globalEnemyList.FindAll(e => e.rarity == EnemyRarity.Epic);

        if (commons.Count >= 2)
        {
            commons = ShuffleList(commons);     //Uso la funzione Shuffle per evitare controlli ulteriori
            pool.Add(commons[0]);
            pool.Add(commons[1]);
        }

        if (rares.Count > 0)
            pool.Add(rares[Random.Range(0, rares.Count)]);

        if (epics.Count > 0)
            pool.Add(epics[Random.Range(0, epics.Count)]);

        return pool;
    }

    private List<T> ShuffleList<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            T temp = list[i];
            int randomIndex = Random.Range(i, list.Count);
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
        return list;
    }
    #endregion
    #region Rooms Management

    //Ricevo una notifica di stanza terminata e controllo se è l'ultimo round 
    public void NotifyRoomCleared(RoomManager room)
    {
        if (!clearedRooms.Contains(room))
            clearedRooms.Add(room);

        if (clearedRooms.Count == roomsLoaded.Count)
        {
            foreach (AbilityUI a in abilityUItoSpawn)
                a.DisableAbilityCd();

            ui.confirmStatsPopupUI.SetActive(true);
            foreach (var r in roomsLoaded)
                r.ShowUpgradeUI();

            ui.confirmStatsButton.onClick.RemoveAllListeners();
            ui.confirmStatsButton.onClick.AddListener(() =>
            {
                if (currentRound == maxRounds)
                    ShowEndLevelPopUp();

                else
                    ShowRoundPopup();
            });
        }
    }

    private void SetActivePlayers()
    {
        foreach (RoomManager room in roomsLoaded)
            if (room.GetPlayer() != null)
                activePlayers.Add(room.GetPlayer());
            else
                Debug.LogWarning($"Stanza {room} non ha un player valido.");

    }

    public List<PlayerBase> GetActivePlayers()
    {
        return activePlayers;
    }

    private void ResetRoomsStats()
    {
        currentRound = 0;
        choosenPG = 0;
        roomsLoaded = new List<RoomManager>();
        clearedRooms = new HashSet<RoomManager>();
        activePlayers = new List<PlayerBase>();
        abilityUItoSpawn = new List<AbilityUI>();
    }

    //Metodo per spawnare le stanze all'inizio del livello; in base al numero di stanze da spawnare
    //cambia la loro posizione a schermo e il loro scaling
    private void SpawnRooms()
    {
        float scaleFactor;
        scaleFactor = Level switch
        {
            1 => 1f,
            2 => 0.8f,
            _ => 0.6f,
        };
        int indexLimit = Level < 3 ? 4 : 6;

        for (int i = 0; i < Level; i++)
        {
            int attempts = 0;
            const int maxAttempts = 10;

            while (attempts < maxAttempts)
            {
                int index = Random.Range(0, indexLimit);
                Vector3 pos = GetRoomPosition(roomsLoaded.Count, Level);
                GameObject roomGO = Instantiate(roomsPrefab[index], pos, Quaternion.identity);
                roomGO.transform.localScale = Vector3.one * scaleFactor;
                RoomManager room = roomGO.GetComponent<RoomManager>();

                bool alreadyUsed = roomsLoaded.Exists(r => r.gameObject.name.Contains(roomsPrefab[index].name));
                if (alreadyUsed)
                {
                    Destroy(roomGO);
                    attempts++;
                    continue;
                }

                roomsLoaded.Add(room);
                break;
            }
        }
    }

    //Definisco la posizione della stanza
    Vector3 GetRoomPosition(int index, int total)
    {
        switch (total)
        {
            case 1:
                return new Vector3(0f, -2.5f, 0f);

            case 2:
                return (index == 0) ? new Vector3(-6.8f, -2.5f, 0f) : new Vector3(6.8f, -2.5f, 0f);

            case 3:
                if (index == 0) return new Vector3(-6.2f, 3f, 0f);
                if (index == 1) return new Vector3(6.3f, 3f, 0f);
                return new Vector3(0f, -6.5f, 0f);

            case 4:
                if (index == 0) return new Vector3(-6.2f, 3f, 0f);
                if (index == 1) return new Vector3(6.3f, 3f, 0f);
                if (index == 2) return new Vector3(-6.2f, -6.5f, 0f);
                return new Vector3(6.3f, -6.5f, 0f);

            default:
                return Vector3.zero;
        }
    }
    #endregion
    #region UI Management
    private void ShowRoundPopup()
    {
        ui.confirmStatsPopupUI.SetActive(false);
        foreach (var r in roomsLoaded)
        {
            r.DestroyUI();
            r.GetEnemySpawner().DisableEnemySpawnStatsUI();
            if (currentRound == 0)
                r.SpawnPlayer();
        }

        if (currentRound == 0)
        {
            Destroy(ui.showEnemySpawnStatsButton.gameObject);
            Destroy(ui.confirmInitialChoicesButton.gameObject);
            SetActivePlayers();
            Invoke("SpawnAbilityUI", 0.2f);
            StartCoroutine(LoadPlayerStatsWihtDelay());
        }

        ui.roundUIText.text = "Round " + (currentRound + 1);
        ui.roundPopupUI.SetActive(true);
        ui.startButton.onClick.RemoveAllListeners();
        ui.startButton.onClick.AddListener(StartNextRound);
    }


    private void ShowEndLevelPopUp()
    {
        ui.endLevelPopupUI.SetActive(true);
        ui.nextLevelButton.onClick.RemoveAllListeners();
        ui.nextLevelButton.onClick.AddListener(() =>
        {
            if (Level < 4) StartNewLevel();
            else StartCoroutine(LoadEndCreditsRoutine());
        });
    }

    private void ShowEnemySpawnStatsUI()
    {
        foreach (RoomManager room in roomsLoaded)
            room.GetEnemySpawner().ShowEnemySpawnStatsUI();
    }

    //Metodo per lo spawn delle icone delle abilità dei PG attivi, anche qui le posizioni cambiano in base al numero
    private void SpawnAbilityUI()
    {
        if (abilityIcons == null || abilityIcons.Length == 0)
        {
            Debug.LogError("abilityIcons non inizializzato o vuoto.");
            return;
        }

        foreach (PlayerBase player in activePlayers)
        {
            int prefabIndex = GetAbilityPrefabIndex(player);
            if (prefabIndex < 0 || prefabIndex >= abilityIcons.Length || abilityIcons[prefabIndex] == null)
            {
                Debug.LogError($"Prefab mancante per {player.GetType().Name}. Controlla abilityIcons[] in Inspector.");
                continue;
            }

            Vector3 pos = GetAbilityUIPosition(abilityUItoSpawn.Count, Level);
            GameObject uiGO = Instantiate(abilityIcons[prefabIndex], pos, Quaternion.identity);

            AbilityUI tempUi = uiGO.GetComponent<AbilityUI>();
            if (ui != null)
            {
                tempUi.Initialize(player);
                abilityUItoSpawn.Add(tempUi);
                player.SetAbilityUI(tempUi);
            }

            else
                Debug.LogError("AbilityUI non trovato nel prefab instanziato.");
        }

    }

    private int GetAbilityPrefabIndex(PlayerBase player)
    {
        return player switch
        {
            FireWizard => 0,
            Kitsune => 1,
            Knight => 2,
            Satyr => 3,
            SamuraiArcher => 4,
            SamuraiCommandant => 5,
            _ => -1,
        };
    }

    //Metodo per definire la posizione dove spawnare le UI delle abilità
    Vector3 GetAbilityUIPosition(int index, int total)
    {
        switch (total)
        {
            case 1:
                return new Vector3(-16f, 0f, 0f);

            case 2:
                return (index == 0) ? new Vector3(-16f, 2.5f, 0f) : new Vector3(-16f, -2.5f, 0f);

            case 3:
                if (index == 0) return new Vector3(-16f, 4.5f, 0f);
                if (index == 1) return new Vector3(-16f, 0f, 0f);
                return new Vector3(-16f, -4.5f, 0f);

            case 4:
                if (index == 0) return new Vector3(-16f, 6f, 0f);
                if (index == 1) return new Vector3(-16f, 2f, 0f);
                if (index == 2) return new Vector3(-16f, -2f, 0f);
                return new Vector3(-16f, -6f, 0f);

            default:
                return Vector3.zero;
        }
    }

    //Metodo per il controllo del numero di PG scelti prima di iniziare il round
    public void AddPlayer()
    {
        choosenPG++;
        if (choosenPG == Level)
            ui.confirmInitialChoicesButton.onClick.AddListener(() => ShowRoundPopup());
        else
            ui.confirmInitialChoicesButton.onClick.RemoveAllListeners();
    }

    //Metodo utile per aggiornare le UI di tutte le stanze per quanto riguarda la selezione del PG
    public void NotifyPGSelected(string name, string previousPG)
    {
        foreach (RoomManager room in roomsLoaded)
            room.PlayerHasBeenSelected(name, previousPG);
    }
    #endregion

    #region Listeners Setters

    private System.Collections.IEnumerator SetTitleListeners()
    {
        yield return new WaitForSeconds(0.3f);
        titleScreenUI.newGameButton.onClick.RemoveAllListeners();
        titleScreenUI.exitButton.onClick.RemoveAllListeners();
        titleScreenUI.exitButton.onClick.AddListener(() => Application.Quit());
        titleScreenUI.optionsButton.onClick.RemoveAllListeners();
        titleScreenUI.optionsButton.onClick.AddListener(() => titleScreenUI.optionsUI.SetActive(true));
        titleScreenUI.exitOptionsButton.onClick.RemoveAllListeners();
        titleScreenUI.exitOptionsButton.onClick.AddListener(() => titleScreenUI.optionsUI.SetActive(false));
        titleScreenUI.newGameButton.onClick.AddListener(() => StartCoroutine(StartTutorialRoutine()));
        if (SaveManager.HasSave())
        {
            SaveManager.LoadCurrencyAndLevel();
            titleScreenUI.continueButton.onClick.RemoveAllListeners();
            titleScreenUI.continueButton.onClick.AddListener(() =>
            {
                titleScreenUI.gameInfoUI.SetActive(true);
                titleScreenUI.gameInfoText.text = $"Level {Level}\nGold = {CurrencyManager.Instance.GetCurrency()}";
                titleScreenUI.loadGameButton.onClick.RemoveAllListeners();
                titleScreenUI.loadGameButton.onClick.AddListener(() =>
                {
                    Debug.Log("Carico il salvataggio...");
                    StartCoroutine(ReloadLevelRoutine());
                });
            });
        }
        else
            titleScreenUI.continueText.color = new Color(1f, 1f, 1f, 0.3f);
    }

    private System.Collections.IEnumerator SetTutorialListeners()
    {
        yield return new WaitForSeconds(0.3f);
        tutorialUI.nextButton.onClick.RemoveAllListeners();
        tutorialUI.nextButton.onClick.AddListener(() =>
        {
            tutorialUI.slides[tutorialSlidesIndex].SetActive(false);
            tutorialUI.previousButton.gameObject.SetActive(true);
            tutorialSlidesIndex++;
            tutorialUI.slides[tutorialSlidesIndex].SetActive(true);
            if (tutorialSlidesIndex == tutorialUI.slides.Length - 1)
            {
                tutorialUI.startGameButton.gameObject.SetActive(true);
                tutorialUI.nextButton.gameObject.SetActive(false);
            }
        });
        tutorialUI.previousButton.onClick.RemoveAllListeners();
        tutorialUI.previousButton.onClick.AddListener(() =>
        {
            tutorialUI.nextButton.gameObject.SetActive(true);
            tutorialUI.slides[tutorialSlidesIndex].SetActive(false);
            tutorialSlidesIndex--;
            if (tutorialSlidesIndex >= 0)
            {
                tutorialUI.slides[tutorialSlidesIndex].SetActive(true);
                if (tutorialSlidesIndex == 0)
                    tutorialUI.previousButton.gameObject.SetActive(false);
            }
            else
            {
                tutorialUI.previousButton.gameObject.SetActive(false);
                tutorialSlidesIndex = 0;
            }
        });
        tutorialUI.startGameButton.onClick.RemoveAllListeners();
        tutorialUI.startGameButton.onClick.AddListener(() =>
        {
            Debug.Log("Inizio una nuova partita...");
            SaveManager.ClearSave();
            ResetRoomsStats();
            Level = 0;
            CurrencyManager.Instance.ResetCurrency();
            StartCoroutine(LoadNextLevelRoutine());
        });
    }

    private System.Collections.IEnumerator SetLevelListeners()
    {
        yield return null;

        if (ui.showEnemySpawnStatsButton != null)
        {
            ui.showEnemySpawnStatsButton.onClick.AddListener(() => ShowEnemySpawnStatsUI());
        }
        else
        {
            Debug.LogError("showEnemySpawnStatsButton è nullo! Controlla che sia assegnato in LevelUIList.");
        }
        ui.pauseButton.onClick.RemoveAllListeners();
        ui.pauseButton.onClick.AddListener(() =>
        {
            IsPaused = true;
            blur.SetActive(true);
            ui.pauseUI.SetActive(true);
        });
        ui.resumeButton.onClick.RemoveAllListeners();
        ui.resumeButton.onClick.AddListener(() =>
        {
            IsPaused = false;
            blur.SetActive(false);
            ui.pauseUI.SetActive(false);
        });
        ui.restartButton.onClick.RemoveAllListeners();
        ui.restartButton.onClick.AddListener(() =>
        {
            IsPaused = false;
            blur.SetActive(false);
            ui.pauseUI.SetActive(false);
            RestartLevel();
        });
        ui.optionsButton.onClick.RemoveAllListeners();
        ui.optionsButton.onClick.AddListener(() =>
        {
            ui.optionsUI.SetActive(true);
            ui.pauseUI.SetActive(false);
            ui.exitOptionsButton.onClick.RemoveAllListeners();
            ui.exitOptionsButton.onClick.AddListener(() =>
            {
                ui.optionsUI.SetActive(false);
                ui.pauseUI.SetActive(true);
            });
        });
        ui.exitButton.onClick.RemoveAllListeners();
        ui.exitButton.onClick.AddListener(() => Application.Quit());
    }
    #endregion
    #region UI Setters
    public void SetLevelUI(LevelUIList u)
    {
        ui = u;
    }

    public void SetTitleScreenUI(TitleScreenUI u)
    {
        titleScreenUI = u;
    }

    public void SetTutorialUI(TutorialUI u)
    {
        tutorialUI = u;
    }

    public void SetBlur(GameObject blurEffect)
    {
        blur = blurEffect;
    }
    #endregion
    #region SaveLoad
    public void SaveLevel(ref LevelData levelData)
    {
        levelData.levelData = Level;
    }

    public void LoadLevel(LevelData levelData)
    {
        Level = levelData.levelData;
    }

    private System.Collections.IEnumerator LoadNextLevelRoutine()
    {
        Level++;
        ResetRoomsStats();
        if (Level > 1)
            SaveManager.Save();

        string nextLevelName = "Level " + Level;

        AsyncOperation unloadOp = SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene());
        yield return unloadOp;

        Debug.Log($"Caricamento del livello: {nextLevelName}");
        AsyncOperation loadOp = SceneManager.LoadSceneAsync(nextLevelName, LoadSceneMode.Additive);
        yield return loadOp;
    }

    private System.Collections.IEnumerator ReloadLevelRoutine()
    {
        ResetRoomsStats();
        AsyncOperation unloadOp = SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene());
        yield return unloadOp;

        string levelName = "Level " + Level;
        Debug.Log($"Ricaricamento del livello: {levelName}");
        AsyncOperation loadOp = SceneManager.LoadSceneAsync(levelName, LoadSceneMode.Additive);
        yield return loadOp;
        IsGameOver = false;
        SaveManager.LoadCurrencyAndLevel();
    }

    private System.Collections.IEnumerator LoadPlayerStatsWihtDelay()
    {
        yield return new WaitForSeconds(0.3f);

        SaveManager.LoadPlayerStats();
    }
    #endregion
}

[System.Serializable]
public struct LevelData
{
    public int levelData;
}