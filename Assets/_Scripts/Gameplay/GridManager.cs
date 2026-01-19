using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Gameplay
{
    [RequireComponent(typeof(GridLayoutGroup))]
    public class GridManager : MonoBehaviour
    {
        [SerializeField] private GameObject _cardPrefab;
        [SerializeField] private List<Sprite> _faces;
        [SerializeField] private Sprite _back;

        public int Rows { get; private set; }
        public int Cols { get; private set; }

        private GridLayoutGroup _layout;
        private List<CardController> _spawned = new List<CardController>();

        private void EnsureLayout()
        {
            if (!_layout) _layout = GetComponent<GridLayoutGroup>();
        }

        public void SetDimensions(int rows, int cols)
        {
            EnsureLayout();
            Rows = rows;
            Cols = cols;
            if (_layout) _layout.constraintCount = cols;
        }

        public void ClearGrid()
        {
            foreach (Transform child in transform) Destroy(child.gameObject);
            _spawned.Clear();
        }

        private void CalculateCardSize()
        {
            RectTransform rect = GetComponent<RectTransform>();
            float width = rect.rect.width - (_layout.padding.left + _layout.padding.right);
            float height = rect.rect.height - (_layout.padding.top + _layout.padding.bottom);
            float cellW = (width - (_layout.spacing.x * (Cols - 1))) / Cols;
            float cellH = (height - (_layout.spacing.y * (Rows - 1))) / Rows;
            _layout.cellSize = new Vector2(cellW, cellH);
        }

        public void GenerateLayout()
        {
            EnsureLayout();
            ClearGrid();
            CalculateCardSize();

            int total = Rows * Cols;
            List<int> ids = new List<int>();
            for (int i = 0; i < total / 2; i++) { ids.Add(i % _faces.Count); ids.Add(i % _faces.Count); }

            for (int i = 0; i < ids.Count; i++)
            {
                int r = Random.Range(i, ids.Count);
                int tmp = ids[i]; ids[i] = ids[r]; ids[r] = tmp;
            }

            foreach (int id in ids) CreateCard(id);
        }

        public void RestoreLayout(List<int> ids, List<bool> matched)
        {
            EnsureLayout();
            ClearGrid();
            CalculateCardSize();

            for (int i = 0; i < ids.Count; i++)
            {
                var card = CreateCard(ids[i]);
                if (matched[i]) card.SetMatched();
            }
        }

        private CardController CreateCard(int id)
        {
            var card = Instantiate(_cardPrefab, transform).GetComponent<CardController>();
            card.Initialize(id, _faces[id], _back);
            _spawned.Add(card);
            return card;
        }

        public void GetState(out List<int> ids, out List<bool> matched)
        {
            ids = new List<int>(); matched = new List<bool>();
            foreach (var c in _spawned) { ids.Add(c.CardTypeId); matched.Add(c.IsMatched); }
        }
    }
}
