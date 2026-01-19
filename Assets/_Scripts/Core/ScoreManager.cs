using UnityEngine;
using System;

namespace Core
{
    public class ScoreManager : MonoBehaviour
    {
        public static ScoreManager Instance { get; private set; }
        public int CurrentScore { get; private set; }
        public event Action<int> OnScoreChanged;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        public void AddScore(int amount)
        {
            CurrentScore += amount;
            OnScoreChanged?.Invoke(CurrentScore);
        }

        public void ResetScore(int startScore = 0)
        {
            CurrentScore = startScore;
            OnScoreChanged?.Invoke(CurrentScore);
        }
    }
}
