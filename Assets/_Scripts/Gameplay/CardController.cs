using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Gameplay
{
    public class CardController : MonoBehaviour, IPointerClickHandler
    {
        [Header("References")]
        [SerializeField] private Image _targetImage;

        [Header("Debug/Status")]
        public int CardTypeId;
        public bool IsFaceUp;
        public bool IsMatched;
        public bool IsProcessing;

        private Sprite _frontSprite;
        private Sprite _backSprite;

        public event Action<CardController> OnCardClicked;

        public void Initialize(int cardTypeId, Sprite front, Sprite back)
        {
            CardTypeId = cardTypeId;
            _frontSprite = front;
            _backSprite = back;

            name = $"Card_{cardTypeId}";
            
            // Default state: Face Down
            _targetImage.sprite = _backSprite;
            IsFaceUp = false;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (IsFaceUp || IsMatched || IsProcessing) return;
            OnCardClicked?.Invoke(this);
        }

        public void FlipOpen()
        {
            if (IsFaceUp) return;
            IsFaceUp = true;
            StartCoroutine(FlipRoutine(true));
        }

        public void FlipClose()
        {
            if (!IsFaceUp) return;
            IsFaceUp = false;
            StartCoroutine(FlipRoutine(false));
        }

        public void SetMatched()
        {
            IsMatched = true;
            IsFaceUp = true;
            _targetImage.sprite = _frontSprite;
            _targetImage.color = Color.gray; 
        }

        private IEnumerator FlipRoutine(bool showFront)
        {
            IsProcessing = true;
            float duration = 0.2f;
            float elapsed = 0f;
            Quaternion startRot = transform.localRotation;
            Quaternion midRot = Quaternion.Euler(0, 90, 0);
            Quaternion endRot = Quaternion.Euler(0, 0, 0);

            // Rotate 0 -> 90
            while (elapsed < duration / 2)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / (duration / 2);
                transform.localRotation = Quaternion.Lerp(startRot, midRot, t);
                yield return null;
            }

            // Swap Sprite exactly at 90 degrees
            _targetImage.sprite = showFront ? _frontSprite : _backSprite;

            // Rotate 90 -> 0
            elapsed = 0f;
            while (elapsed < duration / 2)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / (duration / 2);
                transform.localRotation = Quaternion.Lerp(midRot, endRot, t);
                yield return null;
            }

            transform.localRotation = endRot;
            IsProcessing = false;
        }
    }
}
