using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Tetris;

public class Renderer
{
    // Define offsets for the game field
    private const int GameFieldOffsetX = 250;
    private const int GameFieldOffsetY = 20;
    private readonly GraphicsDeviceManager _graphics;
    private readonly GraphicsDevice _graphicsDevice;
    private readonly SpriteBatch _spriteBatch;

    private Texture2D _blockTexture;
    private List<(Tetromino tetromino, Vector2 position)> _boardTetrominos;
    private readonly Controls _controls;
    private Texture2D _pixel;
    private SpriteFont _scoreFont;

    // for title screen tetrominos
    private List<(Tetromino tetromino, Vector2 position, int size)> _titleScreenTetrominos;

    public Renderer(GraphicsDevice graphicsDevice, GraphicsDeviceManager graphics, Controls controls)
    {
        _graphicsDevice = graphicsDevice;
        _graphics = graphics;
        _spriteBatch = new SpriteBatch(graphicsDevice);
        _controls = controls;
        InitializeTitleScreenTetrominos();
    }

    public void LoadContent(ContentManager content)
    {
        _blockTexture = content.Load<Texture2D>("block");
        _scoreFont = content.Load<SpriteFont>("ScoreFont");

        _pixel = new Texture2D(_graphicsDevice, 1, 1);
        _pixel.SetData(new[] { Color.White });
    }

    private void InitializeTitleScreenTetrominos()
    {
        var random = new Random();
        _titleScreenTetrominos = new List<(Tetromino, Vector2, int)>();
        _boardTetrominos = new List<(Tetromino, Vector2)>();

        for (var i = 0; i < 100; i++)
        {
            var tetromino = Tetromino.GetRandomTetromino();
            var size = random.Next(15, 31);
            var x = random.Next(0, _graphics.PreferredBackBufferWidth - tetromino.Width * size);
            var y = random.Next(0, _graphics.PreferredBackBufferHeight - tetromino.Height * size);
            _titleScreenTetrominos.Add((tetromino, new Vector2(x, y), size));
        }

        var cellSize = 20;
        var boardWidth = 10 * cellSize;
        var boardHeight = 20 * cellSize;
        var boardX = _graphics.PreferredBackBufferWidth - boardWidth - 20;
        var boardY = (_graphics.PreferredBackBufferHeight - boardHeight) / 2;

        for (var i = 0; i < 3; i++)
        {
            var tetromino = Tetromino.GetRandomTetromino();
            var x = boardX + random.Next(0, 10 - tetromino.Width) * cellSize;
            var y = boardY + random.Next(0, 20 - tetromino.Height) * cellSize;
            _boardTetrominos.Add((tetromino, new Vector2(x, y)));
        }
    }

    public void DrawGame(GameBoard gameBoard, Tetromino currentTetromino, Tetromino heldTetromino,
        Tetromino nextTetromino, ScoreManager scoreManager)
    {
        _graphicsDevice.Clear(Color.Black);

        _spriteBatch.Begin();

        DrawBorder(GameFieldOffsetX, GameFieldOffsetY, GameBoard.GridWidth * GameBoard.CellSize,
            GameBoard.GridHeight * GameBoard.CellSize, 2, Color.White);

        for (var y = 0; y < GameBoard.GridHeight; y++)
        for (var x = 0; x < GameBoard.GridWidth; x++)
        {
            var cellColor = gameBoard.Grid[y, x];
            if (cellColor != Color.Transparent)
                _spriteBatch.Draw(_blockTexture,
                    new Rectangle(GameFieldOffsetX + x * GameBoard.CellSize, GameFieldOffsetY + y * GameBoard.CellSize,
                        GameBoard.CellSize, GameBoard.CellSize),
                    cellColor);
        }

        DrawTetromino(currentTetromino, GameFieldOffsetX, GameFieldOffsetY);
        DrawHeldTetromino(heldTetromino);
        DrawNextTetromino(nextTetromino);
        DrawScore(scoreManager);

        _spriteBatch.End();
    }

    private void DrawBorder(int x, int y, int width, int height, int thickness, Color color)
    {
        // top line
        _spriteBatch.Draw(_blockTexture, new Rectangle(x - thickness, y - thickness, width + thickness * 2, thickness),
            color);
        // bottom line
        _spriteBatch.Draw(_blockTexture, new Rectangle(x - thickness, y + height, width + thickness * 2, thickness),
            color);
        // left line
        _spriteBatch.Draw(_blockTexture, new Rectangle(x - thickness, y - thickness, thickness, height + thickness * 2),
            color);
        // right line
        _spriteBatch.Draw(_blockTexture, new Rectangle(x + width, y - thickness, thickness, height + thickness * 2),
            color);
    }

