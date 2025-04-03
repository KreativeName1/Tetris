using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Tetris;

public class InputManager
{
    private KeyboardState _currentKeyboardState;
    private KeyboardState _previousKeyboardState;
    
    private GamePadState _currentGamePadState;
    private GamePadState _previousGamePadState;

    public void Update()
    {
        _previousKeyboardState = _currentKeyboardState;
        _currentKeyboardState = Keyboard.GetState();

        _previousGamePadState = _currentGamePadState;
        _currentGamePadState = GamePad.GetState(PlayerIndex.One);
    }

    public bool IsKeyPressed(Keys key)
    {
        return _currentKeyboardState.IsKeyDown(key) && !_previousKeyboardState.IsKeyDown(key);
    }

    public bool IsKeyDown(Keys key)
    {
        return _currentKeyboardState.IsKeyDown(key);
    }

    public Keys[] GetPressedKeys()
    {
        return _currentKeyboardState.GetPressedKeys();
    }
    
    public bool IsButtonPressed(Buttons button)
    {
        return _currentGamePadState.IsButtonDown(button) && !_previousGamePadState.IsButtonDown(button);
    }

    public bool IsButtonDown(Buttons button)
    {
        return _currentGamePadState.IsButtonDown(button);
    }

    public bool IsGamePadConnected()
    {
        return _currentGamePadState.IsConnected;
    }

    public bool IsAnyInput(Keys key, Buttons button)
    {
        return IsKeyDown(key) || (IsGamePadConnected() && IsButtonDown(button));
    }

    public bool IsAnyInputPressed(Keys key, Buttons button)
    {
        return IsKeyPressed(key) || (IsGamePadConnected() && IsButtonPressed(button));
    }
}