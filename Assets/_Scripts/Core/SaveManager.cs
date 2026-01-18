using UnityEngine;
using System.IO;
using System.Collections.Generic;

namespace Core
{
    [System.Serializable]
    public class GameData
    {
        public int Score;
        public int Rows;
        public int Cols;
        public int LevelIndex; // Track which level this save belongs to
        public List<int> CardLayoutIds;
        public List<bool> CardMatchedStates;
        public List<bool> CardFaceUpStates; // Track if card is physically face up (matched or pending)
    }

    public static class SaveManager
    {
        private static string SavePath => Path.Combine(Application.persistentDataPath, "gamedata.json");
        private const string UnlockedLevelKey = "UnlockedLevel";

        public static void SaveGame(int score, int rows, int cols, int levelIndex, List<int> layoutIds, List<bool> matchedStates, List<bool> faceUpStates)
        {
            GameData data = new GameData
            {
                Score = score,
                Rows = rows,
                Cols = cols,
                LevelIndex = levelIndex,
                CardLayoutIds = layoutIds,
                CardMatchedStates = matchedStates,
                CardFaceUpStates = faceUpStates
            };

            string json = JsonUtility.ToJson(data);
            File.WriteAllText(SavePath, json);
            Debug.Log("Game Saved at " + SavePath);
        }

        public static GameData LoadGame()
        {
            if (!File.Exists(SavePath)) return null;

            try
            {
                string json = File.ReadAllText(SavePath);
                return JsonUtility.FromJson<GameData>(json);
            }
            catch (System.Exception e)
            {
                Debug.LogError("Failed to load save file: " + e.Message);
                return null;
            }
        }

        public static void ClearSave()
        {
            if (File.Exists(SavePath)) File.Delete(SavePath);
        }

        public static bool HasSave()
        {
            return File.Exists(SavePath);
        }

        public static int GetUnlockedLevel()
        {
            return PlayerPrefs.GetInt(UnlockedLevelKey, 1);
        }

        public static void SetUnlockedLevel(int level)
        {
            PlayerPrefs.SetInt(UnlockedLevelKey, level);
            PlayerPrefs.Save();
        }
    }
}
