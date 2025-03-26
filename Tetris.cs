using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Tetris;

public class Tetris : Game
{
    private GraphicsDeviceManager _graphics;
    private GameState _currentState = GameState.TitleScreen;
    private GameBoard _gameBoard;
    private Tetromino _currentTetromino;
    private Tetromino _heldTetromino;
    private Tetromino _nextTetromino;
    private ScoreManager _scoreManager;
    private InputManager _inputManager;
    private AudioManager _audioManager;
    private Renderer _renderer;
    
    
    private double speed = 0.5;
    private double _timeSinceLastFall;
    
    private float _moveTimer = 0f;
    private const float MoveCooldown = 0.1f;
    
    private float _rotateTimer = 0f;
    private const float RotateCooldown = 0.2f;
    
    private Random _random;
    private bool _canHold = true;

    private int musicIndex = 2;

    public Tetris()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;

        _graphics.PreferredBackBufferWidth = GameBoard.GridWidth * 30 + 500;
        _graphics.PreferredBackBufferHeight = GameBoard.GridHeight * 30 + 40;
        _graphics.ApplyChanges();
    }

    protected override void Initialize()
    {
        _gameBoard = new GameBoard();
        _scoreManager = new ScoreManager();
        _inputManager = new InputManager();
        _audioManager = new AudioManager();
        _renderer = new Renderer(GraphicsDevice, _graphics);
        _random = new Random();

        _currentTetromino = Tetromino.GetRandomTetromino();
        _nextTetromino = Tetromino.GetRandomTetromino();

        base.Initialize();
    }

    protected override void LoadContent()
    {
        _renderer.LoadContent(Content);
        _audioManager.LoadContent(Content);

    }
    
    protected override void Update(GameTime gameTime)
    {
        _inputManager.Update();

        switch (_currentState)
        {
            case GameState.TitleScreen:
                UpdateTitleScreen();
                break;
            case GameState.Game:
                UpdateGame(gameTime);
                break;
            case GameState.GameOver:
                UpdateGameOver();
                break;
            case GameState.Pause:
                UpdatePause();
                break;
            case GameState.Select:
                UpdateSelect();
                break;
        }

        base.Update(gameTime);
    }

    private void UpdateSelect()
    {
        if (_inputManager.IsKeyPressed(Keys.Enter))
        {
            _currentState = GameState.Game;
            ResetGame();
        }
        else if (_inputManager.IsKeyPressed(Keys.Escape))
        {
            _currentState = GameState.TitleScreen;
        }
    }
    
    private void UpdateTitleScreen()
    {
        _audioManager.PlayBackgroundMusic(0, true);
        if (_inputManager.IsKeyPressed(Keys.Enter))
        {
            _currentState = GameState.Select;
        }
        else if (_inputManager.IsKeyPressed(Keys.Escape))
        {
            Exit();
        }
    }

    private void UpdateGameOver()
    {
        _audioManager.PlayBackgroundMusic(1, true, false);
        if (_inputManager.IsKeyPressed(Keys.Enter))
        {
            _currentState = GameState.Game;
            ResetGame();
        }
        else if (_inputManager.IsKeyPressed(Keys.Escape))
        {
            _currentState = GameState.TitleScreen;
        }
    }

    private void UpdatePause()
    {
        if (_inputManager.IsKeyPressed(Keys.Enter))
        {
            _currentState = GameState.Game;
        }
        else if (_inputManager.IsKeyPressed(Keys.Delete))
        {
            _currentState = GameState.TitleScreen;
        }
    } 
    
    private void UpdateGame(GameTime gameTime)
    {
        _audioManager.PlayBackgroundMusic(3, true);
        
        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
        _timeSinceLastFall += deltaTime;
        _moveTimer += deltaTime;
        _rotateTimer += deltaTime;

        // Handle player input
        if (_moveTimer >= MoveCooldown)
        {
            if (_inputManager.IsKeyDown(Keys.Left))
            {
                MoveTetromino(-1, 0);
                _moveTimer = 0f;
            }
            else if (_inputManager.IsKeyDown(Keys.Right))
            {
                MoveTetromino(1, 0);
                _moveTimer = 0f;
            }
            
            if (_inputManager.IsKeyDown(Keys.Down))
            {
                if (MoveTetromino(0, 1))
                {
                    _timeSinceLastFall = 0;
                    _moveTimer = 0f;
                }
            }
            
            if (_inputManager.IsKeyPressed(Keys.Space))
            {
                HoldTetromino();
            }
        }

        // Handle rotation
        if (_rotateTimer >= RotateCooldown)
        {
            if (_inputManager.IsKeyPressed(Keys.Up))
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

        if (_inputManager.IsKeyPressed(Keys.Escape))
        {
            _currentState = GameState.Pause;
        }
    }
    
    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Black);

        switch (_currentState)
        {
            case GameState.TitleScreen:
                _renderer.DrawTitleScreen();
                break;
            case GameState.Game:
                _renderer.DrawGame(_gameBoard, _currentTetromino, _heldTetromino, _nextTetromino, _scoreManager);
                break;
            case GameState.GameOver:
                _renderer.DrawGameOver(_scoreManager);
                break;
            case GameState.Pause:
                _renderer.DrawPause();
                break;
            case GameState.Select:
                _renderer.DrawSelect();
                break;
        }

        base.Draw(gameTime);
    }

    private void RotateTetromino()
    {
        _audioManager.PlayRotateSound();
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
    
        if (_gameBoard.IsCollision(_currentTetromino))
        {
            // If there's a collision, try to adjust the position
            for (int xOffset = -1; xOffset <= 1; xOffset++)
            {
                for (int yOffset = 0; yOffset <= 1; yOffset++)
                {
                    _currentTetromino.Position = originalPosition + adjustment + new Vector2(xOffset, yOffset);
                    if (!_gameBoard.IsCollision(_currentTetromino))
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

    private bool MoveTetromino(int deltaX, int deltaY)
    {
        _currentTetromino.Position += new Vector2(deltaX, deltaY);
        if (_gameBoard.IsCollision(_currentTetromino))
        {
            _currentTetromino.Position -= new Vector2(deltaX, deltaY);
            return false;
        }
        return true;
    }

    private void PlaceTetromino()
    {
        _audioManager.PlayLand();
        _gameBoard.PlaceTetromino(_currentTetromino);
        int linesCleared = _gameBoard.ClearCompletedLines();
        if (linesCleared > 0) _audioManager.PlayLineClear();
        int oldLevel = _scoreManager.Level;
        
            _scoreManager.UpdateScore(linesCleared);
        if (oldLevel != _scoreManager.Level) _audioManager.PlayLevelUp();
        speed = _scoreManager.GetSpeed();

        if (_gameBoard.IsGameOver)
        {
            _currentState = GameState.GameOver;
        }
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
                Position = new Vector2(GameBoard.GridWidth / 2 - _heldTetromino.Shape.GetLength(1) / 2, 0)
            };
            _heldTetromino = new Tetromino(temp.Shape, temp.Color);
        }

        _canHold = false;
    }

    private void GetNextTeromino()
    {
        _currentTetromino = _nextTetromino;
        _nextTetromino = Tetromino.GetRandomTetromino();
        _canHold = true;
    }
    
    private void ResetGame()
    {
        _gameBoard.Reset();
        _scoreManager.Reset();
        _currentTetromino = Tetromino.GetRandomTetromino();
        _nextTetromino = Tetromino.GetRandomTetromino();
        _heldTetromino = null;
        _canHold = true;
        speed = 0.5;
        _currentState = GameState.Game;
        _audioManager.PlayBackgroundMusic(1);
    }
}