    private void DrawTetromino(Tetromino tetromino, int offsetX, int offsetY)
    {
        for (var y = 0; y < tetromino.Shape.GetLength(0); y++)
        for (var x = 0; x < tetromino.Shape.GetLength(1); x++)
            if (tetromino.Shape[y, x] != 0)
            {
                var drawX = (int)tetromino.Position.X + x;
                var drawY = (int)tetromino.Position.Y + y;

                if (drawX >= 0 && drawX < GameBoard.GridWidth && drawY >= 0 && drawY < GameBoard.GridHeight)
                    _spriteBatch.Draw(_blockTexture,
                        new Rectangle(offsetX + drawX * GameBoard.CellSize, offsetY + drawY * GameBoard.CellSize,
                            GameBoard.CellSize, GameBoard.CellSize),
                        tetromino.Color);
            }
    }

    private void DrawHeldTetromino(Tetromino heldTetromino)
    {
        var holdAreaX = 20;
        var holdAreaY = 50;

        DrawBorder(holdAreaX, holdAreaY, 6 * GameBoard.CellSize, 5 * GameBoard.CellSize, 2, Color.White);

        var textPosition = new Vector2(holdAreaX + GameBoard.CellSize, holdAreaY - 30);
        _spriteBatch.DrawString(_scoreFont, "HOLD", textPosition, Color.White);

        if (heldTetromino != null)
            for (var y = 0; y < heldTetromino.Shape.GetLength(0); y++)
            for (var x = 0; x < heldTetromino.Shape.GetLength(1); x++)
                if (heldTetromino.Shape[y, x] != 0)
                {
                    var drawX = holdAreaX + (x + 1) * GameBoard.CellSize;
                    var drawY = holdAreaY + (y + 1) * GameBoard.CellSize;

                    _spriteBatch.Draw(_blockTexture,
                        new Rectangle(drawX, drawY, GameBoard.CellSize, GameBoard.CellSize),
                        heldTetromino.Color);
                }
    }

    private void DrawNextTetromino(Tetromino nextTetromino)
    {
        var nextAreaX = _graphics.PreferredBackBufferWidth - 7 * GameBoard.CellSize;
        var nextAreaY = 50;

        DrawBorder(nextAreaX, nextAreaY, 6 * GameBoard.CellSize, 5 * GameBoard.CellSize, 2, Color.White);

        var textPosition = new Vector2(nextAreaX + GameBoard.CellSize, nextAreaY - 30);
        _spriteBatch.DrawString(_scoreFont, "NEXT", textPosition, Color.White);

        if (nextTetromino != null)
            for (var y = 0; y < nextTetromino.Shape.GetLength(0); y++)
            for (var x = 0; x < nextTetromino.Shape.GetLength(1); x++)
                if (nextTetromino.Shape[y, x] != 0)
                {
                    var drawX = nextAreaX + (x + 1) * GameBoard.CellSize;
                    var drawY = nextAreaY + (y + 1) * GameBoard.CellSize;

                    _spriteBatch.Draw(_blockTexture,
                        new Rectangle(drawX, drawY, GameBoard.CellSize, GameBoard.CellSize),
                        nextTetromino.Color);
                }
    }

    private void DrawScore(ScoreManager scoreManager)
    {
        var x = 20;
        var y = _graphics.PreferredBackBufferHeight - 100;
        var scoreText = $"Score: {scoreManager.Score}";
        var levelText = $"Level: {scoreManager.Level}";
        var linesText = $"Lines: {scoreManager.TotalLinesCleared}";

        _spriteBatch.DrawString(_scoreFont, scoreText, new Vector2(x, y), Color.White);
        _spriteBatch.DrawString(_scoreFont, levelText, new Vector2(x, y + 30), Color.White);
        _spriteBatch.DrawString(_scoreFont, linesText, new Vector2(x, y + 60), Color.White);
    }

