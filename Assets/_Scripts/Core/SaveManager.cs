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
        public List<int> CardLayoutIds; // The TypeID of each card by index
        public List<bool> CardMatchedStates; // Whether each card is already matched
    }

    public static class SaveManager
    {
        private static string SavePath => Path.Combine(Application.persistentDataPath, "gamedata.json");

        public static void SaveGame(int score, int rows, int cols, List<int> layoutIds, List<bool> matchedStates)
        {
            GameData data = new GameData
            {
                Score = score,
                Rows = rows,
                Cols = cols,
                CardLayoutIds = layoutIds,
                CardMatchedStates = matchedStates
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
    }
}
