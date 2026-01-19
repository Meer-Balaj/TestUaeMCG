using UnityEngine;

namespace Core
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        private System.Collections.Generic.Queue<Gameplay.CardController> _flippedCards = new System.Collections.Generic.Queue<Gameplay.CardController>();
        private bool _isCheckingMatch = false; 

        private int _currentLevelIndex = 1;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private Gameplay.GridManager _gridManager;
        private UI.GameUIManager _uiManager;
        
        private int _totalPairs;
        private int _matchesFound;

        private void EnsureDependencies()
        {
            if (_gridManager == null) _gridManager = FindObjectOfType<Gameplay.GridManager>(true);
            if (_uiManager == null) _uiManager = FindObjectOfType<UI.GameUIManager>(true);
        }
        
        [SerializeField] private int _failScoreThreshold = -10;

        private void Start()
        {
            EnsureDependencies();
            
            if (ScoreManager.Instance == null)
            {
                 var sm = gameObject.AddComponent<ScoreManager>();
            }

            // Check if we are in Main Menu mode
            var menu = FindObjectOfType<UI.MainMenuController>();
            if (menu != null)
            {
                // We are in Menu, don't auto-start or auto-resume here.
                // MainMenuController will call Resume/Start specifically.
                return;
            }

            // Attempt to auto-resume ONLY if not in menu (debug/direct scene play)
            var data = SaveManager.LoadGame();
            if (data != null)
            {
                ResumeGame(data);
            }
            else
            {
                StartNewGame(1); 
            }
        }

        public void StartGameWithLayout(int levelIndex, int rows, int cols)
        {
            EnsureDependencies();
            if (_gridManager != null)
            {
                _gridManager.SetDimensions(rows, cols);
                StartNewGame(levelIndex);
            }
            else
            {
                Debug.LogError("Cannot StartGameWithLayout: GridManager not found!");
            }
        }

        public void ResumeGame(GameData data)
        {
            EnsureDependencies();
            Debug.Log($"Resuming Level {data.LevelIndex}");
            _currentLevelIndex = data.LevelIndex;
            
            if (_gridManager != null)
                _gridManager.SetDimensions(data.Rows, data.Cols);
            
            _totalPairs = (data.Rows * data.Cols) / 2;

            if (ScoreManager.Instance != null)
                ScoreManager.Instance.InitializeScore(data.Score);
            
            // Recalculate matches first
            _matchesFound = 0;
            if (data.CardMatchedStates != null)
            {
                foreach (bool isMatched in data.CardMatchedStates)
                {
                    if (isMatched) _matchesFound++;
                }
                _matchesFound /= 2;
            }

            // Restore Layout and get pending cards
            var pendingCards = _gridManager.RestoreLayout(data);
            
            // Check if game was already finished (stale save)
            if (_matchesFound >= _totalPairs)
            {
                Debug.Log("Save file indicates level already finished. Restarting level.");
                SaveManager.ClearSave();
                StartNewGame(_currentLevelIndex);
                return;
            }

            // Restore pending flipped queue
            _flippedCards.Clear();
            foreach (var card in pendingCards)
            {
                _flippedCards.Enqueue(card);
            }

            // If we have >= 2 cards pending, match check should trigger?
            // Usually valid game state only has 0 or 1 pending card (waiting for second), 
            // or 2 cards processing (user quit mid-animation).
            // If 2 cards, we should trigger process.
            if (_flippedCards.Count >= 2 && !_isCheckingMatch)
            {
                StartCoroutine(ProcessMatches());
            }
        }

        private void StartNewGame(int levelIndex)
        {
            Debug.Log($"New Game Started: Level {levelIndex}");
            if (_uiManager != null) _uiManager.HideAllPanels();
            
            _currentLevelIndex = levelIndex;
            _isCheckingMatch = false; // Reset checking flag to allow new matches
            ScoreManager.Instance.ResetScore();
            _matchesFound = 0;
            _flippedCards.Clear();
            _gridManager.GenerateLayout();
            
            // Recalculate totals in case settings changed
            _totalPairs = (_gridManager.Rows * _gridManager.Cols) / 2;
            
            SaveManager.ClearSave(); 
        }

        public void RestartGame()
        {
            StartNewGame(_currentLevelIndex);
        }

        public void StopGame()
        {
            if (_uiManager != null) _uiManager.HideAllPanels();
            if (_gridManager != null)
            {
                _gridManager.ClearGrid();
            }
        }

        public void OnCardClicked(Gameplay.CardController card)
        {
            if (_flippedCards.Contains(card) || card.IsMatched) return;

            card.FlipOpen();
            _flippedCards.Enqueue(card);

            if (!_isCheckingMatch && _flippedCards.Count >= 2)
            {
                StartCoroutine(ProcessMatches());
            }
        }

        private System.Collections.IEnumerator ProcessMatches()
        {
            _isCheckingMatch = true;

            while (_flippedCards.Count >= 2)
            {
                var card1 = _flippedCards.Dequeue();
                var card2 = _flippedCards.Dequeue();

                yield return new WaitForSeconds(0.5f);

                if (card1.CardTypeId == card2.CardTypeId)
                {
                    card1.SetMatched();
                    card2.SetMatched();
                    
                    int points = ScoreManager.Instance.OnMatch();
                    _matchesFound++;
                    Debug.Log($"Matched! Score: {ScoreManager.Instance.CurrentScore}");
                    
                    if (SoundManager.Instance != null) SoundManager.Instance.PlayMatch();
                    if (_uiManager != null) _uiManager.SpawnFloatingText(card1.transform.position, points);

                    if (_matchesFound >= _totalPairs)
                    {
                        Debug.Log("Level Complete!");
                        // Play win sound is handled by UI Manager usually, or here. Let's keep existing logic if it was here?
                        // Actually existing code had PlayGameWin here. UI Manager also has it. I will let UI Manager handle it to avoid double sound or remove it from here.
                        // Existing code: if (SoundManager.Instance != null) SoundManager.Instance.PlayGameWin();
                        // I will remove it here and let UI Manager do it, OR keep it here and remove from UI Manager.
                        // Let's keep it here for logic consistency, but UI Manager ShowWinScreen also calls it. I'll comment out here if UI Manager is present.
                        
                        // Check Unlock
                        int unlocked = SaveManager.GetUnlockedLevel();
                        if (_currentLevelIndex >= unlocked)
                        {
                             SaveManager.SetUnlockedLevel(_currentLevelIndex + 1);
                             Debug.Log($"Unlocked Level {_currentLevelIndex + 1}");
                        }

                        SaveManager.ClearSave(); 
                        
                        // Show Win Screen
                        if (_uiManager != null)
                        {
                            _uiManager.ShowWinScreen();
                        }
                        else
                        {
                            yield return new WaitForSeconds(3.0f);
                            ReturnToMainMenu();
                        }
                    }
                    else
                    {
                        SaveCurrentGame();
                    }
                }
                else
                {
                    int points = ScoreManager.Instance.OnMismatch();
                    if (SoundManager.Instance != null) SoundManager.Instance.PlayMismatch();
                    if (_uiManager != null) _uiManager.SpawnFloatingText(card1.transform.position, points);

                    card1.FlipClose();
                    card2.FlipClose();

                    // Check Fail Condition
                    if (ScoreManager.Instance.CurrentScore <= _failScoreThreshold)
                    {
                         Debug.Log("Level Failed!");
                         
                         SaveManager.ClearSave();
                         
                         if (_uiManager != null)
                         {
                             _uiManager.ShowLoseScreen();
                         }
                         else
                         {
                             yield return new WaitForSeconds(3.0f);
                             ReturnToMainMenu();
                         }
                         
                         yield break; // Stop processing
                    }
                }
            }

            _isCheckingMatch = false;
        }

        public void LoadNextLevel()
        {
            int nextLevel = _currentLevelIndex + 1;
            
            // Map dimensions for next level
            int r, c;
            switch(nextLevel)
            {
                case 1: r = 2; c = 2; break;
                case 2: r = 2; c = 3; break;
                case 3: r = 4; c = 4; break;
                case 4: r = 5; c = 6; break;
                default: r = 5; c = 6; break;
            }

            StartGameWithLayout(nextLevel, r, c);
        }

        public void ReturnToMainMenu()
        {
            var menu = FindObjectOfType<UI.MainMenuController>();
            if (menu != null)
            {
                StopGame(); // Clears grid
                // We need to tell the Menu to show itself. 
                // Since MainMenuController handles UI state, we can simulate the "Back" action or expose a dedicated method.
                // However, FindObjectOfType might return the active one but menu panels are hidden.
                // The MainMenuController logic: _mainMenuPanel.SetActive(true) is inside ShowMainMenu.
                // Let's call a method on it if possible, or trigger the button.
                
                // Better approach: Expose a helper in MainMenuController or just use the button restart logic.
                // For now, let's just use the Public method if we can access it, otherwise we might need to make it public.
                // Wait, OnBackToMenuClicked is private. Let's make a public helper on MainMenuController in a separate step or just use SendMessage?
                // No, let's keep it safe. I'll modify MainMenuController to have a Public "ShowMenu" method first.
                // But for this step, I will just call StopGame() and then assume the Menu Controller is listening or we can find it.
                
                // Let's assume we modify MainMenuController soon. For now:
                menu.ShowMainMenuFromGame();
            }
            else
            {
                 // Fallback if no menu
                 RestartGame(); 
            }
        }

        public void SaveCurrentGame()
        {
            if (_gridManager == null) 
            {
                Debug.LogWarning("SaveCurrentGame Failed: GridManager is null");
                return;
            }

            // Don't save if game is finished
            if (_matchesFound >= _totalPairs) 
            {
                 Debug.Log("SaveCurrentGame Skipped: Level Completed");
                 return;
            }

            Debug.Log("Saving Current Game State...");
            _gridManager.GetCurrentState(out var ids, out var matches, out var faceUp);
            SaveManager.SaveGame(ScoreManager.Instance.CurrentScore, _gridManager.Rows, _gridManager.Cols, _currentLevelIndex, ids, matches, faceUp);
        }

        private void OnApplicationQuit()
        {
            SaveCurrentGame();
        }

        private void OnApplicationPause(bool pause)
        {
            if (pause) SaveCurrentGame();
        }
    }
}