    public void DrawTitleScreen()
    {
        _spriteBatch.Begin();

        _spriteBatch.Draw(_pixel,
            new Rectangle(0, 0, _graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight),
            new Color(0, 0, 50));
        DrawTitleScreenDecoration(12 * 20, 20 * 20, 20);

        var titleText = "TETRIS";
        var titlePosition = new Vector2(
            _graphics.PreferredBackBufferWidth / 2 - _scoreFont.MeasureString(titleText).X / 2,
            _graphics.PreferredBackBufferHeight / 4);
        DrawOutlinedText(_spriteBatch, _scoreFont, titleText, titlePosition, Color.Cyan, Color.Blue, 3);

        string[] menuOptions =
        {
            $"[{_controls.MenuSelect.ToString().ToUpper()}]: Start",
            $"[{_controls.ShowControls.ToString().ToUpper()}]: Controls",
            $"[{_controls.MenuBack.ToString().ToUpper()}]: Exit"
        };
        for (var i = 0; i < menuOptions.Length; i++)
        {
            var position = new Vector2(
                _graphics.PreferredBackBufferWidth / 2 - _scoreFont.MeasureString(menuOptions[i]).X / 2,
                _graphics.PreferredBackBufferHeight / 2 + i * 50);
            var color = Color.Lerp(Color.White, Color.Cyan, (float)i / (menuOptions.Length - 1));
            _spriteBatch.DrawString(_scoreFont, menuOptions[i], position, color);
        }

        var versionText = "v1.0dev (c) Sascha Dierl 2025";
        var versionPosition = new Vector2(
            _graphics.PreferredBackBufferWidth - _scoreFont.MeasureString(versionText).X - 10,
            _graphics.PreferredBackBufferHeight - _scoreFont.MeasureString(versionText).Y - 10);
        _spriteBatch.DrawString(_scoreFont, versionText, versionPosition, Color.Gray);

        _spriteBatch.End();
    }

    public void DrawTitleScreenDecoration(int boardWidth, int boardHeight, int cellSize)
    {
        // Draw falling Tetrominos
        foreach (var (tetromino, position, size) in _titleScreenTetrominos)
            DrawTetrominoOnTitleScreen(tetromino, position, size, 0.5f);

        var boardX = (_graphics.PreferredBackBufferWidth - boardWidth) / 2;
        var boardY = (_graphics.PreferredBackBufferHeight - boardHeight) / 2;
        DrawBox(_spriteBatch, new Rectangle(boardX, boardY, boardWidth, boardHeight), Color.White * 0.3f, 2);

        foreach (var (tetromino, position) in _boardTetrominos)
            DrawTetrominoOnTitleScreen(tetromino, new Vector2(boardX + position.X, boardY + position.Y), cellSize,
                0.7f);
    }

    private void DrawTetrominoOnTitleScreen(Tetromino tetromino, Vector2 position, int cellSize, float alpha)
    {
        for (var y = 0; y < tetromino.Height; y++)
        for (var x = 0; x < tetromino.Width; x++)
            if (tetromino.Shape[y, x] != 0)
            {
                var destRect = new Rectangle(
                    (int)position.X + x * cellSize,
                    (int)position.Y + y * cellSize,
                    cellSize,
                    cellSize);
                _spriteBatch.Draw(_blockTexture, destRect, tetromino.Color * alpha);
            }
    }

