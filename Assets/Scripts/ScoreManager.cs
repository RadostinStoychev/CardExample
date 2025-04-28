using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ScoreManager : MonoBehaviour
{
    [Header("Score Settings")]
    [SerializeField] private int baseMatchScore = 10;
    [SerializeField] private int comboMultiplier = 2;
    [SerializeField] private float comboTimeWindow = 5f;
    [SerializeField] private int maxComboMultiplier = 8;
    
    private int currentScore = 0;
    private int currentCombo = 0;
    private float lastMatchTime = 0f;
    
    public int CurrentScore => currentScore;
    public int CurrentCombo => currentCombo;
    
    public event Action<int> OnScoreChanged;
    public event Action<int> OnComboChanged;
    
    private void Start()
    {
        ResetScore();
    }
    
    public void ResetScore()
    {
        currentScore = 0;
        currentCombo = 0;
        lastMatchTime = 0f;
        
        OnScoreChanged?.Invoke(currentScore);
        OnComboChanged?.Invoke(currentCombo);
    }
    
    public void SetScore(int score)
    {
        currentScore = score;
        OnScoreChanged?.Invoke(currentScore);
    }
    
    public void AddScore()
    {
        // Check if this match is part of a combo
        float currentTime = Time.time;
        if (currentTime - lastMatchTime <= comboTimeWindow && lastMatchTime > 0)
        {
            // Increase combo
            currentCombo = Mathf.Min(currentCombo + 1, maxComboMultiplier);
        }
        else
        {
            // Reset combo
            currentCombo = 0;
        }
        
        // Update last match time
        lastMatchTime = currentTime;
        
        // Calculate score to add
        int scoreToAdd = baseMatchScore;
        
        // Apply combo multiplier if combo > 0
        if (currentCombo > 0)
        {
            scoreToAdd *= (1 + currentCombo * comboMultiplier / 10);
        }
        
        // Add score
        currentScore += scoreToAdd;
        
        // Fire events
        OnScoreChanged?.Invoke(currentScore);
        OnComboChanged?.Invoke(currentCombo);
    }
}