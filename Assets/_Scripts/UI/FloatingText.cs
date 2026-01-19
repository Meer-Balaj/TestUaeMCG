using UnityEngine;
using TMPro;

namespace UI
{
    public class FloatingText : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _textComponent; 
        [SerializeField] private float _moveSpeed = 100f; // Pixels per second if on Canvas
        [SerializeField] private float _lifeTime = 1.0f;

        private float _timer;
        private Color _startColor;

        public void Initialize(string text, Color color)
        {
             if (_textComponent == null) _textComponent = GetComponent<TextMeshProUGUI>();
             
             if (_textComponent != null)
             {
                 _textComponent.text = text;
                 _textComponent.color = color;
                 _startColor = color;
             }
             
             _timer = 0f;
             Destroy(gameObject, _lifeTime);
        }

        private void Update()
        {
            // Move up
            transform.Translate(Vector3.up * _moveSpeed * Time.deltaTime);

            // Fade out
            _timer += Time.deltaTime;
            if (_textComponent != null)
            {
                float alpha = Mathf.Lerp(1f, 0f, _timer / _lifeTime);
                _textComponent.color = new Color(_startColor.r, _startColor.g, _startColor.b, alpha);
            }
        }
    }
}
