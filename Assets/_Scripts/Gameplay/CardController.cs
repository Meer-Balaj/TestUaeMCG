using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

namespace Gameplay
{
    public class CardController : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private Image _display;
        
        public int CardTypeId { get; private set; }
        public bool IsMatched { get; private set; }
        public bool IsFaceUp { get; private set; }
        
        private Sprite _front;
        private Sprite _back;
        private bool _animating;

        public void Initialize(int typeId, Sprite front, Sprite back)
        {
            if (!_display) _display = GetComponentInChildren<Image>();
            
            CardTypeId = typeId;
            _front = front;
            _back = back;
            
            if (_display) 
            {
                _display.sprite = back;
                _display.color = Color.white;
            }
            
            IsMatched = false;
            IsFaceUp = false;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!IsFaceUp && !IsMatched && !_animating)
                Core.GameManager.Instance.OnCardSelected(this);
        }

        public void FlipOpen() => StartCoroutine(Flip(true));
        public void FlipClose() => StartCoroutine(Flip(false));

        public void SetMatched()
        {
            IsMatched = true;
            IsFaceUp = true;
            if (_display)
            {
                _display.sprite = _front;
                _display.color = Color.gray;
            }
        }

        private IEnumerator Flip(bool showFront)
        {
            _animating = true;
            if (showFront) Core.AudioManager.Instance.PlayFlip();
            
            float duration = 0.12f;
            for (float t = 0; t < duration; t += Time.deltaTime)
            {
                if (!this || !_display) yield break;
                transform.localScale = new Vector3(Mathf.Lerp(1, 0, t / duration), 1, 1);
                yield return null;
            }

            if (!_display) yield break;
            _display.sprite = showFront ? _front : _back;
            IsFaceUp = showFront;

            for (float t = 0; t < duration; t += Time.deltaTime)
            {
                if (!this || !_display) yield break;
                transform.localScale = new Vector3(Mathf.Lerp(0, 1, t / duration), 1, 1);
                yield return null;
            }
            if (this) transform.localScale = Vector3.one;
            _animating = false;
        }
    }
}
