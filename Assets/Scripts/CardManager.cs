using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class CardState
{
    public int Value;
    public bool IsMatched;
    public bool IsFlipped;
}

public class CardManager : MonoBehaviour
{
    [Header("Card Settings")]
    [SerializeField] private GameObject cardPrefab;
    [SerializeField] private Transform cardContainer;
    [SerializeField] private List<Sprite> cardImages = new List<Sprite>();
    [SerializeField] private Sprite cardBackSprite;
    
    [Header("Layout Settings")]
    [SerializeField] private GridLayoutGroup gridLayout;
    [SerializeField] private RectTransform gridRectTransform;
    [SerializeField] private float cardAspectRatio = 0.7f; // Width / Height
    [SerializeField] private float cardSpacing = 10f;
    [SerializeField] private float edgePadding = 20f;
    
    [Header("Pool Settings")]
    [SerializeField] private int initialPoolSize = 24;
    [SerializeField] private int maxPoolSize = 60;
    
    private List<Card> activeCards = new List<Card>();
    private List<Card> cardPool = new List<Card>();
    private Dictionary<int, int> valueToSpriteIndex = new Dictionary<int, int>();
    
    // Cached list for card values to avoid garbage collection
    private List<int> cardValues = new List<int>(60);
    private List<int> spriteIndices = new List<int>(30);
    
    private void Awake()
    {
        // Initialize object pool
        InitializeCardPool();
    }
    
    private void InitializeCardPool()
    {
        for (int i = 0; i < initialPoolSize; i++)
        {
            CreatePooledCard();
        }
    }
    
    private Card CreatePooledCard()
    {
        GameObject cardObject = Instantiate(cardPrefab, cardContainer);
        Card card = cardObject.GetComponent<Card>();
        cardObject.SetActive(false);
        cardPool.Add(card);
        return card;
    }
    
    private Card GetCardFromPool()
    {
        // Find inactive card in pool
        for (int i = 0; i < cardPool.Count; i++)
        {
            if (!cardPool[i].gameObject.activeSelf)
            {
                cardPool[i].gameObject.SetActive(true);
                return cardPool[i];
            }
        }
        
        // If no inactive cards and we haven't reached max pool size, create new card
        if (cardPool.Count < maxPoolSize)
        {
            Card newCard = CreatePooledCard();
            newCard.gameObject.SetActive(true);
            return newCard;
        }
        
        // If we've reached max pool size, reuse the first card (should never happen with proper sizing)
        Debug.LogWarning("Card pool maximum reached. Consider increasing the pool size.");
        Card recycledCard = cardPool[0];
        recycledCard.gameObject.SetActive(true);
        recycledCard.Reset();
        return recycledCard;
    }
    
    private void ReturnAllCardsToPool()
    {
        foreach (Card card in activeCards)
        {
            card.gameObject.SetActive(false);
        }
        activeCards.Clear();
    }
    
    public void GenerateCards(int width, int height)
    {
        // Clear existing cards
        ReturnAllCardsToPool();
        
        // Calculate card size based on available space
        SetupGridLayout(width, height);
        
        // Create card pairs
        int totalCards = width * height;
        int numPairs = totalCards / 2;
        
        // Create list of card values (pairs of 0, 1, 2, etc.)
        cardValues.Clear();
        for (int i = 0; i < numPairs; i++)
        {
            cardValues.Add(i);
            cardValues.Add(i);
        }
        
        // Shuffle the card values
        ShuffleList(cardValues, totalCards);
        
        // Assign random sprite to each value
        AssignSpritesToValues(numPairs);
        
        // Create the cards
        for (int i = 0; i < totalCards; i++)
        {
            Card card = GetCardFromPool();
            SetupCard(card, cardValues[i]);
            activeCards.Add(card);
        }
    }
    
    public void GenerateCardsFromSave(int width, int height, CardState[] cardStates)
    {
        // Clear existing cards
        ReturnAllCardsToPool();
        
        // Calculate card size based on available space
        SetupGridLayout(width, height);
        
        // Create the cards from saved states
        foreach (CardState cardState in cardStates)
        {
            Card card = GetCardFromPool();
            SetupCard(card, cardState.Value);
            
            // Restore card state
            if (cardState.IsMatched)
            {
                card.SetMatched();
            }
            else if (cardState.IsFlipped)
            {
                card.Flip();
            }
            
            activeCards.Add(card);
        }
    }
    
    private void SetupGridLayout(int width, int height)
    {
        // Set grid dimensions
        gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gridLayout.constraintCount = width;
        
        // Calculate available space
        float availableWidth = gridRectTransform.rect.width - (edgePadding * 2) - (cardSpacing * (width - 1));
        float availableHeight = gridRectTransform.rect.height - (edgePadding * 2) - (cardSpacing * (height - 1));
        
        // Calculate card size based on available space and aspect ratio
        float cardWidth = availableWidth / width;
        float cardHeight = availableHeight / height;
        
        // Adjust to maintain aspect ratio
        if (cardWidth / cardHeight > cardAspectRatio)
        {
            cardWidth = cardHeight * cardAspectRatio;
        }
        else
        {
            cardHeight = cardWidth / cardAspectRatio;
        }
        
        // Set cell size and spacing
        gridLayout.cellSize = new Vector2(cardWidth, cardHeight);
        gridLayout.spacing = new Vector2(cardSpacing, cardSpacing);
        gridLayout.padding.left = gridLayout.padding.right = Mathf.FloorToInt(edgePadding);
        gridLayout.padding.top = gridLayout.padding.bottom = Mathf.FloorToInt(edgePadding);
    }
    
    private void SetupCard(Card card, int value)
    {
        // Set card properties
        int spriteIndex = valueToSpriteIndex[value];
        card.Setup(value, cardImages[spriteIndex], cardBackSprite);
    }
    
    private void AssignSpritesToValues(int numPairs)
    {
        valueToSpriteIndex.Clear();
        spriteIndices.Clear();
        
        // Create a shuffled list of sprite indices
        int availableSprites = Mathf.Min(cardImages.Count, numPairs);
        
        for (int i = 0; i < availableSprites; i++)
        {
            spriteIndices.Add(i);
        }
        
        // If we need more pairs than available sprites, add random sprites again
        while (spriteIndices.Count < numPairs)
        {
            spriteIndices.Add(Random.Range(0, availableSprites));
        }
        
        ShuffleList(spriteIndices, numPairs);
        
        // Assign sprite indices to card values
        for (int i = 0; i < numPairs; i++)
        {
            valueToSpriteIndex[i] = spriteIndices[i];
        }
    }
    
    private void ShuffleList<T>(List<T> list, int count)
    {
        int n = count;
        while (n > 1)
        {
            n--;
            int k = Random.Range(0, n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }
    
    public CardState[] GetCardStates()
    {
        CardState[] cardStates = new CardState[activeCards.Count];
        
        for (int i = 0; i < activeCards.Count; i++)
        {
            cardStates[i] = new CardState
            {
                Value = activeCards[i].CardValue,
                IsMatched = activeCards[i].IsMatched,
                IsFlipped = activeCards[i].IsFlipped
            };
        }
        
        return cardStates;
    }
    
    // Optimize loading of grid when changing layouts
    public void PrewarmLayoutCalculation(int width, int height)
    {
        // Calculate and cache layout values without creating cards
        SetupGridLayout(width, height);
    }
    
    // Memory optimization when scene changes
    private void OnDestroy()
    {
        valueToSpriteIndex.Clear();
        cardValues.Clear();
        spriteIndices.Clear();
        activeCards.Clear();
        cardPool.Clear();
    }
}