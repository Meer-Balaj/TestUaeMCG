using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public enum GameAction { Play, Back, Retry, Next, Home, Quit }

    public class UIButton : MonoBehaviour
    {
        [SerializeField] private GameAction _action;
        
        private void Awake()
        {
            var btn = GetComponent<Button>();
            if (btn) btn.onClick.AddListener(OnClick);
        }

        private void OnClick()
        {
            if (Core.AudioManager.Instance) Core.AudioManager.Instance.PlayClick();
            
            switch (_action)
            {
                case GameAction.Play: UIManager.Instance.OnPlayBtn(); break;
                case GameAction.Back: UIManager.Instance.OnBackBtn(); break;
                case GameAction.Retry: UIManager.Instance.OnRetryBtn(); break;
                case GameAction.Next: UIManager.Instance.OnNextLevelBtn(); break;
                case GameAction.Home: UIManager.Instance.OnHomeBtn(); break;
                case GameAction.Quit: UIManager.Instance.OnQuitBtn(); break;
            }
        }
    }
}
