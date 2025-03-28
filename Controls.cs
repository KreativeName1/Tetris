using Microsoft.Xna.Framework.Input;

namespace Tetris;

public class Controls
{
    public Keys MoveLeft { get; set; } = Keys.Left;
    public Keys MoveRight { get; set; } = Keys.Right;
    public Keys SoftDrop { get; set; } = Keys.Down;
    public Keys HardDrop { get; set; } = Keys.Space;
    
    public Keys RotateClockwise { get; set; } = Keys.Up;
    public Keys RotateCounterClockwise { get; set; } = Keys.Z;
    
    public Keys HoldPiece { get; set; } = Keys.C;
    
    public Keys Pause { get; set; } = Keys.P;
    public Keys Quit { get; set; } = Keys.Escape;
    
    public Keys MenuUp { get; set; } = Keys.Up;
    public Keys MenuDown { get; set; } = Keys.Down;
    public Keys MenuLeft { get; set; } = Keys.Left;
    public Keys MenuRight { get; set; } = Keys.Right;
    public Keys MenuSelect { get; set; } = Keys.Enter;
    public Keys MenuBack { get; set; } = Keys.Escape;
    public Keys DebugReload { get; set; } = Keys.R;
    public Keys ShowControls { get; set; } = Keys.F1;
}