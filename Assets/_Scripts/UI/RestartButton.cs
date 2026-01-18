using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class RestartButton : MonoBehaviour
    {
        private Button _button;

        private void Awake()
        {
            _button = GetComponent<Button>();
            if (_button != null)
            {
                _button.onClick.AddListener(OnRestartClicked);
            }
        }

        private void OnRestartClicked()
        {
            if (Core.SoundManager.Instance != null) Core.SoundManager.Instance.PlayClick();
            if (Core.GameManager.Instance != null)
            {
                Core.GameManager.Instance.RestartGame();
            }
        }
    }
}
