
using Tetris;

for (int i = 0; i < 10; i++)
{
    HighscoreManager.SaveHighscore(new HighscoreData($"Player {i + 1}", 1000 + i, 10 + i, 1));
}

using var game = new Tetris.Tetris();
game.Run();