    public void DrawGameOver(ScoreManager scoreManager, List<HighscoreData> highscores, int newHighscoreIndex = -1,
        string playerName = "", int scrollOffset = 0)
    {
        _spriteBatch.Begin();

        var screenWidth = _graphicsDevice.Viewport.Width;
        var screenHeight = _graphicsDevice.Viewport.Height;

        var pixel = new Texture2D(_graphicsDevice, 1, 1);
        pixel.SetData(new[] { Color.Black });
        _spriteBatch.Draw(pixel, new Rectangle(0, 0, screenWidth, screenHeight), new Color(0, 0, 0, 200));

        var gameOverText = "GAME OVER";
        var scoreText = $"Your Score: {scoreManager.Score}";
        var restartText = $"[{_controls.MenuBack.ToString().ToUpper()}]: return to Menu";

        var gameOverPos = new Vector2(screenWidth / 2 - _scoreFont.MeasureString(gameOverText).X / 2, 50);
        var scorePos = new Vector2(screenWidth / 2 - _scoreFont.MeasureString(scoreText).X / 2, 100);
        var restartPos = new Vector2(screenWidth / 2 - _scoreFont.MeasureString(restartText).X / 2, screenHeight - 50);

        DrawOutlinedText(_spriteBatch, _scoreFont, gameOverText, gameOverPos, Color.Red, Color.White, 2);
        _spriteBatch.DrawString(_scoreFont, scoreText, scorePos, Color.Yellow);

        if (newHighscoreIndex != -1)
        {
            var newHighscoreText = "NEW HIGHSCORE!";
            var enterNameText = "Enter your name:";
            var newHighscorePos = new Vector2(screenWidth / 2 - _scoreFont.MeasureString(newHighscoreText).X / 2, 150);
            var enterNamePos = new Vector2(screenWidth / 2 - _scoreFont.MeasureString(enterNameText).X / 2, 190);
            var nameInputPos = new Vector2(screenWidth / 2 - _scoreFont.MeasureString(playerName).X / 2, 230);

            DrawOutlinedText(_spriteBatch, _scoreFont, newHighscoreText, newHighscorePos, Color.Gold, Color.White, 1);
            _spriteBatch.DrawString(_scoreFont, enterNameText, enterNamePos, Color.White);
            _spriteBatch.DrawString(_scoreFont, playerName + "_", nameInputPos, Color.Cyan);
        }

        var titleText = "TOP SCORES";
        var titlePosition = new Vector2(screenWidth / 2 - _scoreFont.MeasureString(titleText).X / 2, 280);
        DrawOutlinedText(_spriteBatch, _scoreFont, titleText, titlePosition, Color.Gold, Color.White, 1);


        var boxPadding = 20;
        var boxWidth = 300;
        var boxHeight = 220;
        var boxX = screenWidth / 2 - boxWidth / 2;
        var boxY = 320;
        DrawBox(_spriteBatch, new Rectangle(boxX, boxY, boxWidth, boxHeight), Color.White, 2);

        // Draw scrollbar if there are more than 5 highscores
        if (highscores.Count > 5)
        {
            var scrollbarHeight = boxHeight * (5f / highscores.Count);
            var scrollbarY = boxY + (boxHeight - scrollbarHeight) * (scrollOffset / (float)(highscores.Count - 5));
            DrawBox(_spriteBatch, new Rectangle(boxX + boxWidth - 10, (int)scrollbarY, 5, (int)scrollbarHeight),
                Color.Gray, 1);
        }

        for (var i = scrollOffset; i < Math.Min(highscores.Count, scrollOffset + 5); i++)
        {
            var data = highscores[i];
            var rankText = $"{i + 1}.";
            var nameText = data.Name;
            var scoreValueText = data.Score.ToString();

            var rankPos = new Vector2(boxX + boxPadding, boxY + 20 + (i - scrollOffset) * 40);
            var namePos = new Vector2(boxX + boxPadding + 40, boxY + 20 + (i - scrollOffset) * 40);
            var highscorePos = new Vector2(boxX + boxWidth - boxPadding - _scoreFont.MeasureString(scoreValueText).X,
                boxY + 20 + (i - scrollOffset) * 40);

            var textColor = GetHighscoreColor(i);

            if (i == newHighscoreIndex)
            {
                var highlightRect = new Rectangle(
                    (int)rankPos.X - 5,
                    (int)rankPos.Y - 5,
                    boxWidth - 2 * boxPadding + 10,
                    30);
                _spriteBatch.Draw(_pixel, highlightRect, new Color(255, 255, 0, 100));
                textColor = Color.Cyan;
            }

            _spriteBatch.DrawString(_scoreFont, rankText, rankPos, textColor);
            _spriteBatch.DrawString(_scoreFont, nameText, namePos, textColor);
            _spriteBatch.DrawString(_scoreFont, scoreValueText, highscorePos, textColor);
        }

        // Draw scroll instructions if there are more than 5 highscores
        if (highscores.Count > 5)
        {
            var scrollInstructions = $"Use {_controls.MenuUp}/{_controls.MenuDown} to scroll";
            var scrollInstructionsPos =
                new Vector2(screenWidth / 2 - _scoreFont.MeasureString(scrollInstructions).X / 2,
                    boxY + boxHeight + 10);
            _spriteBatch.DrawString(_scoreFont, scrollInstructions, scrollInstructionsPos, Color.White);
        }

        _spriteBatch.DrawString(_scoreFont, restartText, restartPos, Color.White);

        _spriteBatch.End();
    }

    private Color GetHighscoreColor(int index)
    {
        switch (index)
        {
            case 0: return Color.Gold;
            case 1: return Color.Silver;
            case 2: return new Color(205, 127, 50);
            default: return Color.White;
        }
    }

    private void DrawOutlinedText(SpriteBatch spriteBatch, SpriteFont font, string text, Vector2 position, Color color,
        Color outlineColor, int thickness)
    {
        for (var i = -thickness; i <= thickness; i++)
        for (var j = -thickness; j <= thickness; j++)
            spriteBatch.DrawString(font, text, position + new Vector2(i, j), outlineColor);

        spriteBatch.DrawString(font, text, position, color);
    }

