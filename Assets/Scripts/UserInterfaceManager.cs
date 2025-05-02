using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UserInterfaceManager : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject gameplayPanel;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject newGamePanel;
    
    [Header("Gameplay UI")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI movesText;
    [SerializeField] private TextMeshProUGUI comboText;

    [Header("Game Over UI")]
    [SerializeField] private TextMeshProUGUI finalScoreText;
    [SerializeField] private TextMeshProUGUI finalMovesText;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button menuButton;
    
    [Header("New Game UI")]
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button newGameButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private TMP_Dropdown gridSizeDropdown;

    [Header("References")]
    [SerializeField] private GameManager gameManager;
    [SerializeField] private ScoreManager scoreManager;

    // Available grid sizes (width, height)
    private readonly (int, int)[] availableGridSizes = new[]
    {
        (2, 2),
        (2, 3),
        (4, 3),
        (4, 4),
        (5, 4),
        (6, 5)
    };
    
    private void Start()
    {
        // Set up event listeners
        if (scoreManager != null)
        {
            scoreManager.OnScoreChanged += UpdateScoreText;
            scoreManager.OnComboChanged += UpdateComboText;
        }
        
        // Set up button listeners
        if (resumeButton != null) resumeButton.onClick.AddListener(HideMenuPanel);
        if (restartButton != null) restartButton.onClick.AddListener(RestartGame);
        if (menuButton != null) menuButton.onClick.AddListener(ShowMenuPanel);
        if (newGameButton != null) newGameButton.onClick.AddListener(StartNewGame);
        if (quitButton != null) quitButton.onClick.AddListener(QuitGame);
        
        // Set up grid size dropdown
        SetupGridSizeDropdown();

        // Show gameplay UI by default
        ShowGameplayUI();
    }
    
    private void SetupGridSizeDropdown()
    {
        if (gridSizeDropdown == null)
        {
            return;
        }
        
        gridSizeDropdown.ClearOptions();
            
        List<string> options = new List<string>();
        foreach (var size in availableGridSizes)
        {
            options.Add($"{size.Item1}x{size.Item2}");
        }
            
        gridSizeDropdown.AddOptions(options);
    }
    
    public void ShowGameplayUI()
    {
        gameplayPanel.SetActive(true);
        gameOverPanel.SetActive(false);
        newGamePanel.SetActive(false);
    }
    
    public void ShowGameOverUI(int finalScore, int totalMoves)
    {
        gameplayPanel.SetActive(false);
        gameOverPanel.SetActive(true);
        newGamePanel.SetActive(false);
        
        finalScoreText.text = $"Final Score: {finalScore}";
        finalMovesText.text = $"Total Moves: {totalMoves}";
    }
    
    private void ShowMenuPanel()
    {
        newGamePanel.SetActive(true);
    }
    
    private void HideMenuPanel()
    {
        newGamePanel.SetActive(false);
    }
    
    private void UpdateScoreText(int score)
    {
        if (scoreText != null)
        {
            scoreText.text = $"Score: {score}";
        }
    }
    
    public void UpdateMovesText(int moves)
    {
        if (movesText != null)
        {
            movesText.text = $"Moves: {moves}";
        }
    }
    
    private void UpdateComboText(int combo)
    {
        if (comboText != null)
        {
            if (combo > 0)
            {
                comboText.gameObject.SetActive(true);
                comboText.text = $"Combo: x{combo + 1}";
            }
            else
            {
                comboText.gameObject.SetActive(false);
            }
        }
    }
    
    private void RestartGame()
    {
        gameManager.RestartGame();
    }
    
    private void StartNewGame()
    {
        int selectedIndex = gridSizeDropdown.value;
        var gridSize = availableGridSizes[selectedIndex];
        
        gameManager.ChangeGridSize(gridSize.Item1, gridSize.Item2);
        HideMenuPanel();
    }

    private void QuitGame()
    {
        gameManager.QuitGame();
    }

    private void OnDestroy()
    {
        // Remove event listeners
        if (scoreManager != null)
        {
            scoreManager.OnScoreChanged -= UpdateScoreText;
            scoreManager.OnComboChanged -= UpdateComboText;
        }
        
        // Remove button listeners
        if (resumeButton != null) resumeButton.onClick.RemoveListener(HideMenuPanel);
        if (restartButton != null) restartButton.onClick.RemoveListener(RestartGame);
        if (menuButton != null) menuButton.onClick.RemoveListener(ShowMenuPanel);
        if (newGameButton != null) newGameButton.onClick.RemoveListener(StartNewGame);
        if (quitButton != null) quitButton.onClick.RemoveListener(QuitGame);
    }
}