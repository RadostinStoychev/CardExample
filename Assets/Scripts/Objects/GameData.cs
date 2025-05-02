using System;

[Serializable]
public class GameData
{
    public int GridWidth;
    public int GridHeight;
    public int RemainingPairs;
    public int TotalMoves;
    public int Score;
    public CardState[] CardStates;
    
    // Value-to-sprite mapping record
    public ValueToSpriteMapping[] SpriteMapping;
}