using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using DG.Tweening;

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
        private Tween _flipTween;

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
            transform.localScale = Vector3.one;
        }

        public void AnimateSpawn(float delay)
        {
            transform.localScale = Vector3.zero;
            transform.DOScale(Vector3.one, 0.5f)
                .SetDelay(delay)
                .SetEase(Ease.OutBack)
                .SetUpdate(true);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!IsFaceUp && !IsMatched && !_animating)
                Core.GameManager.Instance.OnCardSelected(this);
        }

        public void FlipOpen() => Flip(true);
        public void FlipClose() => Flip(false);

        public void SetMatched()
        {
            IsMatched = true;
            IsFaceUp = true;
            
            // Correct match animation: grow larger then back to normal
            transform.DOScale(Vector3.one * 1.2f, 0.2f).SetLoops(2, LoopType.Yoyo).SetEase(Ease.OutQuad);
            _display.DOColor(Color.gray, 0.4f);
            
            if (_display)
            {
                _display.sprite = _front;
            }
        }

        public void AnimateMismatch()
        {
            // Shake animation for mismatch + delay flip back
            Sequence mismatchSeq = DOTween.Sequence();
            mismatchSeq.Append(transform.DOShakePosition(0.4f, 15f, 20, 90, false, true));
            mismatchSeq.OnComplete(() => FlipClose());
        }

        private void Flip(bool showFront)
        {
            if (_animating) _flipTween?.Kill();
            
            _animating = true;
            if (showFront) Core.AudioManager.Instance.PlayFlip();

            _flipTween = transform.DOScaleX(0, 0.12f).OnComplete(() =>
            {
                if (!_display) return;
                _display.sprite = showFront ? _front : _back;
                IsFaceUp = showFront;
                
                transform.DOScaleX(1, 0.12f).OnComplete(() => _animating = false);
            });
        }

        private void OnDestroy()
        {
            if (Application.isPlaying)
            {
                _flipTween?.Kill();
                transform.DOKill();
            }
        }
    }
}