    private void DrawBox(SpriteBatch spriteBatch, Rectangle rect, Color color, int thickness)
    {
        spriteBatch.Draw(_pixel, new Rectangle(rect.X, rect.Y, rect.Width, thickness), color);
        spriteBatch.Draw(_pixel, new Rectangle(rect.X, rect.Y, thickness, rect.Height), color);
        spriteBatch.Draw(_pixel, new Rectangle(rect.X, rect.Y + rect.Height - thickness, rect.Width, thickness), color);
        spriteBatch.Draw(_pixel, new Rectangle(rect.X + rect.Width - thickness, rect.Y, thickness, rect.Height), color);
    }

    public void DrawPause()
    {
        _spriteBatch.Begin();

        _spriteBatch.Draw(_pixel,
            new Rectangle(0, 0, _graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight), Color.Black);

        var pauseText = "PAUSED";
        var continueText = $"[{_controls.Pause.ToString().ToUpper()}]: Continue";
        var menuText = $"[{_controls.MenuBack.ToString().ToUpper()}]: Menu";

        var pausePosition = new Vector2(
            _graphics.PreferredBackBufferWidth / 2 - _scoreFont.MeasureString(pauseText).X / 2,
            _graphics.PreferredBackBufferHeight / 3 + 30);

        var continuePosition = new Vector2(
            _graphics.PreferredBackBufferWidth / 2 - _scoreFont.MeasureString(continueText).X / 2,
            _graphics.PreferredBackBufferHeight / 2 + 0);

        var menuPosition = new Vector2(
            _graphics.PreferredBackBufferWidth / 2 - _scoreFont.MeasureString(menuText).X / 2,
            _graphics.PreferredBackBufferHeight / 2 + 50);

        DrawOutlinedText(_spriteBatch, _scoreFont, pauseText, pausePosition, Color.Cyan, Color.Blue, 3);

        _spriteBatch.DrawString(_scoreFont, continueText, continuePosition, Color.White);
        _spriteBatch.DrawString(_scoreFont, menuText, menuPosition, Color.White);

        var boxWidth = 300;
        var boxHeight = 220;
        var boxX = _graphics.PreferredBackBufferWidth / 2 - boxWidth / 2;
        var boxY = _graphics.PreferredBackBufferHeight / 2 - boxHeight / 2;
        DrawBox(_spriteBatch, new Rectangle(boxX, boxY, boxWidth, boxHeight), Color.White, 2);

        _spriteBatch.End();
    }


    public void DrawSelect(int selectedLevel, int maxLevel, int selectedSongIndex, int startIndex, int maxSongIndex)
    {
        _spriteBatch.Begin();

        _spriteBatch.Draw(_pixel,
            new Rectangle(0, 0, _graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight),
            new Color(0, 0, 50));

        DrawTitleScreenDecoration(20 * 20, 24 * 20, 20);

        var titleText = "GAME OPTIONS";
        var levelText = $"Starting Level: {selectedLevel}";
        var songText = $"Music Track: {selectedSongIndex}";
        var controlsText =
            $"{_controls.MenuUp.ToString().ToUpper()}/{_controls.MenuDown.ToString().ToUpper()}: Music   {_controls.MenuLeft.ToString().ToUpper()}/{_controls.MenuRight.ToString().ToUpper()}: Level";
        var startText = $"[{_controls.MenuSelect.ToString().ToUpper()}]: Start game";
        var backText = $"[{_controls.MenuBack.ToString().ToUpper()}]: Back";


        var titlePosition = new Vector2(
            _graphics.PreferredBackBufferWidth / 2 - _scoreFont.MeasureString(titleText).X / 2,
            100);

        var levelPosition = new Vector2(
            _graphics.PreferredBackBufferWidth / 2 - _scoreFont.MeasureString(levelText).X / 2,
            200);

        var musicPosition = new Vector2(
            _graphics.PreferredBackBufferWidth / 2 - _scoreFont.MeasureString(songText).X / 2,
            270);

        var controlsPosition = new Vector2(
            _graphics.PreferredBackBufferWidth / 2 - _scoreFont.MeasureString(controlsText).X / 2,
            370);

        var startPosition = new Vector2(
            _graphics.PreferredBackBufferWidth / 2 - _scoreFont.MeasureString(startText).X / 2,
            450);

        var backPosition = new Vector2(
            _graphics.PreferredBackBufferWidth / 2 - _scoreFont.MeasureString(backText).X / 2,
            500);

        // Draw title
        DrawOutlinedText(_spriteBatch, _scoreFont, titleText, titlePosition, Color.Cyan, Color.Blue, 2);

        // Draw options
        DrawSelectionArrows(levelPosition, levelText, selectedLevel > 1, selectedLevel < maxLevel);
        DrawSelectionArrows(musicPosition, songText, selectedSongIndex > startIndex, selectedSongIndex < maxSongIndex);

        // Draw controls info
        _spriteBatch.DrawString(_scoreFont, controlsText, controlsPosition, Color.Yellow);

        // Draw action texts
        _spriteBatch.DrawString(_scoreFont, startText, startPosition, Color.Green);
        _spriteBatch.DrawString(_scoreFont, backText, backPosition, Color.Red);

        _spriteBatch.End();
    }

