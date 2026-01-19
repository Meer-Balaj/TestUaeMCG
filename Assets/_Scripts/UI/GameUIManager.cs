using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class GameUIManager : MonoBehaviour
    {
        [Header("Panels")]
        [SerializeField] private GameObject _winPanel;
        [SerializeField] private GameObject _losePanel;

        [Header("Floating Text")]
        [SerializeField] private FloatingText _floatingTextPrefab;
        [SerializeField] private Transform _floatingTextContainer; // Assign the Canvas or a Panel inside Canvas

        [Header("Buttons")]
        [SerializeField] private Button _winNextLevelButton;
        [SerializeField] private Button _winHomeButton;
        [SerializeField] private Button _loseRetryButton;
        [SerializeField] private Button _loseHomeButton;

        private void Start()
        {
            if (_winPanel) _winPanel.SetActive(false);
            if (_losePanel) _losePanel.SetActive(false);

            if (_winNextLevelButton) _winNextLevelButton.onClick.AddListener(OnNextLevelClicked);
            if (_winHomeButton) _winHomeButton.onClick.AddListener(OnHomeClicked);
            if (_loseRetryButton) _loseRetryButton.onClick.AddListener(OnRetryClicked);
            if (_loseHomeButton) _loseHomeButton.onClick.AddListener(OnHomeClicked);
        }

        public void ShowWinScreen()
        {
            if (_winPanel) _winPanel.SetActive(true);
            if (Core.SoundManager.Instance != null) Core.SoundManager.Instance.PlayGameWin();
        }

        public void ShowLoseScreen()
        {
            if (_losePanel) _losePanel.SetActive(true);
            if (Core.SoundManager.Instance != null) Core.SoundManager.Instance.PlayGameFail();
        }

        public void SpawnFloatingText(Vector3 worldPos, int scoreChange)
        {
             if (_floatingTextPrefab == null) return;
             
             // Ensure we have a container
             Transform container = _floatingTextContainer != null ? _floatingTextContainer : transform;

             FloatingText textInstance = Instantiate(_floatingTextPrefab, container);
             
             // Offset upward slightly from the card click position
             Vector3 spawnPos = worldPos + new Vector3(0, 50f, 0); 
             textInstance.transform.position = spawnPos;

             string prefix = scoreChange >= 0 ? "+" : "";
             Color color = scoreChange >= 0 ? Color.green : Color.red;
             
             textInstance.Initialize($"{prefix}{scoreChange}", color);
        }

        public void HideAllPanels()
        {
            if (_winPanel) _winPanel.SetActive(false);
            if (_losePanel) _losePanel.SetActive(false);
        }

        private void OnNextLevelClicked()
        {
             HideAllPanels();
             if (Core.GameManager.Instance != null)
             {
                 Core.GameManager.Instance.LoadNextLevel();
             }
        }

        private void OnRetryClicked()
        {
             HideAllPanels();
             if (Core.GameManager.Instance != null)
             {
                 Core.GameManager.Instance.RestartGame();
             }
        }

        private void OnHomeClicked()
        {
             HideAllPanels();
             if (Core.GameManager.Instance != null)
             {
                 Core.GameManager.Instance.ReturnToMainMenu();
             }
        }
    }
}
