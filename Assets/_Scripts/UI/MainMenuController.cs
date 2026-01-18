using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class MainMenuController : MonoBehaviour
    {
        [Header("Panels")]
        [SerializeField] private GameObject _mainMenuPanel;
        [SerializeField] private GameObject _levelSelectionPanel;
        [SerializeField] private GameObject _gameHudPanel; // Reference to Game HUD to enable it

        [Header("Main Menu Buttons")]
        [SerializeField] private Button _playButton;
        [SerializeField] private Button _quitButton;

        [Header("Game Mode Buttons")]
        [SerializeField] private Button _backToMenuButton;

        [Header("Grid Selection Buttons")]
        // We will assign these dynamically or use a unified handler if possible, 
        // but individual buttons are easier for the user to wire up in Inspector.
        [SerializeField] private Button _btn2x2;
        [SerializeField] private Button _btn2x3; // 6 cards
        [SerializeField] private Button _btn4x4;
        [SerializeField] private Button _btn5x6; // 30 cards, even
        // Note: 3x3 (9) and 5x5 (25) are odd and invalid for pairs. 
        // I will provide buttons for valid sizes that are close.

        private void Start()
        {
            // Initial State
            ShowMainMenu();
            _levelSelectionPanel.SetActive(false);
            
            // Listeners
            _playButton.onClick.AddListener(OnPlayClicked);
            _quitButton.onClick.AddListener(OnQuitClicked);
            
            if (_backToMenuButton != null)
                _backToMenuButton.onClick.AddListener(OnBackToMenuClicked);
        }

        private void RefreshLevelButtons()
        {
            int unlocked = Core.SaveManager.GetUnlockedLevel();
            
            // Level Mapping
            SetupLevelButton(_btn2x2, 1, 2, 2, unlocked);
            SetupLevelButton(_btn2x3, 2, 2, 3, unlocked);
            SetupLevelButton(_btn4x4, 3, 4, 4, unlocked);
            SetupLevelButton(_btn5x6, 4, 5, 6, unlocked);
        }

        private void SetupLevelButton(Button btn, int levelIndex, int rows, int cols, int unlockedLevel)
        {
            if (btn == null) return;

            bool isLocked = levelIndex > unlockedLevel;
            btn.interactable = !isLocked;

            // Optional: Visually grey out if locked (Button interactable handles some, but we can force color)
            var image = btn.GetComponent<Image>();
            if (image != null)
            {
                image.color = isLocked ? Color.gray : Color.white;
            }

            // Remove existing listeners then add
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => StartLevel(levelIndex, rows, cols));
        }

        public void ShowMainMenuFromGame()
        {
             ShowMainMenu();
             _levelSelectionPanel.SetActive(false);
        }

        private void ShowMainMenu()
        {
            RefreshLevelButtons();
            _mainMenuPanel.SetActive(true);
            if(_gameHudPanel) _gameHudPanel.SetActive(false);
        }

        private void OnPlayClicked()
        {
            if (Core.SoundManager.Instance != null) Core.SoundManager.Instance.PlayClick();
            _mainMenuPanel.SetActive(false);
            _levelSelectionPanel.SetActive(true);
        }

        private void OnQuitClicked()
        {
            if (Core.SoundManager.Instance != null) Core.SoundManager.Instance.PlayClick();
            Debug.Log("Quit Game");
            Application.Quit();
        }

        private void OnBackToMenuClicked()
        {
             Debug.Log("Back To Menu Clicked");
             if (Core.SoundManager.Instance != null) Core.SoundManager.Instance.PlayClick();
             
             // Save current state before leaving, so we can resume later
             if (Core.GameManager.Instance != null)
             {
                 Core.GameManager.Instance.SaveCurrentGame();
                 Core.GameManager.Instance.StopGame();
             }
             else
             {
                 Debug.LogError("GameManager Instance is null!");
             }

             // Show Menu
             ShowMainMenu();
             _levelSelectionPanel.SetActive(false);
        }

        private void StartLevel(int levelIndex, int rows, int cols)
        {
            if (Core.SoundManager.Instance != null) Core.SoundManager.Instance.PlayClick();
            
            _levelSelectionPanel.SetActive(false);
            if(_gameHudPanel) _gameHudPanel.SetActive(true);

            // Check if we have a save for this level
            var data = Core.SaveManager.LoadGame();
            if (data != null && data.LevelIndex == levelIndex)
            {
                 // Resume
                 Core.GameManager.Instance.ResumeGame(data);
                 // We rely on GameManager to update the grid. 
                 // However, we need to ensure the scene state is ready.
                 // GameManager.ResumeGame does all logic.
            }
            else
            {
                // New Game
                Core.GameManager.Instance.StartGameWithLayout(levelIndex, rows, cols);
            }
        }
    }
}