    private void DrawSelectionArrows(Vector2 position, string text, bool canDecrease, bool canIncrease)
    {
        var leftArrow = "<";
        var rightArrow = ">";
        var leftArrowSize = _scoreFont.MeasureString(leftArrow);
        var rightArrowSize = _scoreFont.MeasureString(rightArrow);

        var leftArrowPosition = new Vector2(
            position.X - leftArrowSize.X - 20,
            position.Y);

        var rightArrowPosition = new Vector2(
            position.X + _scoreFont.MeasureString(text).X + 20,
            position.Y);

        var leftArrowColor = canDecrease ? Color.White : Color.Gray;
        var rightArrowColor = canIncrease ? Color.White : Color.Gray;

        _spriteBatch.DrawString(_scoreFont, text, position, Color.White);

        _spriteBatch.DrawString(_scoreFont, leftArrow, leftArrowPosition, leftArrowColor);
        _spriteBatch.DrawString(_scoreFont, rightArrow, rightArrowPosition, rightArrowColor);
    }

    public void DrawControls()
    {
        _spriteBatch.Begin();
        _spriteBatch.Draw(_pixel,
            new Rectangle(0, 0, _graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight),
            new Color(0, 0, 50));

        var boardWidth = 20 * 20;
        var boardHeight = 24 * 20;
        var cellSize = 20;
        DrawTitleScreenDecoration(boardWidth, boardHeight, cellSize);

        var titleText = "GAME CONTROLS";

        var controls = new List<string>
        {
            $"Left/Right: {_controls.MoveLeft.ToString().ToUpper()}/{_controls.MoveRight.ToString().ToUpper()}",
            $"Hold: {_controls.HoldPiece.ToString().ToUpper()}",
            $"Soft Drop: {_controls.SoftDrop.ToString().ToUpper()}",
            $"Hard Drop: {_controls.HardDrop.ToString().ToUpper()}",
            $"Rotate Clockwise: {_controls.RotateClockwise.ToString().ToUpper()}",
            $"Rotate Counterclockwise: {_controls.RotateCounterClockwise.ToString().ToUpper()}",
            $"Reload Tetromino: {_controls.DebugReload.ToString().ToUpper()}",
            $"Pause: {_controls.Pause.ToString().ToUpper()}",
            $"Back: {_controls.MenuBack.ToString().ToUpper()}",
            $"Select: {_controls.MenuSelect.ToString().ToUpper()}"
        };

        var titlePosition = new Vector2(
            _graphics.PreferredBackBufferWidth / 2 - _scoreFont.MeasureString(titleText).X / 2,
            100);
        DrawOutlinedText(_spriteBatch, _scoreFont, titleText, titlePosition, Color.Cyan, Color.Blue, 2);

        var borderX = (_graphics.PreferredBackBufferWidth - boardWidth) / 2;
        var borderY = (_graphics.PreferredBackBufferHeight - boardHeight) / 2;

        var yOffset = borderY + 60;
        var xOffset = borderX + 20;
        foreach (var control in controls)
        {
            _spriteBatch.DrawString(_scoreFont, control, new Vector2(xOffset, yOffset), Color.White);
            yOffset += 30;
        }

        var backToMenuText = $"[{_controls.MenuBack.ToString().ToUpper()}]: Back";
        var backToMenuPosition = new Vector2(
            _graphics.PreferredBackBufferWidth / 2 - _scoreFont.MeasureString(backToMenuText).X / 2,
            borderY + boardHeight - 40);
        _spriteBatch.DrawString(_scoreFont, backToMenuText, backToMenuPosition, Color.Yellow);

        _spriteBatch.End();
    }
}