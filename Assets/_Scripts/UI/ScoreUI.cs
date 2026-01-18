using UnityEngine;
using TMPro; 

namespace UI
{
    public class ScoreUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private TextMeshProUGUI _scoreText; 
        [SerializeField] private string _prefix = "Score: ";

        private void Start()
        {
            if (Core.ScoreManager.Instance != null)
            {
                Core.ScoreManager.Instance.OnScoreChanged += UpdateScoreText;
                // Initialize display
                UpdateScoreText(Core.ScoreManager.Instance.CurrentScore);
            }
        }

        private void OnDestroy()
        {
            if (Core.ScoreManager.Instance != null)
            {
                Core.ScoreManager.Instance.OnScoreChanged -= UpdateScoreText;
            }
        }

        private void UpdateScoreText(int newScore)
        {
             if (_scoreText != null)
             {
                 _scoreText.text = $"{_prefix}{newScore}";
             }
        }
    }
}
