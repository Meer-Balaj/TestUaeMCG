using UnityEngine;
using TMPro;
using DG.Tweening;

namespace UI
{
    public class FloatingText : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _tmp;
        [SerializeField] private float _distance = 100f;
        [SerializeField] private float _life = 1f;

        public void Initialize(string text, Color color)
        {
            if (!_tmp) _tmp = GetComponent<TextMeshProUGUI>();
            _tmp.text = text;
            _tmp.color = color;
            
            transform.DOMoveY(transform.position.y + _distance, _life).SetEase(Ease.OutQuad);
            _tmp.DOFade(0, _life).SetEase(Ease.InQuad).OnComplete(() => Destroy(gameObject));
        }
    }
}
