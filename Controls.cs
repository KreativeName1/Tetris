using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework.Input;
using Newtonsoft.Json;

namespace Tetris;

public class Controls
{
    private readonly string _configFilePath;
    private ControlsConfig _config;

    public Controls(string configFilePath = "controls.json")
    {
        _configFilePath = configFilePath;
        LoadFromConfigFile();
    }
    
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


    public Buttons GamepadDebugReload { get; set; } = Buttons.RightStick;
    public Buttons GamepadMoveLeft { get; set; } = Buttons.DPadLeft;
    public Buttons GamepadMoveRight { get; set; } = Buttons.DPadRight;
    public Buttons GamepadSoftDrop { get; set; } = Buttons.DPadDown;
    public Buttons GamepadHardDrop { get; set; } = Buttons.A;
    public Buttons GamepadRotateClockwise { get; set; } = Buttons.B;
    public Buttons GamepadRotateCounterClockwise { get; set; } = Buttons.X;
    public Buttons GamepadHoldPiece { get; set; } = Buttons.Y;
    public Buttons GamepadPause { get; set; } = Buttons.Start;
    public Buttons GamepadQuit { get; set; } = Buttons.Back;
    public Buttons GamepadMenuUp { get; set; } = Buttons.DPadUp;
    public Buttons GamepadMenuDown { get; set; } = Buttons.DPadDown;
    public Buttons GamepadMenuLeft { get; set; } = Buttons.DPadLeft;
    public Buttons GamepadMenuRight { get; set; } = Buttons.DPadRight;
    public Buttons GamepadMenuSelect { get; set; } = Buttons.A;
    public Buttons GamepadMenuBack { get; set; } = Buttons.B;
    public Buttons GamepadShowControls { get; set; } = Buttons.Y;

    
      public void LoadFromConfigFile()
    {
        if (File.Exists(_configFilePath))
        {
            string json = File.ReadAllText(_configFilePath);
            _config = JsonConvert.DeserializeObject<ControlsConfig>(json);
            ApplyConfig();
        }
        else
        {
            CreateDefaultConfig();
            SaveToConfigFile();
        }
    }

    private void CreateDefaultConfig()
    {
        _config = new ControlsConfig
        {
            KeyboardControls = new Dictionary<string, string>
            {
                {"MoveLeft", "Left"},
                {"MoveRight", "Right"},
                {"SoftDrop", "Down"},
                {"HardDrop", "Space"},
                {"RotateClockwise", "Up"},
                {"RotateCounterClockwise", "Z"},
                {"HoldPiece", "C"},
                {"Pause", "P"},
                {"Quit", "Escape"},
                {"MenuUp", "Up"},
                {"MenuDown", "Down"},
                {"MenuLeft", "Left"},
                {"MenuRight", "Right"},
                {"MenuSelect", "Enter"},
                {"MenuBack", "Escape"},
                {"DebugReload", "R"},
                {"ShowControls", "F1"}
            },
            GamepadControls = new Dictionary<string, string>
            {
                {"MoveLeft", "DPadLeft"},
                {"MoveRight", "DPadRight"},
                {"SoftDrop", "DPadDown"},
                {"HardDrop", "A"},
                {"RotateClockwise", "B"},
                {"RotateCounterClockwise", "X"},
                {"HoldPiece", "Y"},
                {"Pause", "Start"},
                {"Quit", "Back"},
                {"MenuUp", "DPadUp"},
                {"MenuDown", "DPadDown"},
                {"MenuLeft", "DPadLeft"},
                {"MenuRight", "DPadRight"},
                {"MenuSelect", "A"},
                {"MenuBack", "B"},
                {"DebugReload", "RightStick"},
                {"ShowControls", "Y"}
            }
        };
    }

    private void ApplyConfig()
    {
        foreach (var kvp in _config.KeyboardControls)
        {
            var property = GetType().GetProperty(kvp.Key);
            if (property != null && property.PropertyType == typeof(Keys))
            {
                property.SetValue(this, Enum.Parse<Keys>(kvp.Value));
            }
        }

        foreach (var kvp in _config.GamepadControls)
        {
            var property = GetType().GetProperty("Gamepad" + kvp.Key);
            if (property != null && property.PropertyType == typeof(Buttons))
            {
                property.SetValue(this, Enum.Parse<Buttons>(kvp.Value));
            }
        }
    }

    public void SaveToConfigFile()
    {
        CreateConfigFromCurrentSettings();
        string json = JsonConvert.SerializeObject(_config, Formatting.Indented);
        File.WriteAllText(_configFilePath, json);
    }

    private void CreateConfigFromCurrentSettings()
    {
        _config = new ControlsConfig
        {
            KeyboardControls = new Dictionary<string, string>(),
            GamepadControls = new Dictionary<string, string>()
        };

        foreach (var property in GetType().GetProperties())
        {
            if (property.PropertyType == typeof(Keys))
            {
                _config.KeyboardControls[property.Name] = property.GetValue(this).ToString();
            }
            else if (property.PropertyType == typeof(Buttons) && property.Name.StartsWith("Gamepad"))
            {
                _config.GamepadControls[property.Name.Substring(7)] = property.GetValue(this).ToString();
            }
        }
    }
}