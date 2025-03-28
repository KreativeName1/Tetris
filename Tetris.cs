using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Tetris;

public class Tetris : Game
{
    private const float MoveCooldown = 0.125f;
    private const float RotateCooldown = 0.2f;

    private const float KeyPressCooldown = 0.2f;
    private AudioManager _audioManager;
    private bool _canHold = true;
    private Controls _controls;
    private GameState _currentState = GameState.TitleScreen;
    private Tetromino _currentTetromino;
    private GameBoard _gameBoard;
    private readonly GraphicsDeviceManager _graphics;
    private Tetromino _heldTetromino;
    private int _highscoreScrollOffset;
    private InputManager _inputManager;
    private float _keyPressTimer;

    private float _moveTimer;
    private Tetromino _nextTetromino;

    private Random _random;
    private Renderer _renderer;

    private float _rotateTimer;
    private ScoreManager _scoreManager;
    private double _timeSinceLastFall;
    private bool isEnteringName;


    private int musicIndex = 2;

    private string playerName = "";

    private int selLevel = 1;
    private readonly int selMaxLevel = 10;
    private int selMusic = 1;


    private double speed = 0.5;

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
        _controls = new Controls();
        _renderer = new Renderer(GraphicsDevice, _graphics, _controls);
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

        if (_inputManager.IsKeyPressed(_controls.DebugReload)) Tetromino.BlockList = Tetromino.ReadFromJSON();

        switch (_currentState)
        {
            case GameState.TitleScreen:
                UpdateTitleScreen();
                break;
            case GameState.Game:
                UpdateGame(gameTime);
                break;
            case GameState.GameOver:
                UpdateGameOver(gameTime);
                break;
            case GameState.Pause:
                UpdatePause();
                break;
            case GameState.Select:
                UpdateSelect();
                break;
            case GameState.Controls:
                UpdateControls();
                break;
        }

