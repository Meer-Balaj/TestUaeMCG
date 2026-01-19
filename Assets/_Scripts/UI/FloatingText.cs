using UnityEngine;
using TMPro;

namespace UI
{
    public class FloatingText : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _tmp;
        [SerializeField] private float _speed = 100f;
        [SerializeField] private float _life = 1f;

        private float _time;
        private Color _color;

        public void Initialize(string text, Color color)
        {
            if (!_tmp) _tmp = GetComponent<TextMeshProUGUI>();
            _tmp.text = text;
            _tmp.color = color;
            _color = color;
            Destroy(gameObject, _life);
        }

        private void Update()
        {
            transform.Translate(Vector3.up * _speed * Time.deltaTime);
            _time += Time.deltaTime;
            if (_tmp) _tmp.color = new Color(_color.r, _color.g, _color.b, 1 - (_time / _life));
        }
    }
}
