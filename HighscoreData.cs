using System;

namespace Tetris;

[Serializable]
public class HighscoreData
{
    public HighscoreData(string name, int score, int level, int totalLinesCleared)
    {
        Name = name;
        Score = score;
        Level = level;
        TotalLinesCleared = totalLinesCleared;
    }

    public HighscoreData()
    {
    }

    public string Name { get; set; }
    public int Score { get; set; }
    public int Level { get; set; }
    public int TotalLinesCleared { get; set; }

    public override string ToString()
    {
        return $"{Name} - Score: {Score}, Level: {Level}, Lines Cleared: {TotalLinesCleared}";
    }
}