        base.Update(gameTime);
    }

    private void UpdateSelect()
    {
        if (_inputManager.IsKeyPressed(_controls.MenuLeft))
        {
            selLevel--;
            if (selLevel < 1) selLevel = 1;
        }
        else if (_inputManager.IsKeyPressed(_controls.MenuRight))
        {
            selLevel++;
            if (selLevel > selMaxLevel) selLevel = selMaxLevel;
        }
        else if (_inputManager.IsKeyPressed(_controls.MenuDown))
        {
            selMusic--;
            if (selMusic < _audioManager.MinMusicIndex) selMusic = _audioManager.MinMusicIndex;
        }
        else if (_inputManager.IsKeyPressed(_controls.MenuUp))
        {
            selMusic++;
            if (selMusic > _audioManager.MaxMusicIndex) selMusic = _audioManager.MaxMusicIndex;
        }

        if (_inputManager.IsKeyPressed(_controls.MenuSelect))
        {
            ResetGame();
            _scoreManager.Level = selLevel;
            _currentState = GameState.Game;
        }
        else if (_inputManager.IsKeyPressed(_controls.MenuBack))
        {
            _currentState = GameState.TitleScreen;
        }

        _audioManager.PlayBackgroundMusic(selMusic, true);
    }

    private void UpdateTitleScreen()
    {
        _audioManager.PlayBackgroundMusic(1, true);
        if (_inputManager.IsKeyPressed(_controls.MenuSelect)) _currentState = GameState.Select;
        else if (_inputManager.IsKeyPressed(_controls.Quit)) Exit();
        else if (_inputManager.IsKeyPressed(_controls.ShowControls)) _currentState = GameState.Controls;
    }

    private void UpdateGameOver(GameTime gameTime)
    {
        _audioManager.PlayBackgroundMusic(0, true, false);

        _keyPressTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;

        if (isEnteringName)
        {
            if (_keyPressTimer >= KeyPressCooldown)
            {
                var pressedKeys = _inputManager.GetPressedKeys();
                if (pressedKeys.Length > 0)
                {
                    var key = pressedKeys[0]; // Consider only the first pressed key

                    if (key >= Keys.A && key <= Keys.Z && playerName.Length < 10)
                    {
                        playerName += key.ToString();
                        _keyPressTimer = 0f;
                    }
                    else if (key == Keys.Back && playerName.Length > 0)
                    {
                        playerName = playerName.Substring(0, playerName.Length - 1);
                        _keyPressTimer = 0f;
                    }
                    else if (key == Keys.Enter && playerName.Length > 0)
                    {
                        HighscoreManager.SaveHighscore(new HighscoreData(playerName, _scoreManager.Score,
                            _scoreManager.Level, _scoreManager.TotalLinesCleared));
                        isEnteringName = false;
                        _currentState = GameState.TitleScreen;
                        _highscoreScrollOffset = 0;
                        _keyPressTimer = 0f;
                    }
                }
            }
        }
        else
        {
            if (_keyPressTimer >= KeyPressCooldown && _inputManager.IsKeyPressed(_controls.MenuBack))
            {
                _currentState = GameState.TitleScreen;
                _highscoreScrollOffset = 0;
                _keyPressTimer = 0f;
            }

            if (_keyPressTimer >= KeyPressCooldown && _inputManager.IsKeyPressed(_controls.MenuUp) &&
                _highscoreScrollOffset > 0)
            {
                _highscoreScrollOffset--;
                _keyPressTimer = 0f;
            }
            else if (_keyPressTimer >= KeyPressCooldown && _inputManager.IsKeyPressed(_controls.MenuDown) &&
                     _highscoreScrollOffset < HighscoreManager.LoadHighscores().Count - 5)
            {
                _highscoreScrollOffset++;
                _keyPressTimer = 0f;
            }
        }
    }

    private void UpdatePause()
    {
        if (!_audioManager.IsPaused) _audioManager.Pause();
        if (_inputManager.IsKeyPressed(_controls.Pause))
        {
            _audioManager.PlayPause();
            _audioManager.Resume();
            _currentState = GameState.Game;
        }
        else if (_inputManager.IsKeyPressed(_controls.MenuBack))
        {
            _currentState = GameState.TitleScreen;
        }
    }

    private void UpdateControls()
    {
        if (_inputManager.IsKeyPressed(_controls.MenuBack)) _currentState = GameState.TitleScreen;
    }

    private void UpdateGame(GameTime gameTime)
    {
        _audioManager.PlayBackgroundMusic(selMusic, true);

        var deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
        _timeSinceLastFall += deltaTime;
        _moveTimer += deltaTime;
        _rotateTimer += deltaTime;

        // Handle player input
        if (_moveTimer >= MoveCooldown)
        {
            if (_inputManager.IsKeyDown(_controls.MoveLeft))
            {
                MoveTetromino(-1, 0);
                _moveTimer = 0f;
            }
            else if (_inputManager.IsKeyDown(_controls.MoveRight))
            {
                MoveTetromino(1, 0);
                _moveTimer = 0f;
            }

            if (_inputManager.IsKeyDown(_controls.SoftDrop))
                if (MoveTetromino(0, 1))
                {
                    _timeSinceLastFall = 0;
                    _moveTimer = 0f;
                }

            if (_inputManager.IsKeyPressed(_controls.HardDrop))
            {
                while (MoveTetromino(0, 1))
                {
                }

                _audioManager.PlayHardDrop();
                PlaceTetromino();
                GetNextTeromino();
            }

            if (_inputManager.IsKeyPressed(_controls.HoldPiece)) HoldTetromino();
        }

        // Handle rotation
        if (_rotateTimer >= RotateCooldown)
        {
            if (_inputManager.IsKeyPressed(_controls.RotateClockwise))
            {
                RotateTetromino();
                _rotateTimer = 0f;
            }
            else if (_inputManager.IsKeyPressed(_controls.RotateCounterClockwise))
            {
                RotateTetromino(false);
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

        if (_inputManager.IsKeyPressed(_controls.Pause))
        {
            _audioManager.PlayPause();
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
                var highscores = HighscoreManager.LoadHighscores();
                var newHighscoreIndex = HighscoreManager.GetNewHighscoreIndex(_scoreManager.Score);
                if (HighscoreManager.IsNewHighscore(_scoreManager.Score))
                {
                    var newHighscore = new HighscoreData(playerName, _scoreManager.Score, _scoreManager.Level,
                        _scoreManager.TotalLinesCleared);
                    highscores.Insert(newHighscoreIndex, newHighscore);
                }

                _renderer.DrawGameOver(_scoreManager, highscores, newHighscoreIndex, isEnteringName ? playerName : null,
                    _highscoreScrollOffset);
                break;
            case GameState.Pause:
                _renderer.DrawPause();
                break;
            case GameState.Select:
                _renderer.DrawSelect(selLevel, selMaxLevel, selMusic, _audioManager.MinMusicIndex,
                    _audioManager.MaxMusicIndex);
                break;
            case GameState.Controls:
                _renderer.DrawControls();
                break;
        }

        base.Draw(gameTime);
    }

    private void RotateTetromino(bool clockwise = true)
    {
        _audioManager.PlayRotateSound();
        var originalShape = _currentTetromino.Shape.Clone() as int[,];
        var originalRows = _currentTetromino.Shape.GetLength(0);
        var originalCols = _currentTetromino.Shape.GetLength(1);
        var originalPosition = _currentTetromino.Position;
        var originalRotation = _currentTetromino.Rotation;

        _currentTetromino.Rotate(clockwise);

        var newRows = _currentTetromino.Shape.GetLength(0);
        var newCols = _currentTetromino.Shape.GetLength(1);
        var adjustment = new Vector2(
            (originalCols - newCols) / 2f,
            (originalRows - newRows) / 2f
        );
        _currentTetromino.Position += adjustment;

        if (_gameBoard.IsCollision(_currentTetromino))
        {
            // If there's a collision, try to adjust the position
            for (var xOffset = -1; xOffset <= 1; xOffset++)
            for (var yOffset = 0; yOffset <= 1; yOffset++)
            {
                _currentTetromino.Position = originalPosition + adjustment + new Vector2(xOffset, yOffset);
                if (!_gameBoard.IsCollision(_currentTetromino)) return;
            }

            // If no valid position found, revert the rotation
            _currentTetromino.Shape = originalShape;
            _currentTetromino.Position = originalPosition;
            _currentTetromino.Rotation = originalRotation;
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
        var linesCleared = _gameBoard.ClearCompletedLines();
        if (linesCleared > 0) _audioManager.PlayLineClear();
        var oldLevel = _scoreManager.Level;

        _scoreManager.UpdateScore(linesCleared);
        if (oldLevel != _scoreManager.Level) _audioManager.PlayLevelUp();
        speed = _scoreManager.GetSpeed();

        if (_gameBoard.IsGameOver)
        {
            _currentState = GameState.GameOver;
            if (HighscoreManager.IsNewHighscore(_scoreManager.Score))
            {
                isEnteringName = true;
                playerName = "";
            }
            else
            {
                isEnteringName = false;
            }
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
            var temp = _currentTetromino;
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