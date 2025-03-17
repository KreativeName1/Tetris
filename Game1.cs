using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Tetris;

public class Game1 : Game
{
    private List<Tetromino> _blockList = Tetromino.ReadFromJSON();
    
    private GameState _currentState = GameState.TitleScreen;
    
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private Texture2D _blockTexture;
    private SpriteFont _scoreFont;

    private double speed = 0.5;
    private double _timeSinceLastFall;
    
    private float _moveTimer = 0f;
    private const float MoveCooldown = 0.1f;
    
    private float _rotateTimer = 0f;
    private const float RotateCooldown = 0.2f;
    
    private const int GridWidth = 10;
    private const int GridHeight = 20;
    private const int CellSize = 30;
    private Color[,] _grid;
    
    private int _score = 0;
    private int _level = 1;
    private int _totalLinesCleared = 0;

    private Tetromino _currentTetromino;
    private Random _random;
    
    private Tetromino _heldTetromino;
    private bool _canHold = true;
    
    private Tetromino _nextTetromino;
    
    private bool _isGameOver = false;

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    
        _graphics.PreferredBackBufferWidth = GridWidth * CellSize + 500;
        _graphics.PreferredBackBufferHeight = GridHeight * CellSize + 40;
        _graphics.ApplyChanges();
    }

    protected override void Initialize()
    {
        _grid = new Color[GridHeight, GridWidth];
        _random = new Random();
        _currentTetromino = GetRandomTetromino();
        _timeSinceLastFall = 0;
        
        _heldTetromino = null;
        _nextTetromino = GetRandomTetromino();

        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        _blockTexture = Content.Load<Texture2D>("block");
        _scoreFont = Content.Load<SpriteFont> ("ScoreFont");
    }
    
    protected override void Update(GameTime gameTime)
    {
        var keyboardState = Keyboard.GetState();

        if (keyboardState.IsKeyDown(Keys.Escape))
            Exit();

        switch (_currentState)
        {
            case GameState.TitleScreen:
                if (keyboardState.IsKeyDown(Keys.Enter))
                {
                    _currentState = GameState.Game;
                    ResetGame();
                }
                break;

            case GameState.Game:
                UpdateGame(gameTime);
                break;

            case GameState.GameOver:
                if (keyboardState.IsKeyDown(Keys.Enter))
                {
                    _currentState = GameState.Game;
                    ResetGame();
                }
                break;
        }

        base.Update(gameTime);
    }
    
    protected void UpdateGame(GameTime gameTime)
    {
        var keyboardState = Keyboard.GetState();
        
        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
        _timeSinceLastFall += deltaTime;
        _moveTimer += deltaTime;
        _rotateTimer += deltaTime;
    
    
        // Handle player input
        if (_moveTimer >= MoveCooldown)
        {
            if (keyboardState.IsKeyDown(Keys.Left))
            {
                MoveTetromino(-1, 0);
                _moveTimer = 0f;
            }
            else if (keyboardState.IsKeyDown(Keys.Right))
            {
                MoveTetromino(1, 0);
                _moveTimer = 0f;
            }
            
            if (keyboardState.IsKeyDown(Keys.Down))
            {
                if (MoveTetromino(0, 1))
                {
                    _timeSinceLastFall = 0;
                    _moveTimer = 0f;
                }
            }
            
            if (keyboardState.IsKeyDown(Keys.Space))
            {
                HoldTetromino();
            }
        }
    
        // Handle rotation
        if (_rotateTimer >= RotateCooldown)
        {
            if (keyboardState.IsKeyDown(Keys.Up))
            {
                RotateTetromino();
                _rotateTimer = 0f;
            }
        }
    
        // Regular falling
        if (_timeSinceLastFall >= speed)
        {
            _timeSinceLastFall = 0;
            if (!MoveTetromino(0, 1))
            {
                PlaceTetromino();
                GetNextTeromino();
            }
        }
    
    }
    
    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Black);

        switch (_currentState)
        {
            case GameState.TitleScreen:
                DrawTitleScreen();
                break;

            case GameState.Game:
                DrawGame();
                break;

            case GameState.GameOver:
                DrawGame();
                DrawGameOver();
                break;
        }

        base.Draw(gameTime);
    }

    protected void DrawGame() {
    
        _spriteBatch.Begin();
        int borderThickness = 2;
        int gameAreaOffsetX = 250;
        int gameAreaOffsetY = 20;
        DrawBorder(gameAreaOffsetX, gameAreaOffsetY, GridWidth * CellSize, GridHeight * CellSize, borderThickness, Color.White);
        _spriteBatch.End();
        
        _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, Matrix.CreateTranslation(gameAreaOffsetX, gameAreaOffsetY, 0));
        DrawGrid();
        DrawTetromino(_currentTetromino);
        DrawHeldTetromino();
        DrawNextTetromino();
        _spriteBatch.End();
        
        _spriteBatch.Begin();
        DrawScore(gameAreaOffsetX + GridWidth * CellSize + 20, gameAreaOffsetY);
        _spriteBatch.End();
    }
    
    private void DrawBorder(int x, int y, int width, int height, int thickness, Color color)
    {
        // top line
        _spriteBatch.Draw(_blockTexture, new Rectangle(x - thickness, y - thickness, width + thickness * 2, thickness), color);
        // bottom line
        _spriteBatch.Draw(_blockTexture, new Rectangle(x - thickness, y + height, width + thickness * 2, thickness), color);
        // left line
        _spriteBatch.Draw(_blockTexture, new Rectangle(x - thickness, y - thickness, thickness, height + thickness * 2), color);
        // right line
        _spriteBatch.Draw(_blockTexture, new Rectangle(x + width, y - thickness, thickness, height + thickness * 2), color);
    }
    
    private void DrawScore(int x, int y)
    {
        string scoreText = $"Score: {_score}";
        string levelText = $"Level: {_level}";
        string linesText = $"Lines: {_totalLinesCleared}";
    
        _spriteBatch.DrawString(_scoreFont, scoreText, new Vector2(x, y), Color.White);
        _spriteBatch.DrawString(_scoreFont, levelText, new Vector2(x, y + 30), Color.White);
        _spriteBatch.DrawString(_scoreFont, linesText, new Vector2(x, y + 60), Color.White);
    }
    
    protected void DrawGrid()
    {
        for (int y = 0; y < GridHeight; y++)
        {
            for (int x = 0; x < GridWidth; x++)
            {
                Color cellColor = _grid[y, x];
                if (cellColor != Color.Transparent)
                {
                    _spriteBatch.Draw(_blockTexture, 
                        new Rectangle(x * CellSize, y * CellSize, CellSize, CellSize), 
                        cellColor);
                }
            }
        }
    }
    private void DrawHeldTetromino()
         {
             int holdAreaX = -7 * CellSize;
             int holdAreaY = 2 * CellSize;
         
             DrawBorder(holdAreaX, holdAreaY, 6 * CellSize, 5 * CellSize, 2, Color.White);
         
             Vector2 textPosition = new Vector2(holdAreaX + CellSize, holdAreaY - 30);
             _spriteBatch.DrawString(_scoreFont, "HOLD", textPosition, Color.White);
         
             if (_heldTetromino != null)
             {
                 for (int y = 0; y < _heldTetromino.Shape.GetLength(0); y++)
                 {
                     for (int x = 0; x < _heldTetromino.Shape.GetLength(1); x++)
                     {
                         if (_heldTetromino.Shape[y, x] != 0)
                         {
                             int drawX = holdAreaX + (x + 1) * CellSize;
                             int drawY = holdAreaY + (y + 1) * CellSize;
         
                             _spriteBatch.Draw(_blockTexture, 
                                 new Rectangle(drawX, drawY, CellSize, CellSize), 
                                 _heldTetromino.Color);
                         }
                     }
                 }
             }
         }
    protected void DrawTetromino(Tetromino tetromino)
    {
        for (int y = 0; y < tetromino.Shape.GetLength(0); y++)
        {
            for (int x = 0; x < tetromino.Shape.GetLength(1); x++)
            {
                if (tetromino.Shape[y, x] != 0)
                {
                    int drawX = (int)tetromino.Position.X + x;
                    int drawY = (int)tetromino.Position.Y + y;
                    
                    if (drawX >= 0 && drawX < GridWidth && drawY >= 0 && drawY < GridHeight)
                    {
                        _spriteBatch.Draw(_blockTexture, 
                            new Rectangle(drawX * CellSize, drawY * CellSize, CellSize, CellSize), 
                            tetromino.Color);
                    }
                }
            }
        }
    }

    protected void DrawGameOver()
    {
        string gameOverText = "GAME OVER!";
        // center it in the grid
        Vector2 textPosition = new Vector2(GridWidth * CellSize / 2 - _scoreFont.MeasureString(gameOverText).X / 2, GridHeight * CellSize / 2 - _scoreFont.MeasureString(gameOverText).Y / 2);
        _spriteBatch.DrawString(_scoreFont, gameOverText, textPosition, Color.White);
        // add score, and show that it can be restarted and closed
        string restartText = "Press ENTER to restart";
        textPosition = new Vector2(GridWidth * CellSize / 2 - _scoreFont.MeasureString(restartText).X / 2, GridHeight * CellSize / 2 + _scoreFont.MeasureString(restartText).Y / 2);
        _spriteBatch.DrawString(_scoreFont, restartText, textPosition, Color.White);
        string closeText = "Press ESCAPE to close";
        textPosition = new Vector2(GridWidth * CellSize / 2 - _scoreFont.MeasureString(closeText).X / 2, GridHeight * CellSize / 2 + _scoreFont.MeasureString(closeText).Y / 2 + 30);
        _spriteBatch.DrawString(_scoreFont, closeText, textPosition, Color.White);
        string scoreText = $"Your Score: {_score}";
        textPosition = new Vector2(GridWidth * CellSize / 2 - _scoreFont.MeasureString(scoreText).X / 2, GridHeight * CellSize / 2 + _scoreFont.MeasureString(scoreText).Y / 2 + 60);
        _spriteBatch.DrawString(_scoreFont, scoreText, textPosition, Color.White);
    }

    private void DrawNextTetromino()
    {
        int nextAreaX = -7 * CellSize;
        int nextAreaY = 10 * CellSize;
    
        DrawBorder(nextAreaX, nextAreaY, 6 * CellSize, 5 * CellSize, 2, Color.White);
    
        Vector2 textPosition = new Vector2(nextAreaX + CellSize, nextAreaY - 30);
        _spriteBatch.DrawString(_scoreFont, "NEXT", textPosition, Color.White);
    
        if (_nextTetromino != null)
        {
            for (int y = 0; y < _nextTetromino.Shape.GetLength(0); y++)
            {
                for (int x = 0; x < _nextTetromino.Shape.GetLength(1); x++)
                {
                    if (_nextTetromino.Shape[y, x] != 0)
                    {
                        int drawX = nextAreaX + (x + 1) * CellSize;
                        int drawY = nextAreaY + (y + 1) * CellSize;
    
                        _spriteBatch.Draw(_blockTexture, 
                            new Rectangle(drawX, drawY, CellSize, CellSize), 
                            _nextTetromino.Color);
                    }
                }
            }
        }
    }
    
    
    
    private void RotateTetromino()
    {
        int[,] originalShape = _currentTetromino.Shape.Clone() as int[,];
        int originalRows = _currentTetromino.Shape.GetLength(0);
        int originalCols = _currentTetromino.Shape.GetLength(1);
        Vector2 originalPosition = _currentTetromino.Position;
    
        _currentTetromino.Rotate();
    
        int newRows = _currentTetromino.Shape.GetLength(0);
        int newCols = _currentTetromino.Shape.GetLength(1);
        Vector2 adjustment = new Vector2(
            (originalCols - newCols) / 2f,
            (originalRows - newRows) / 2f
        );
        _currentTetromino.Position += adjustment;
    
        if (IsCollision())
        {
            // If there's a collision, try to adjust the position
            for (int xOffset = -1; xOffset <= 1; xOffset++)
            {
                for (int yOffset = 0; yOffset <= 1; yOffset++)
                {
                    _currentTetromino.Position = originalPosition + adjustment + new Vector2(xOffset, yOffset);
                    if (!IsCollision())
                    {
                        return;
                    }
                }
            }
    
            _currentTetromino.Shape = originalShape;
            _currentTetromino.Position = originalPosition;
            _currentTetromino.Rotation = (Rotation)(((int)_currentTetromino.Rotation + 3) % 4);
        }
    }

    

    private Tetromino GetRandomTetromino()
    {
        if (_blockList.Count == 0) throw new InvalidOperationException("No tetrominos available in the block list.");

        int index = _random.Next(_blockList.Count);
        Tetromino selectedTetromino = _blockList[index];

        Tetromino newTetromino = new Tetromino(selectedTetromino.Shape, selectedTetromino.Color)
        {
            Position = new Vector2(GridWidth / 2 - selectedTetromino.Shape.GetLength(1) / 2, 0)
        };

        return newTetromino;
    }

    private bool MoveTetromino(int deltaX, int deltaY)
    {
        _currentTetromino.Position += new Vector2(deltaX, deltaY);
        if (IsCollision())
        {
            _currentTetromino.Position -= new Vector2(deltaX, deltaY);
            return false;
        }
        return true;
    }

    private bool IsCollision()
    {
        for (int y = 0; y < _currentTetromino.Shape.GetLength(0); y++)
        {
            for (int x = 0; x < _currentTetromino.Shape.GetLength(1); x++)
            {
                if (_currentTetromino.Shape[y, x] != 0)
                {
                    int gridX = (int)_currentTetromino.Position.X + x;
                    int gridY = (int)_currentTetromino.Position.Y + y;

                    if (gridX < 0 || gridX >= GridWidth || gridY >= GridHeight)
                        return true;

                    if (gridY >= 0 && _grid[gridY, gridX] != Color.Transparent)
                        return true;
                }
            }
        }
        return false;
    }

    private void PlaceTetromino()
    {
        for (int y = 0; y < _currentTetromino.Shape.GetLength(0); y++)
        {
            for (int x = 0; x < _currentTetromino.Shape.GetLength(1); x++)
            {
                if (_currentTetromino.Shape[y, x] != 0)
                {
                    int gridX = (int)_currentTetromino.Position.X + x;
                    int gridY = (int)_currentTetromino.Position.Y + y;
    
                    if (gridX >= 0 && gridX < GridWidth && gridY >= 0 && gridY < GridHeight)
                    {
                        _grid[gridY, gridX] = _currentTetromino.Color;
                        
                        if (gridY <= 0)
                        {
                            _isGameOver = true;
                            return;
                        }
                    }
                }
            }

            _canHold = true;
        }
    
        int linesCleared = ClearCompletedLines();
        UpdateScore(linesCleared);
        IncreaseSpeed(linesCleared);
    }

    private void HoldTetromino()
    {
        if (!_canHold) return;

        if (_heldTetromino == null)
        {
            _heldTetromino = new Tetromino(_currentTetromino.Shape, _currentTetromino.Color);
            GetNextTeromino();
        }
        else
        {
            Tetromino temp = _currentTetromino;
            _currentTetromino = new Tetromino(_heldTetromino.Shape, _heldTetromino.Color)
            {
                Position = new Vector2(GridWidth / 2 - _heldTetromino.Shape.GetLength(1) / 2, 0)
            };
            _heldTetromino = new Tetromino(temp.Shape, temp.Color);
        }

        _canHold = false;
    }
    
    
    
    private int ClearCompletedLines()
    {
        int linesCleared = 0;
        for (int y = GridHeight - 1; y >= 0; y--)
        {
            if (IsLineComplete(y))
            {
                ClearLine(y);
                ShiftLinesDown(y);
                linesCleared++;
                y++; // Recheck the same row
            }
        }
        return linesCleared;
    }

    private bool IsLineComplete(int y)
    {
        for (int x = 0; x < GridWidth; x++)
        {
            if (_grid[y, x] == Color.Transparent)
            {
                return false;
            }
        }
        return true;
    }

    private void ClearLine(int y)
    {
        for (int x = 0; x < GridWidth; x++)
        {
            _grid[y, x] = Color.Transparent;
        }
    }

    private void ShiftLinesDown(int clearedLine)
    {
        for (int y = clearedLine; y > 0; y--)
        {
            for (int x = 0; x < GridWidth; x++)
            {
                _grid[y, x] = _grid[y - 1, x];
            }
        }
        // Clear the top line
        for (int x = 0; x < GridWidth; x++)
        {
            _grid[0, x] = Color.Transparent;
        }
    }
    
    private void UpdateScore(int linesCleared)
    {
        if (linesCleared > 0)
        {
            int baseScore = 10;
            
            int scoreIncrease = (int)(baseScore * Math.Pow(2, linesCleared - 1) * _level);
            
            _score += scoreIncrease;
            _totalLinesCleared += linesCleared;
            _level = (_totalLinesCleared / 10) + 1;
        }
    }

    private void IncreaseSpeed(int linesCleared)
    {
        if (linesCleared > 0) speed = Math.Max(0.1, 0.5 - (_level - 1) * 0.05);
    }

    private void GetNextTeromino()
    {
        _currentTetromino = new Tetromino(_nextTetromino);
        _nextTetromino = GetRandomTetromino();
    }
    
    private void ResetGame()
    {
        _grid = new Color[GridHeight, GridWidth];
        _currentTetromino = GetRandomTetromino();
        _nextTetromino = GetRandomTetromino();
        _heldTetromino = null;
        _canHold = true;
        _score = 0;
        _level = 1;
        _totalLinesCleared = 0;
        speed = 0.5;
        _isGameOver = false;
        _currentState = GameState.Game;
    }
    
    
    private void DrawTitleScreen()
    {
        string titleText = "TETRIS";
        string startText = "Press ENTER to start";
        string exitText = "Press ESC to exit";

        Vector2 titlePosition = new Vector2(
            _graphics.PreferredBackBufferWidth / 2 - _scoreFont.MeasureString(titleText).X / 2,
            _graphics.PreferredBackBufferHeight / 3);

        Vector2 startPosition = new Vector2(
            _graphics.PreferredBackBufferWidth / 2 - _scoreFont.MeasureString(startText).X / 2,
            _graphics.PreferredBackBufferHeight / 2);

        Vector2 exitPosition = new Vector2(
            _graphics.PreferredBackBufferWidth / 2 - _scoreFont.MeasureString(exitText).X / 2,
            _graphics.PreferredBackBufferHeight / 2 + 50);

        _spriteBatch.Begin();
        _spriteBatch.DrawString(_scoreFont, titleText, titlePosition, Color.White);
        _spriteBatch.DrawString(_scoreFont, startText, startPosition, Color.White);
        _spriteBatch.DrawString(_scoreFont, exitText, exitPosition, Color.White);
        _spriteBatch.End();
    }
}