using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace UI
{
    public enum PanelType { MainMenu, LevelSelect, HUD, Win, Lose }

    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        [Header("Panels")]
        [SerializeField] private GameObject _mainMenuPanel;
        [SerializeField] private GameObject _levelSelectPanel;
        [SerializeField] private GameObject _hudPanel;
        [SerializeField] private GameObject _winPanel;
        [SerializeField] private GameObject _losePanel;

        [Header("Floating Text")]
        [SerializeField] private FloatingText _floatingTextPrefab;
        [SerializeField] private Transform _floatingTextContainer;

        [Header("HUD Elements")]
        [SerializeField] private TextMeshProUGUI _scoreText;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                transform.SetParent(null);
                DontDestroyOnLoad(gameObject);
            }
            else Destroy(gameObject);
        }

        private void Start()
        {
            ShowPanel(PanelType.MainMenu);
            Core.ScoreManager.Instance.OnScoreChanged += UpdateScoreDisplay;
        }

        private void OnDestroy()
        {
            if (Core.ScoreManager.Instance != null)
                Core.ScoreManager.Instance.OnScoreChanged -= UpdateScoreDisplay;
        }

        public void ShowPanel(PanelType type)
        {
            _mainMenuPanel?.SetActive(type == PanelType.MainMenu);
            _levelSelectPanel?.SetActive(type == PanelType.LevelSelect);
            _hudPanel?.SetActive(type == PanelType.HUD);
            _winPanel?.SetActive(type == PanelType.Win);
            _losePanel?.SetActive(type == PanelType.Lose);

            if (type == PanelType.LevelSelect)
            {
                foreach (var lb in FindObjectsOfType<LevelButton>(true)) lb.Refresh();
            }
        }

        public void SpawnScorePopup(Vector3 worldPos, int amount)
        {
            if (!_floatingTextPrefab) return;
            var popup = Instantiate(_floatingTextPrefab, _floatingTextContainer ? _floatingTextContainer : transform);
            popup.transform.position = worldPos + new Vector3(0, 50, 0);
            popup.Initialize($"{(amount >= 0 ? "+" : "")}{amount}", amount >= 0 ? Color.green : Color.red);
        }

        private void UpdateScoreDisplay(int currentScore)
        {
            if (_scoreText) _scoreText.text = $"Score: {currentScore}";
        }

        // Button Callbacks
        public void OnPlayBtn() => ShowPanel(PanelType.LevelSelect);
        public void OnBackBtn() => ShowPanel(PanelType.MainMenu);
        
        public void OnLevelBtn(int level, int rows, int cols)
        {
            var saved = Core.SaveManager.LoadFullState();
            if (saved != null && saved.Level == level)
            {
                Core.GameManager.Instance.ResumeLevel();
            }
            else
            {
                ShowPanel(PanelType.HUD);
                Core.GameManager.Instance.StartLevel(level, rows, cols);
            }
        }

        public void OnRetryBtn() 
        {
            ShowPanel(PanelType.HUD);
            Core.GameManager.Instance.RestartLevel();
        }

        public void OnNextLevelBtn() 
        {
            ShowPanel(PanelType.HUD);
            Core.GameManager.Instance.NextLevel();
        }
        public void OnHomeBtn()
        {
            Core.GameManager.Instance.ReturnToMenu();
            ShowPanel(PanelType.MainMenu);
        }

        public void OnQuitBtn()
        {
            Debug.Log("Quitting Game...");
            Application.Quit();
        }
    }
}
