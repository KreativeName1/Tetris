using System;

namespace Tetris
{
    public class ScoreManager
    {
        public int Score { get; private set; }
        public int Level { get; private set; }
        public int TotalLinesCleared { get; private set; }

        public ScoreManager()
        {
            Reset();
        }

        public void UpdateScore(int linesCleared)
        {
            if (linesCleared > 0)
            {
                int baseScore = 10;
                int scoreIncrease = (int)(baseScore * Math.Pow(2, linesCleared - 1) * Level);
                Score += scoreIncrease;
                TotalLinesCleared += linesCleared;
                Level = (TotalLinesCleared / 10) + 1;
            }
        }

        public double GetSpeed()
        {
            return Math.Max(0.1, 0.5 - (Level - 1) * 0.05);
        }

        public void Reset()
        {
            Score = 0;
            Level = 1;
            TotalLinesCleared = 0;
        }
    }
}