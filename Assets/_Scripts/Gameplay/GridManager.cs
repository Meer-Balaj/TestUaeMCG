using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Gameplay
{
    [RequireComponent(typeof(GridLayoutGroup))]
    public class GridManager : UIBehaviour
    {
        [Header("Layout Settings")]
        [SerializeField] private int _rows = 4;
        [SerializeField] private int _cols = 4; // 4x4 = 16 cards (8 pairs)
        [SerializeField] private float _spacing = 10f;
        
        [Header("References")]
        [SerializeField] private RectTransform _container;
        [SerializeField] private GameObject _cardPrefab;
        
        [Header("Card Assets")]
        [SerializeField] private Sprite _cardBackSprite;
        [SerializeField] private List<Sprite> _cardFaceSprites;

        private GridLayoutGroup _gridLayout;
        private RectTransform _rectTransform;
        private List<CardController> _spawnedCards = new List<CardController>();

        protected override void Awake()
        {
            base.Awake();
            _gridLayout = GetComponent<GridLayoutGroup>();
            _rectTransform = GetComponent<RectTransform>();
            if (_container == null) _container = _rectTransform;
        }

        protected override void Start()
        {
            base.Start();
            GenerateLayout();
        }

        public void GenerateLayout()
        {
            if (_cardPrefab == null)
            {
                Debug.LogError("Card Prefab is missing!");
                return;
            }

            ClearGrid();
            SetupGridLayout();
            SpawnNewCards();
        }

        private void ClearGrid()
        {
            foreach (Transform child in _container)
            {
                Destroy(child.gameObject);
            }
            _spawnedCards.Clear();
        }

        private void SetupGridLayout()
        {
            if (_gridLayout == null) return;

            _gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            _gridLayout.constraintCount = _cols;
            _gridLayout.spacing = new Vector2(_spacing, _spacing);

            float containerWidth = _container.rect.width;
            float containerHeight = _container.rect.height;
            
            float totalSpacingW = _spacing * (_cols - 1);
            float totalSpacingH = _spacing * (_rows - 1);
            
            float paddingW = _gridLayout.padding.left + _gridLayout.padding.right;
            float paddingH = _gridLayout.padding.top + _gridLayout.padding.bottom;

            float availableWidth = containerWidth - totalSpacingW - paddingW;
            float availableHeight = containerHeight - totalSpacingH - paddingH;

            float cellWidth = availableWidth / _cols;
            float cellHeight = availableHeight / _rows;

            _gridLayout.cellSize = new Vector2(cellWidth, cellHeight);
        }

        private void SpawnNewCards()
        {
            int totalCards = _rows * _cols;
            if (totalCards % 2 != 0)
            {
                Debug.LogError("Total cards must be even!");
                return;
            }

            if (_cardFaceSprites == null || _cardFaceSprites.Count == 0)
            {
                Debug.LogError("No face sprites assigned in GridManager!");
                return;
            }

            int pairCount = totalCards / 2;
            int[] cardTypeIds = new int[totalCards];

            for (int i = 0; i < pairCount; i++)
            {
                int typeId = i % _cardFaceSprites.Count; 
                cardTypeIds[i * 2] = typeId;
                cardTypeIds[i * 2 + 1] = typeId;
            }

            // Shuffle
            for (int i = totalCards - 1; i > 0; i--)
            {
                int r = Random.Range(0, i + 1);
                int temp = cardTypeIds[i];
                cardTypeIds[i] = cardTypeIds[r];
                cardTypeIds[r] = temp;
            }

            _spawnedCards.Clear();
            for (int i = 0; i < totalCards; i++)
            {
                GameObject cardObj = Instantiate(_cardPrefab, _container);
                CardController card = cardObj.GetComponent<CardController>();
                if (card != null)
                {
                    int typeId = cardTypeIds[i];
                    Sprite face = _cardFaceSprites[typeId];
                    card.Initialize(typeId, face, _cardBackSprite);
                    card.OnCardClicked += Core.GameManager.Instance.OnCardClicked;
                    _spawnedCards.Add(card);
                }
            }
        }
        
        protected override void OnRectTransformDimensionsChange()
        {
            base.OnRectTransformDimensionsChange();
            if (_gridLayout != null)
            {
                SetupGridLayout();
            }
        }
    }
}
