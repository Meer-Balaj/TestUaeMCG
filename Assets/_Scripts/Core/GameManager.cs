using UnityEngine;

namespace Core
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        private System.Collections.Generic.Queue<Gameplay.CardController> _flippedCards = new System.Collections.Generic.Queue<Gameplay.CardController>();
        private bool _isCheckingMatch = false; // Flag to know if a check is running (though we want continuous, we need to process the queue)

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
        
        private int _totalPairs;
        private int _matchesFound;

        private void Start()
        {
            _gridManager = FindObjectOfType<Gameplay.GridManager>();
            if (_gridManager == null) Debug.LogError("GridManager not found!");
            
            if (ScoreManager.Instance == null)
            {
                 var sm = gameObject.AddComponent<ScoreManager>();
            }

            // Calculate totals
            _totalPairs = (_gridManager.Rows * _gridManager.Cols) / 2;

            // Attempt to Load
            var data = SaveManager.LoadGame();
            if (data != null)
            {
                Debug.Log("Game Loaded!");
                ScoreManager.Instance.InitializeScore(data.Score);
                _gridManager.RestoreLayout(data);
                
                // Recalculate matches found based on loaded data
                _matchesFound = 0;
                foreach (bool isMatched in data.CardMatchedStates)
                {
                    if (isMatched) _matchesFound++;
                }
                _matchesFound /= 2; // Since state stores individual cards
            }
            else
            {
                StartNewGame();
            }
        }

        private void StartNewGame()
        {
            Debug.Log("New Game Started");
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
            StartNewGame();
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
                    
                    ScoreManager.Instance.OnMatch();
                    _matchesFound++;
                    Debug.Log($"Matched! Score: {ScoreManager.Instance.CurrentScore}");
                    
                    if (SoundManager.Instance != null) SoundManager.Instance.PlayMatch();

                    if (_matchesFound >= _totalPairs)
                    {
                        Debug.Log("Level Complete!");
                        if (SoundManager.Instance != null) SoundManager.Instance.PlayGameWin();
                        
                        SaveManager.ClearSave(); 
                        yield return new WaitForSeconds(1.0f);
                        RestartGame();
                    }
                    else
                    {
                        SaveCurrentGame();
                    }
                }
                else
                {
                    ScoreManager.Instance.OnMismatch();
                    if (SoundManager.Instance != null) SoundManager.Instance.PlayMismatch();
                    card1.FlipClose();
                    card2.FlipClose();
                }
            }

            _isCheckingMatch = false;
        }

        public void SaveCurrentGame()
        {
            if (_gridManager == null) return;

            _gridManager.GetCurrentState(out var ids, out var matches);
            SaveManager.SaveGame(ScoreManager.Instance.CurrentScore, _gridManager.Rows, _gridManager.Cols, ids, matches);
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
