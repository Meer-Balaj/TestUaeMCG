using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System.Collections;

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

        private int _displayedScore;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                transform.SetParent(null);
                DontDestroyOnLoad(gameObject);
            }
            else Destroy(gameObject);
            
            // Ensure floating text container is on top of other UI if it exists
            if (_floatingTextContainer) _floatingTextContainer.SetAsLastSibling();
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
            // Add a small delay for Win/Lose panels to let game animations finish
            float delay = (type == PanelType.Win || type == PanelType.Lose) ? 1.0f : 0f;
            
            if (delay > 0) StartCoroutine(ShowPanelDelayed(type, delay));
            else ExecuteShowPanel(type);
        }

        private IEnumerator ShowPanelDelayed(PanelType type, float delay)
        {
            yield return new WaitForSeconds(delay);
            ExecuteShowPanel(type);
        }

        private void ExecuteShowPanel(PanelType type)
        {
            TogglePanel(_mainMenuPanel, type == PanelType.MainMenu);
            TogglePanel(_levelSelectPanel, type == PanelType.LevelSelect);
            TogglePanel(_hudPanel, type == PanelType.HUD);
            TogglePanel(_winPanel, type == PanelType.Win);
            TogglePanel(_losePanel, type == PanelType.Lose);

            if (type == PanelType.LevelSelect)
            {
                foreach (var lb in FindObjectsOfType<LevelButton>(true)) lb.Refresh();
            }
        }

        private void TogglePanel(GameObject panel, bool show)
        {
            if (!panel) return;

            CanvasGroup cg = panel.GetComponent<CanvasGroup>();
            if (!cg) cg = panel.AddComponent<CanvasGroup>();

            if (show)
            {
                panel.SetActive(true);
                panel.transform.localScale = Vector3.one * 0.8f;
                cg.alpha = 0;
                
                panel.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack).SetUpdate(true);
                cg.DOFade(1, 0.4f).SetUpdate(true);
            }
            else
            {
                if (panel.activeSelf)
                {
                    panel.transform.DOScale(Vector3.one * 0.8f, 0.3f).SetEase(Ease.InBack).SetUpdate(true);
                    cg.DOFade(0, 0.3f).SetUpdate(true).OnComplete(() => panel.SetActive(false));
                }
            }
        }

        public void SpawnScorePopup(Vector3 worldPos, int amount)
        {
            if (!_floatingTextPrefab) return;
            
            // Convert world pos (grid) to screen space if needed, button usually worldPos is already screen space for UI
            var popup = Instantiate(_floatingTextPrefab, _floatingTextContainer ? _floatingTextContainer : transform);
            popup.transform.position = worldPos + new Vector3(0, 50, 0);
            popup.transform.SetAsLastSibling(); // Ensure it's on top of other popups
            popup.Initialize($"{(amount >= 0 ? "+" : "")}{amount}", amount >= 0 ? Color.green : Color.red);
        }

        private void UpdateScoreDisplay(int currentScore)
        {
            if (_scoreText)
            {
                // Smoothly animate the number rolling
                DOTween.To(() => _displayedScore, x => {
                    _displayedScore = x;
                    _scoreText.text = $"Score: {x}";
                }, currentScore, 0.5f).SetUpdate(true);
                
                _scoreText.transform.DOKill();
                _scoreText.transform.localScale = Vector3.one;
                _scoreText.transform.DOPunchScale(Vector3.one * 0.15f, 0.3f, 10, 1).SetUpdate(true);
            }
        }

        // Button Callbacks
        public void OnPlayBtn() => ShowPanel(PanelType.LevelSelect);
        public void OnBackBtn() => ShowPanel(PanelType.MainMenu);
        
        public void OnLevelBtn(int level, int rows, int cols)
        {
            var saved = Core.SaveManager.LoadFullState();
            if (saved != null && saved.Level == level && saved.CardIds != null && saved.CardIds.Count > 0)
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
