using Microsoft.Xna.Framework;

namespace Tetris;

public class GameBoard
{
    public static int CellSize = 30;
    public static int GridWidth = 10;
    public static int GridHeight = 20;

    public GameBoard()
    {
        Reset();
    }

    public bool IsGameOver { get; private set; }
    public Color[,] Grid { get; private set; }

    public bool IsCollision(Tetromino tetromino)
    {
        for (var y = 0; y < tetromino.Shape.GetLength(0); y++)
        for (var x = 0; x < tetromino.Shape.GetLength(1); x++)
            if (tetromino.Shape[y, x] != 0)
            {
                var gridX = (int)tetromino.Position.X + x;
                var gridY = (int)tetromino.Position.Y + y;

                if (gridX < 0 || gridX >= GridWidth || gridY >= GridHeight)
                    return true;

                if (gridY >= 0 && Grid[gridY, gridX] != Color.Transparent)
                    return true;
            }

        return false;
    }

    public void PlaceTetromino(Tetromino tetromino)
    {
        for (var y = 0; y < tetromino.Shape.GetLength(0); y++)
        for (var x = 0; x < tetromino.Shape.GetLength(1); x++)
            if (tetromino.Shape[y, x] != 0)
            {
                var gridX = (int)tetromino.Position.X + x;
                var gridY = (int)tetromino.Position.Y + y;

                if (gridX >= 0 && gridX < GridWidth && gridY >= 0 && gridY < GridHeight)
                {
                    Grid[gridY, gridX] = tetromino.Color;

                    // Check for game over condition
                    if (gridY <= 0)
                    {
                        IsGameOver = true;
                        return;
                    }
                }
            }
    }

    public int ClearCompletedLines()
    {
        var linesCleared = 0;
        for (var y = GridHeight - 1; y >= 0; y--)
            if (IsLineComplete(y))
            {
                ClearLine(y);
                ShiftLinesDown(y);
                linesCleared++;
                y++; // Recheck the same row
            }

        return linesCleared;
    }

    private bool IsLineComplete(int y)
    {
        for (var x = 0; x < GridWidth; x++)
            if (Grid[y, x] == Color.Transparent)
                return false;

        return true;
    }

    private void ClearLine(int y)
    {
        for (var x = 0; x < GridWidth; x++) Grid[y, x] = Color.Transparent;
    }

    private void ShiftLinesDown(int clearedLine)
    {
        for (var y = clearedLine; y > 0; y--)
        for (var x = 0; x < GridWidth; x++)
            Grid[y, x] = Grid[y - 1, x];

        // Clear the top line
        for (var x = 0; x < GridWidth; x++) Grid[0, x] = Color.Transparent;
    }

    public void Reset()
    {
        Grid = new Color[GridHeight, GridWidth];
        IsGameOver = false;
    }
}