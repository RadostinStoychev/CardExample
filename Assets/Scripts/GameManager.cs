using System.Collections;
using UnityEngine;
using System;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    [Header("Game Configuration")]
    [SerializeField] private int gridWidth = 4;
    [SerializeField] private int gridHeight = 4;
    [SerializeField] private float matchDelay = 1.0f;
    [SerializeField] private float mismatchDelay = 1.0f;
    
    [Header("References")]
    [SerializeField] private CardManager cardManager;
    [SerializeField] private ScoreManager scoreManager;
    [SerializeField] private AudioManager audioManager;
    [SerializeField] private UserInterfaceManager uiManager;
    
    private Card firstSelectedCard;
    private Card secondSelectedCard;
    private bool canSelectCard = true;
    private int remainingPairs;
    private int totalMoves = 0;
    private bool isGameActive = false;
    private Coroutine checkMatchCoroutine;
    
    public event Action<int> OnMoveMade;
    public event Action OnGameOver;
    
    private void Awake()
    {
        // Singleton pattern with safety check
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        // Load saved game or start new game
        if (SaveSystem.HasSavedGame())
        {
            LoadGame();
        }
        else
        {
            StartNewGame(gridWidth, gridHeight);
        }
        
        // Subscribe to application events for saving
        Application.lowMemory += HandleLowMemory;
    }
    
    private void HandleLowMemory()
    {
        // Handle low memory situations
        Resources.UnloadUnusedAssets();
        GC.Collect();
    }
    
    private void StartNewGame(int width, int height)
    {
        if (width * height % 2 != 0)
        {
            Debug.LogWarning("Grid dimensions must create an even number of cards. Adjusting height.");
            height = height % 2 == 0 ? height : height + 1;
        }
        
        gridWidth = width;
        gridHeight = height;
        
        firstSelectedCard = null;
        secondSelectedCard = null;
        canSelectCard = true;
        totalMoves = 0;
        
        // Stop any ongoing coroutines
        if (checkMatchCoroutine != null)
        {
            StopCoroutine(checkMatchCoroutine);
            checkMatchCoroutine = null;
        }
        
        // Calculate number of pairs
        remainingPairs = (width * height) / 2;
        
        scoreManager.ResetScore();
        
        // Prewarm layout calculation for the new grid size
        cardManager.PrewarmLayoutCalculation(width, height);
        
        // Generate cards
        cardManager.GenerateCards(width, height);
        
        isGameActive = true;
        
        // Update UI
        uiManager.UpdateMovesText(totalMoves);
        uiManager.ShowGameplayUI();
    }
    
    private void SaveGame()
    {
        if (!isGameActive)
        {
            return;
        }

        GameData gameData = new GameData
        {
            GridWidth = gridWidth,
            GridHeight = gridHeight,
            RemainingPairs = remainingPairs,
            TotalMoves = totalMoves,
            Score = scoreManager.CurrentScore,
            CardStates = cardManager.GetCardStates(),
            SpriteMapping = cardManager.GetSpriteMapping()
        };
        
        SaveSystem.SaveGame(gameData);
        Debug.Log("Game saved successfully");
    }
    
    private void LoadGame()
    {
        GameData gameData = SaveSystem.LoadGame();

        if (gameData != null)
        {
            gridWidth = gameData.GridWidth;
            gridHeight = gameData.GridHeight;
            remainingPairs = gameData.RemainingPairs;
            totalMoves = gameData.TotalMoves;
            
            // Reset selection state
            firstSelectedCard = null;
            secondSelectedCard = null;
            canSelectCard = true;
            
            // Stop any ongoing coroutines
            if (checkMatchCoroutine != null)
            {
                StopCoroutine(checkMatchCoroutine);
                checkMatchCoroutine = null;
            }
            
            scoreManager.SetScore(gameData.Score);
            
            // Generate cards with saved state
            cardManager.GenerateCardsFromSave(gridWidth, gridHeight, gameData.CardStates);
            
            isGameActive = true;
            
            uiManager.UpdateMovesText(totalMoves);
            uiManager.ShowGameplayUI();
            
            Debug.Log("Game loaded successfully");
        }
        else
        {
            Debug.LogWarning("No saved game found, starting new game");
            StartNewGame(gridWidth, gridHeight);
        }
    }
    
    public void SelectCard(Card card)
    {
        if (!isGameActive || !canSelectCard || card.IsMatched || card.IsFlipped)
        {
            return;
        }
        
        audioManager.PlayCardFlip();
        card.Flip();
        
        if (firstSelectedCard == null)
        {
            firstSelectedCard = card;
        }
        else if (secondSelectedCard == null && firstSelectedCard != card)
        {
            secondSelectedCard = card;
            
            // Increment moves
            totalMoves++;
            OnMoveMade?.Invoke(totalMoves);
            uiManager.UpdateMovesText(totalMoves);
            
            if (checkMatchCoroutine != null)
            {
                StopCoroutine(checkMatchCoroutine);
            }
            
            checkMatchCoroutine = StartCoroutine(CheckForMatch());
        }
    }
    
    private IEnumerator CheckForMatch()
    {
        if (firstSelectedCard.CardValue == secondSelectedCard.CardValue)
        {
            yield return new WaitForSeconds(matchDelay);
            audioManager.PlayCardMatch();
            
            // Mark cards as matched
            firstSelectedCard.SetMatched();
            secondSelectedCard.SetMatched();
            
            // Update score
            scoreManager.AddScore();
            
            // Check if game is over
            remainingPairs--;
            if (remainingPairs <= 0)
            {
                GameOver();
            }
        }
        else
        {
            yield return new WaitForSeconds(mismatchDelay);
            audioManager.PlayCardMismatch();
            
            // Flip cards back
            firstSelectedCard.Flip();
            secondSelectedCard.Flip();
        }
        
        // Reset selected cards
        firstSelectedCard = null;
        secondSelectedCard = null;
        checkMatchCoroutine = null;
    }
    
    private void GameOver()
    {
        isGameActive = false;
        
        audioManager.PlayGameOver();
        uiManager.ShowGameOverUI(scoreManager.CurrentScore, totalMoves);
        SaveSystem.ClearSave();
        
        OnGameOver?.Invoke();
    }
    
    public void RestartGame()
    {
        StartNewGame(gridWidth, gridHeight);
    }
    
    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
    
    public void ChangeGridSize(int width, int height)
    {
        if (isGameActive)
        {
            SaveGame();
        }
        
        StartNewGame(width, height);
    }
    
    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus && isGameActive)
        {
            SaveGame();
        }
    }
    
    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus && isGameActive)
        {
            SaveGame();
        }
    }
    
    private void OnApplicationQuit()
    {
        if (isGameActive)
        {
            SaveGame();
        }
    }
    
    private void OnDestroy()
    {
        // Clean up event subscriptions
        Application.lowMemory -= HandleLowMemory;
        StopAllCoroutines();
        Instance = null;
    }
}