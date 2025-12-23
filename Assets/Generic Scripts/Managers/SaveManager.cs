using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.IO;

//Script che si occupa di gestire il salvataggio e il caricamento dei dati di gioco
//Contiene le strutture dati per salvare i personaggi, la valuta e il livello
public class SaveManager
{
    private static SaveData saveData = new();

    [System.Serializable]
    public struct SaveData
    {
        public List<PlayerSaveData> allCharacters;
        public CurrencySaveData currencyData;
        public LevelData levelData;
    }
    #region SaveData
    public static string SaveFileName()
    {
        string saveFile = Application.persistentDataPath + "/save" + ".save";
        Debug.Log("Save file path: " + saveFile);
        return saveFile;
    }

    public static void Save()
    {
        HandleSaveData();

        File.WriteAllText(SaveFileName(), JsonUtility.ToJson(saveData, true));
    }

    //Il salvataggio dei dati avviene in maniera unica per tutte le variabili e avviene 
    //alla fine di ogni livello
    private static void HandleSaveData()
    {
        if (saveData.allCharacters == null)
            saveData.allCharacters = new List<PlayerSaveData>();

        else
            saveData.allCharacters.Clear();

        PlayerBase[] players = GameObject.FindObjectsOfType<PlayerBase>();
        foreach (var player in players)
        {
            PlayerSaveData data = new PlayerSaveData();
            player.Save(ref data);
            saveData.allCharacters.Add(data);
        }

        CurrencyManager.Instance.Save(ref saveData.currencyData);
        GameManager.Instance.SaveLevel(ref saveData.levelData);
    }

    public static void ClearSave()
    {
        saveData = new SaveData();
        if (HasSave())
        {
            File.Delete(SaveFileName());
            Debug.Log("Salvataggio cancellato.");
        }
        else
        {
            Debug.LogWarning("Nessun file di salvataggio da cancellare.");
        }
    }

    public static bool HasSave()
    {
        return File.Exists(SaveFileName());
    }
    #endregion
    #region LoadData
    //Il caricamento dei dati viene separato in caricamento dei personaggi e caricamento della valuta e del livello
    //in quanto i personaggi vengono caricati soltanto quando vengono selezionati,
    //mentre la valuta e il livello vengono caricati all'inizio del livello
    public static void LoadPlayerStats()
    {
        if (!HasSave())
        {
            Debug.LogWarning("Nessun file di salvataggio trovato.");
            return;
        }
        string saveContent = File.ReadAllText(SaveFileName());

        saveData = JsonUtility.FromJson<SaveData>(saveContent);
        HandleLoadPlayerData();
    }

    public static void LoadCurrencyAndLevel()
    {
        if (!HasSave())
        {
            CurrencyManager.Instance.ResetCurrency();
            return;
        }
        string saveContent = File.ReadAllText(SaveFileName());

        saveData = JsonUtility.FromJson<SaveData>(saveContent);
        HandleLoadCurrencyAndLevelData();
    }

    private static void HandleLoadPlayerData()
    {
        PlayerBase[] players = GameObject.FindObjectsOfType<PlayerBase>();
        foreach (var saved in saveData.allCharacters)
        {
            foreach (var player in players)
            {
                if (player.characterId == saved.characterId)
                {
                    player.Load(saved);
                    break;
                }
            }
        }
    }

    private static void HandleLoadCurrencyAndLevelData()
    {
        CurrencyManager.Instance.Load(saveData.currencyData);
        GameManager.Instance.LoadLevel(saveData.levelData);
    }
    #endregion
}
