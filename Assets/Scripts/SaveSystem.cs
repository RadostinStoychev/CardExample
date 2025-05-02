using UnityEngine;
using System.IO;
using System;

public class SaveSystem : MonoBehaviour
{
    private const string SAVE_FILENAME = "cardsave.json";
    private const string PREFS_KEY_HAS_SAVE = "HasCardGameSave";

    public static void SaveGame(GameData gameData)
    {
        string json = JsonUtility.ToJson(gameData);
        
        string path = Path.Combine(Application.persistentDataPath, SAVE_FILENAME);
        
        try
        {
            File.WriteAllText(path, json);
            PlayerPrefs.SetInt(PREFS_KEY_HAS_SAVE, 1);
            PlayerPrefs.Save();
            Debug.Log($"Game saved to {path}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to save game: {e.Message}");
        }
    }
    
    public static GameData LoadGame()
    {
        string path = Path.Combine(Application.persistentDataPath, SAVE_FILENAME);
        
        if (File.Exists(path))
        {
            try
            {
                string json = File.ReadAllText(path);
                return JsonUtility.FromJson<GameData>(json);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to load game: {e.Message}");
                return null;
            }
        }
        else
        {
            Debug.LogWarning("Save file not found");
            return null;
        }
    }
    
    public static bool HasSavedGame()
    {
        return PlayerPrefs.GetInt(PREFS_KEY_HAS_SAVE, 0) == 1;
    }
    
    public static void ClearSave()
    {
        string path = Path.Combine(Application.persistentDataPath, SAVE_FILENAME);
        
        if (File.Exists(path))
        {
            try
            {
                File.Delete(path);
                PlayerPrefs.DeleteKey(PREFS_KEY_HAS_SAVE);
                PlayerPrefs.Save();
                Debug.Log("Save file deleted");
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to delete save file: {e.Message}");
            }
        }
    }
}

[Serializable]
public class GameData
{
    public int GridWidth;
    public int GridHeight;
    public int RemainingPairs;
    public int TotalMoves;
    public int Score;
    public CardState[] CardStates;
    
    // Value-to-sprite mapping record
    public ValueToSpriteMapping[] SpriteMapping;
}

[Serializable]
public class ValueToSpriteMapping
{
    public int Value;
    public int SpriteIndex;
}
