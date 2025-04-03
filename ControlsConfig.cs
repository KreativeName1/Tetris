using System.Collections.Generic;

namespace Tetris;

public class ControlsConfig
{
    public Dictionary<string, string> KeyboardControls { get; set; } = new Dictionary<string, string>();
    public Dictionary<string, string> GamepadControls { get; set; } = new Dictionary<string, string>();
}