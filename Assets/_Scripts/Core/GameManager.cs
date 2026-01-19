using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Gameplay;
using UI;

namespace Core
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [SerializeField] private int _failThreshold = -10;
        
        private GridManager _grid;
        private int _currentLevel;
        private int _matchesFound;
        private int _totalPairs;
        private bool _isProcessing;
        private Queue<CardController> _selectionQueue = new Queue<CardController>();

        private void Awake()
        {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = 60;
            if (Instance == null) 
            { 
                Instance = this; 
                transform.SetParent(null);
                DontDestroyOnLoad(gameObject); 
            }
            else { Destroy(gameObject); }
        }

        private void Start() => _grid = FindObjectOfType<GridManager>(true);

        public void StartLevel(int level, int rows, int cols)
        {
            if (!_grid) _grid = FindObjectOfType<GridManager>(true);
            
            _currentLevel = level;
            _matchesFound = 0;
            _totalPairs = (rows * cols) / 2;
            _isProcessing = false;
            _selectionQueue.Clear();
            
            ScoreManager.Instance.ResetScore();
            _grid.SetDimensions(rows, cols);
            
            // Wait one frame before generating to ensure UIManager transition doesn't mess with Grid layout calculations
            StartCoroutine(DelayedGenerate());
            
            SaveManager.ClearFullState();
        }

        private IEnumerator DelayedGenerate()
        {
            yield return null;
            _grid.GenerateLayout();
        }

        public void ResumeLevel()
        {
            var state = SaveManager.LoadFullState();
            if (state == null || state.CardIds == null || state.CardIds.Count == 0) return;

            if (!_grid) _grid = FindObjectOfType<GridManager>(true);

            _currentLevel = state.Level;
            _totalPairs = (state.Rows * state.Cols) / 2;
            _matchesFound = 0;
            foreach(var m in state.Matched) if(m) _matchesFound++;
            _matchesFound /= 2;

            _grid.SetDimensions(state.Rows, state.Cols);
            _grid.RestoreLayout(state.CardIds, state.Matched);
            ScoreManager.Instance.ResetScore(state.Score);

            UIManager.Instance.ShowPanel(PanelType.HUD);
        }

        public void RestartLevel() => StartLevel(_currentLevel, _grid.Rows, _grid.Cols);

        public void NextLevel()
        {
            int next = _currentLevel + 1;
            int r, c;

            switch(next)
            {
                case 1: r = 2; c = 2; break; // 2x2
                case 2: r = 2; c = 3; break; // 2x3
                case 3: r = 4; c = 4; break; // 4x4
                case 4: r = 5; c = 6; break; // 5x6
                default: 
                    r = 6; c = 6; break; // 6x6 onwards
            }
            StartLevel(next, r, c);
        }

        public void ReturnToMenu()
        {
            StopAllCoroutines();
            _isProcessing = false;
            _selectionQueue.Clear();
            SaveCurrentState();
            _grid.ClearGrid();
        }

        public void OnCardSelected(CardController card)
        {
            if (_selectionQueue.Contains(card) || card.IsMatched) return;

            card.FlipOpen();
            _selectionQueue.Enqueue(card);

            if (!_isProcessing && _selectionQueue.Count >= 2) StartCoroutine(ProcessMatchQueue());
        }

        private IEnumerator ProcessMatchQueue()
        {
            _isProcessing = true;

            while (_selectionQueue.Count >= 2)
            {
                var c1 = _selectionQueue.Dequeue();
                var c2 = _selectionQueue.Dequeue();

                yield return new WaitForSeconds(0.6f);

                // Safety check: were cards destroyed while waiting?
                if (!c1 || !c2) 
                {
                    _isProcessing = false;
                    _selectionQueue.Clear();
                    yield break;
                }

                if (c1.CardTypeId == c2.CardTypeId)
                {
                    c1.SetMatched();
                    c2.SetMatched();
                    _matchesFound++;
                    
                    ScoreManager.Instance.AddScore(10);
                    UIManager.Instance.SpawnScorePopup(c1.transform.position, 10);
                    AudioManager.Instance.PlayMatch();

                    if (_matchesFound >= _totalPairs)
                    {
                        SaveManager.SaveProgress(_currentLevel + 1);
                        SaveManager.ClearFullState();
                        UIManager.Instance.ShowPanel(PanelType.Win);
                        yield break;
                    }
                }
                else
                {
                    c1.AnimateMismatch();
                    c2.AnimateMismatch();
                    ScoreManager.Instance.AddScore(-1);
                    UIManager.Instance.SpawnScorePopup(c1.transform.position, -1);
                    AudioManager.Instance.PlayMismatch();

                    if (ScoreManager.Instance.CurrentScore <= _failThreshold)
                    {
                        SaveManager.ClearFullState();
                        UIManager.Instance.ShowPanel(PanelType.Lose);
                        yield break;
                    }
                }
                SaveCurrentState();
            }

            _isProcessing = false;
        }

        private void SaveCurrentState()
        {
            if (!_grid || _matchesFound >= _totalPairs || ScoreManager.Instance.CurrentScore <= _failThreshold) return;
            
            _grid.GetState(out var ids, out var matched);
            if (ids == null || ids.Count == 0) return;
            
            var state = new GameState
            {
                Level = _currentLevel,
                Rows = _grid.Rows,
                Cols = _grid.Cols,
                Score = ScoreManager.Instance.CurrentScore,
                CardIds = ids,
                Matched = matched
            };
            SaveManager.SaveFullState(state);
        }

        private void OnApplicationQuit() => SaveCurrentState();
        private void OnApplicationPause(bool pause) { if (pause) SaveCurrentState(); }
    }
}
