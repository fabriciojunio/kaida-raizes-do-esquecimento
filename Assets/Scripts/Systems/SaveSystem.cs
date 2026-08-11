using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Save simples em JSON (PlayerPrefs para o caminho do arquivo seria outra
/// opção). Guarda checkpoint, habilidades desbloqueadas e itens coletados —
/// base para um metroidvania com progressão.
/// </summary>
[System.Serializable]
public class SaveData
{
    public float checkpointX, checkpointY;
    public string room = "";
    public List<string> unlockedAbilities = new List<string>();
    public List<string> collectedItems = new List<string>();
}

public class SaveSystem : MonoBehaviour
{
    public static SaveSystem Instance { get; private set; }
    public SaveData Data = new SaveData();

    string SavePath => Application.persistentDataPath + "/savegame.json";

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public bool HasSave() => System.IO.File.Exists(SavePath);

    public void SaveGame()
    {
        if (GameManager.Instance != null)
        {
            Data.checkpointX = GameManager.Instance.CurrentCheckpoint.x;
            Data.checkpointY = GameManager.Instance.CurrentCheckpoint.y;
            Data.room = GameManager.Instance.CurrentRoom;
        }
        System.IO.File.WriteAllText(SavePath, JsonUtility.ToJson(Data, true));
    }

    public bool LoadGame()
    {
        if (!HasSave()) return false;
        try
        {
            var loaded = JsonUtility.FromJson<SaveData>(System.IO.File.ReadAllText(SavePath));
            if (loaded == null) return false;
            Data = loaded;
            if (Data.unlockedAbilities == null) Data.unlockedAbilities = new List<string>();
            if (Data.collectedItems == null) Data.collectedItems = new List<string>();
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("Save corrompido, começando do zero: " + e.Message);
            Data = new SaveData();
            return false;
        }
        if (GameManager.Instance != null)
            GameManager.Instance.SetCheckpoint(new Vector2(Data.checkpointX, Data.checkpointY), Data.room);
        return true;
    }

    /// <summary>Apaga o progresso (usado pelo menu e pelos testes).</summary>
    public void DeleteSave()
    {
        if (HasSave()) System.IO.File.Delete(SavePath);
        Data = new SaveData();
    }

    public bool HasAbility(string id) => Data.unlockedAbilities.Contains(id);
    public void UnlockAbility(string id)
    {
        if (!Data.unlockedAbilities.Contains(id)) Data.unlockedAbilities.Add(id);
        SaveGame();
    }

    public bool IsCollected(string id) => Data.collectedItems.Contains(id);
    public void MarkCollected(string id)
    {
        if (!Data.collectedItems.Contains(id)) Data.collectedItems.Add(id);
    }
}
