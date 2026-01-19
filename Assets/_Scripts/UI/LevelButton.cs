using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class LevelButton : MonoBehaviour
    {
        [SerializeField] private int _levelIndex;
        [SerializeField] private int _rows;
        [SerializeField] private int _cols;

        private Button _btn;

        private void Awake()
        {
            _btn = GetComponent<Button>();
            _btn.onClick.AddListener(OnClick);
        }

        private void OnClick()
        {
            if (Core.AudioManager.Instance) Core.AudioManager.Instance.PlayClick();
            UIManager.Instance.OnLevelBtn(_levelIndex, _rows, _cols);
        }

        public void Refresh()
        {
            if (!_btn) _btn = GetComponent<Button>();
            int unlocked = Core.SaveManager.LoadProgress();
            _btn.interactable = _levelIndex <= unlocked;
            
            var img = GetComponent<Image>();
            if (img) img.color = _btn.interactable ? Color.white : Color.gray;
        }

        private void Start() => Refresh();
    }
}
