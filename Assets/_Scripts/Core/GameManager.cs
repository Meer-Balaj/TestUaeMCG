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

        private void Start()
        {
            Debug.Log("GameManager Initialized");
        }

        public void OnCardClicked(Gameplay.CardController card)
        {
            if (_flippedCards.Contains(card) || card.IsMatched) return;

            card.FlipOpen();
            _flippedCards.Enqueue(card);

            // We need 2 cards to check. 
            // If we have >= 2, we can start checking, but wait...
            // "Continuous card flipping" implies we don't wait for the USER.
            // But we must process them in pairs.
            // If the user flips A, B, C, D (quickly).
            // A & B enters queue. C & D enters queue.
            // We should process A & B. Then C & D.
            
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

                // Wait a moment so the user sees the second card flip
                // We shouldn't block the USER from flipping more, but this coroutine handles the comparison logic logic time.
                // The flip animation takes 0.2s. Let's wait at least that.
                yield return new WaitForSeconds(0.5f);

                if (card1.CardTypeId == card2.CardTypeId)
                {
                    // Match!
                    card1.SetMatched();
                    card2.SetMatched();
                    Debug.Log($"Matched Type {card1.CardTypeId}!");
                    // TODO: Play Sound & Score
                }
                else
                {
                    // No Match
                    card1.FlipClose();
                    card2.FlipClose();
                    // TODO: Play Sound
                }
            }

            _isCheckingMatch = false;
        }
    }
}
