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

        public int Rows => _rows;
        public int Cols => _cols;

        public void SetDimensions(int rows, int cols)
        {
            _rows = rows;
            _cols = cols;
        }

        protected override void Start()
        {
            base.Start();
            // Let GameManager decide when to generate or load
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

        public List<CardController> RestoreLayout(Core.GameData data)
        {
            List<CardController> pendingCards = new List<CardController>();
            if (_cardPrefab == null) return pendingCards;
            
            _rows = data.Rows;
            _cols = data.Cols;
            
            ClearGrid();
            SetupGridLayout();
            
            _spawnedCards.Clear();
            int totalCards = _rows * _cols;
            bool hasFaceUpData = data.CardFaceUpStates != null && data.CardFaceUpStates.Count == data.CardLayoutIds.Count;

            for (int i = 0; i < totalCards; i++)
            {
                if (i >= data.CardLayoutIds.Count) break;

                GameObject cardObj = Instantiate(_cardPrefab, _container);
                CardController card = cardObj.GetComponent<CardController>();
                if (card != null)
                {
                    int typeId = data.CardLayoutIds[i];
                    bool isMatched = data.CardMatchedStates[i];
                    bool isFaceUp = hasFaceUpData ? data.CardFaceUpStates[i] : isMatched;

                    Sprite face = (_cardFaceSprites != null && typeId < _cardFaceSprites.Count) 
                                ? _cardFaceSprites[typeId] 
                                : null;

                    card.Initialize(typeId, face, _cardBackSprite);
                    
                    if (isMatched)
                    {
                        card.SetMatched();
                    }
                    else if (isFaceUp)
                    {
                        // Visually open but not matched
                        card.SetFaceUpImmediate(); 
                        pendingCards.Add(card);
                    }
                    
                    card.OnCardClicked += Core.GameManager.Instance.OnCardClicked;
                    _spawnedCards.Add(card);
                }
            }
            return pendingCards;
        }

        public void GetCurrentState(out List<int> ids, out List<bool> matchedStates, out List<bool> faceUpStates)
        {
            ids = new List<int>();
            matchedStates = new List<bool>();
            faceUpStates = new List<bool>();
            
            foreach (var card in _spawnedCards)
            {
                // Safety check if card was destroyed externally
                if (card != null)
                {
                    ids.Add(card.CardTypeId);
                    matchedStates.Add(card.IsMatched);
                    faceUpStates.Add(card.IsFaceUp);
                }
            }
        }

        public void ClearGrid()
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
                // Loop through sprites if we have more pairs than sprites
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
