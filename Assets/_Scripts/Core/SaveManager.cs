using UnityEngine;
using System.IO;
using System.Collections.Generic;

namespace Core
{
    [System.Serializable]
    public class GameState
    {
        public int Level;
        public int Rows;
        public int Cols;
        public int Score;
        public List<int> CardIds;
        public List<bool> Matched;
    }

    public static class SaveManager
    {
        private static string Path => Application.persistentDataPath + "/savegame.json";

        public static void SaveProgress(int level)
        {
            PlayerPrefs.SetInt("Unlocked", level);
            PlayerPrefs.Save();
        }

        public static int LoadProgress() => PlayerPrefs.GetInt("Unlocked", 1);

        public static void SaveFullState(GameState state)
        {
            File.WriteAllText(Path, JsonUtility.ToJson(state));
        }

        public static GameState LoadFullState()
        {
            if (!File.Exists(Path)) return null;
            return JsonUtility.FromJson<GameState>(File.ReadAllText(Path));
        }

        public static void ClearFullState()
        {
            if (File.Exists(Path)) File.Delete(Path);
        }
    }
}
