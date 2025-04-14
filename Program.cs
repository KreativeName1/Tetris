using System.IO;
using Tetris;

if (!Directory.Exists(Tetris.Tetris.DataDir)) Directory.CreateDirectory(Tetris.Tetris.DataDir);

using var game = new Tetris.Tetris();
game.Run();