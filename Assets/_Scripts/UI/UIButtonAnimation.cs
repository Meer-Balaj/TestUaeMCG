using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using UnityEngine.UI;

namespace UI
{
    [RequireComponent(typeof(Button))]
    public class UIButtonAnimation : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private float _scaleFactor = 0.95f;
        [SerializeField] private float _duration = 0.1f;
        
        private Vector3 _originalScale;

        private void Awake()
        {
            _originalScale = transform.localScale;
        }

        private void OnDestroy()
        {
            // Check if application is playing to avoid creating [DOTween] manager during quit
            if (Application.isPlaying)
            {
                transform.DOKill();
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            transform.DOScale(_originalScale * _scaleFactor, _duration).SetUpdate(true);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            transform.DOScale(_originalScale, _duration).SetUpdate(true);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            transform.DOScale(_originalScale * 1.05f, _duration).SetUpdate(true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            transform.DOScale(_originalScale, _duration).SetUpdate(true);
        }

        private void OnDisable()
        {
            if (Application.isPlaying)
            {
                transform.localScale = _originalScale;
                transform.DOKill();
            }
        }
    }
}
