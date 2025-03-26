using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;

namespace Tetris
{
public class Renderer
    {
        private SpriteBatch _spriteBatch;
        private Texture2D _blockTexture;
        private SpriteFont _scoreFont;
        private GraphicsDevice _graphicsDevice;
        private GraphicsDeviceManager _graphics;

        // Define offsets for the game field
        private const int GameFieldOffsetX = 250;
        private const int GameFieldOffsetY = 20;

        public Renderer(GraphicsDevice graphicsDevice, GraphicsDeviceManager graphics)
        {
            _graphicsDevice = graphicsDevice;
            _graphics = graphics;
            _spriteBatch = new SpriteBatch(graphicsDevice);
        }

        public void LoadContent(Microsoft.Xna.Framework.Content.ContentManager content)
        {
            _blockTexture = content.Load<Texture2D>("block");
            _scoreFont = content.Load<SpriteFont>("ScoreFont");
        }

        public void DrawGame(GameBoard gameBoard, Tetromino currentTetromino, Tetromino heldTetromino, Tetromino nextTetromino, ScoreManager scoreManager)
        {
            _graphicsDevice.Clear(Color.Black);

            _spriteBatch.Begin();

            // Draw game board border
            DrawBorder(GameFieldOffsetX, GameFieldOffsetY, GameBoard.GridWidth * GameBoard.CellSize, GameBoard.GridHeight * GameBoard.CellSize, 2, Color.White);

            // Draw game board
            for (int y = 0; y < GameBoard.GridHeight; y++)
            {
                for (int x = 0; x < GameBoard.GridWidth; x++)
                {
                    Color cellColor = gameBoard.Grid[y, x];
                    if (cellColor != Color.Transparent)
                    {
                        _spriteBatch.Draw(_blockTexture, 
                            new Rectangle(GameFieldOffsetX + x * GameBoard.CellSize, GameFieldOffsetY + y * GameBoard.CellSize, GameBoard.CellSize, GameBoard.CellSize), 
                            cellColor);
                    }
                }
            }

            // Draw current tetromino
            DrawTetromino(currentTetromino, GameFieldOffsetX, GameFieldOffsetY);

            // Draw held tetromino
            DrawHeldTetromino(heldTetromino);

            // Draw next tetromino
            DrawNextTetromino(nextTetromino);

            // Draw score
            DrawScore(scoreManager);

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

        private void DrawTetromino(Tetromino tetromino, int offsetX, int offsetY)
        {
            for (int y = 0; y < tetromino.Shape.GetLength(0); y++)
            {
                for (int x = 0; x < tetromino.Shape.GetLength(1); x++)
                {
                    if (tetromino.Shape[y, x] != 0)
                    {
                        int drawX = (int)tetromino.Position.X + x;
                        int drawY = (int)tetromino.Position.Y + y;
                    
                        if (drawX >= 0 && drawX < GameBoard.GridWidth && drawY >= 0 && drawY < GameBoard.GridHeight)
                        {
                            _spriteBatch.Draw(_blockTexture, 
                                new Rectangle(offsetX + drawX * GameBoard.CellSize, offsetY + drawY * GameBoard.CellSize, GameBoard.CellSize, GameBoard.CellSize), 
                                tetromino.Color);
                        }
                    }
                }
            }
        }

        private void DrawHeldTetromino(Tetromino heldTetromino)
        {
            int holdAreaX = 20;
            int holdAreaY = 50;
         
            DrawBorder(holdAreaX, holdAreaY, 6 * GameBoard.CellSize, 5 * GameBoard.CellSize, 2, Color.White);
         
            Vector2 textPosition = new Vector2(holdAreaX + GameBoard.CellSize, holdAreaY - 30);
            _spriteBatch.DrawString(_scoreFont, "HOLD", textPosition, Color.White);
         
            if (heldTetromino != null)
            {
                for (int y = 0; y < heldTetromino.Shape.GetLength(0); y++)
                {
                    for (int x = 0; x < heldTetromino.Shape.GetLength(1); x++)
                    {
                        if (heldTetromino.Shape[y, x] != 0)
                        {
                            int drawX = holdAreaX + (x + 1) * GameBoard.CellSize;
                            int drawY = holdAreaY + (y + 1) * GameBoard.CellSize;
         
                            _spriteBatch.Draw(_blockTexture, 
                                new Rectangle(drawX, drawY, GameBoard.CellSize, GameBoard.CellSize), 
                                heldTetromino.Color);
                        }
                    }
                }
            }
        }

        private void DrawNextTetromino(Tetromino nextTetromino)
        {
            int nextAreaX = _graphics.PreferredBackBufferWidth - 7 * GameBoard.CellSize;
            int nextAreaY = 50;
    
            DrawBorder(nextAreaX, nextAreaY, 6 * GameBoard.CellSize, 5 * GameBoard.CellSize, 2, Color.White);
    
            Vector2 textPosition = new Vector2(nextAreaX + GameBoard.CellSize, nextAreaY - 30);
            _spriteBatch.DrawString(_scoreFont, "NEXT", textPosition, Color.White);
    
            if (nextTetromino != null)
            {
                for (int y = 0; y < nextTetromino.Shape.GetLength(0); y++)
                {
                    for (int x = 0; x < nextTetromino.Shape.GetLength(1); x++)
                    {
                        if (nextTetromino.Shape[y, x] != 0)
                        {
                            int drawX = nextAreaX + (x + 1) * GameBoard.CellSize;
                            int drawY = nextAreaY + (y + 1) * GameBoard.CellSize;
    
                            _spriteBatch.Draw(_blockTexture, 
                                new Rectangle(drawX, drawY, GameBoard.CellSize, GameBoard.CellSize), 
                                nextTetromino.Color);
                        }
                    }
                }
            }
        }

        private void DrawScore(ScoreManager scoreManager)
        {
            int x = 20;
            int y = _graphics.PreferredBackBufferHeight - 100;
            string scoreText = $"Score: {scoreManager.Score}";
            string levelText = $"Level: {scoreManager.Level}";
            string linesText = $"Lines: {scoreManager.TotalLinesCleared}";
    
            _spriteBatch.DrawString(_scoreFont, scoreText, new Vector2(x, y), Color.White);
            _spriteBatch.DrawString(_scoreFont, levelText, new Vector2(x, y + 30), Color.White);
            _spriteBatch.DrawString(_scoreFont, linesText, new Vector2(x, y + 60), Color.White);
        }

        public void DrawTitleScreen()
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

        public void DrawGameOver(ScoreManager scoreManager) 
        {
            _spriteBatch.Begin();
            string restartText = "Press ENTER to restart";
            string closeText = "Press ESCAPE to go to menu";
            string gameOverText = "GAME OVER!";
        
            Vector2 gameOverPos = new Vector2(_graphicsDevice.Viewport.Width / 2 - _scoreFont.MeasureString(gameOverText).X / 2, 200);
            _spriteBatch.DrawString(_scoreFont, gameOverText, gameOverPos, Color.White);
            Vector2 textPosition =
                new Vector2(_graphicsDevice.Viewport.Width / 2 - _scoreFont.MeasureString(restartText).X / 2, 250);
            _spriteBatch.DrawString(_scoreFont, restartText, textPosition, Color.White);
            Vector2 closeTextPos =
                new Vector2(_graphicsDevice.Viewport.Width / 2 - _scoreFont.MeasureString(closeText).X / 2, 310);
            _spriteBatch.DrawString(_scoreFont, closeText, closeTextPos, Color.White);
            _spriteBatch.End();
        }

        public void DrawPause()
        {
            _spriteBatch.Begin();
            string pauseText = "PAUSED";
            string restartText = "Press ENTER to restart";
            string closeText = "Press DELETE to go to menu";
            Vector2 gameOverPos = new Vector2(_graphicsDevice.Viewport.Width / 2 - _scoreFont.MeasureString(pauseText).X / 2,
                200);
            _spriteBatch.DrawString(_scoreFont, pauseText, gameOverPos, Color.White);
            Vector2 textPosition =
                new Vector2(_graphicsDevice.Viewport.Width / 2 - _scoreFont.MeasureString(restartText).X / 2, 250);
            _spriteBatch.DrawString(_scoreFont, restartText, textPosition, Color.White);
            Vector2 closeTextPos =
                new Vector2(_graphicsDevice.Viewport.Width / 2 - _scoreFont.MeasureString(closeText).X / 2, 310);
            _spriteBatch.DrawString(_scoreFont, closeText, closeTextPos, Color.White);
            _spriteBatch.End();
        }


        public void DrawSelect()
        {
            int selectedLevel = 1;
            int maxLevel = 10;
            int selectedSongIndex = 0;
            
            _spriteBatch.Begin();

            string titleText = "GAME OPTIONS";
            string levelText = $"Starting Level: {selectedLevel}";
            string controlsText = "UP/DOWN: Level, LEFT/RIGHT: Change Song";
            string startText = "Press ENTER to start game";
            string backText = "Press ESC to go back";

            Vector2 titlePosition = new Vector2(
                _graphics.PreferredBackBufferWidth / 2 - _scoreFont.MeasureString(titleText).X / 2,
                100);

            Vector2 levelPosition = new Vector2(
                _graphics.PreferredBackBufferWidth / 2 - _scoreFont.MeasureString(levelText).X / 2,
                200);


            Vector2 controlsPosition = new Vector2(
                _graphics.PreferredBackBufferWidth / 2 - _scoreFont.MeasureString(controlsText).X / 2,
                400);

            Vector2 startPosition = new Vector2(
                _graphics.PreferredBackBufferWidth / 2 - _scoreFont.MeasureString(startText).X / 2,
                450);

            Vector2 backPosition = new Vector2(
                _graphics.PreferredBackBufferWidth / 2 - _scoreFont.MeasureString(backText).X / 2,
                500);

            _spriteBatch.DrawString(_scoreFont, titleText, titlePosition, Color.White);
            _spriteBatch.DrawString(_scoreFont, levelText, levelPosition, Color.White);
            _spriteBatch.DrawString(_scoreFont, controlsText, controlsPosition, Color.Yellow);
            _spriteBatch.DrawString(_scoreFont, startText, startPosition, Color.Green);
            _spriteBatch.DrawString(_scoreFont, backText, backPosition, Color.Red);

            // Draw level selection arrows
            DrawSelectionArrows(levelPosition, levelText, selectedLevel > 1, selectedLevel < maxLevel);


            _spriteBatch.End();
        }

        private void DrawSelectionArrows(Vector2 position, string text, bool canDecrease, bool canIncrease)
        {
            string leftArrow = "<";
            string rightArrow = ">";
            Vector2 leftArrowSize = _scoreFont.MeasureString(leftArrow);
            Vector2 rightArrowSize = _scoreFont.MeasureString(rightArrow);

            Vector2 leftArrowPosition = new Vector2(
                position.X - leftArrowSize.X - 20,
                position.Y);

            Vector2 rightArrowPosition = new Vector2(
                position.X + _scoreFont.MeasureString(text).X + 20,
                position.Y);

            Color leftArrowColor = canDecrease ? Color.White : Color.Gray;
            Color rightArrowColor = canIncrease ? Color.White : Color.Gray;

            _spriteBatch.DrawString(_scoreFont, leftArrow, leftArrowPosition, leftArrowColor);
            _spriteBatch.DrawString(_scoreFont, rightArrow, rightArrowPosition, rightArrowColor);
        }
    }
}