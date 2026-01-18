using UnityEngine;
using System;

namespace Core
{
    public class ScoreManager : MonoBehaviour
    {
        public static ScoreManager Instance { get; private set; }

        [Header("Score Config")]
        [SerializeField] private int _scorePerMatch = 10;
        [SerializeField] private int _scorePerMismatch = -1; // Use negative for penalty
        [SerializeField] private bool _enableMismatchPenalty = true;
        
        [Header("Combo Config")]
        [SerializeField] private bool _enableCombo = true;
        [SerializeField] private int _comboBonus = 5; // Extra points per consecutive match
        
        public int CurrentScore { get; private set; }
        public event Action<int> OnScoreChanged;

        private int _currentComboStreak = 0;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void InitializeScore(int startScore)
        {
            CurrentScore = startScore;
            _currentComboStreak = 0;
            OnScoreChanged?.Invoke(CurrentScore);
        }

        public void OnMatch()
        {
            int points = _scorePerMatch;
            
            if (_enableCombo)
            {
                // Format: Base + (Streak * Bonus)
                // Streak starts at 0. First match = Base. Next consecutive = Base + Bonus.
                if (_currentComboStreak > 0)
                {
                    points += (_currentComboStreak * _comboBonus);
                }
                _currentComboStreak++;
            }

            AddScoreInternal(points);
        }

        public void OnMismatch()
        {
            // Reset combo on mismatch
            _currentComboStreak = 0;

            if (_enableMismatchPenalty)
            {
                AddScoreInternal(_scorePerMismatch);
            }
        }

        private void AddScoreInternal(int amount)
        {
            CurrentScore += amount;
            // Prevent negative score? Optional. Let's allow negative for now.
            OnScoreChanged?.Invoke(CurrentScore);
        }

        public void ResetScore()
        {
            CurrentScore = 0;
            _currentComboStreak = 0;
            OnScoreChanged?.Invoke(CurrentScore);
        }
    }
}
