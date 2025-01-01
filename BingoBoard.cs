using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Godot;

public partial class BingoBoard : VBoxContainer {
    [Export(PropertyHint.Dir)] private string _spriteDir = "res://Sprites";

    private const string LastStatePath = "user://lastState.cfg";

    [Export] private GridContainer _board;
    private LineEdit _seedEdit;

    private int Seed { get; set; }
    private int _lastSeed;
    private Random _random;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready() {
        _seedEdit = GetNode<LineEdit>("SettingsVBox/SeedHBox/LineEdit");
        TryLoadState();
    }
    
    public override void _Notification(int what) {
        if (what != NotificationWMCloseRequest) return;

        SaveState();
        
        GetTree().Quit();
    }

    private void TryLoadState() {
        var inState = new ConfigFile();
        var error = inState.Load(LastStatePath);
        if (error != Error.Ok) {
            Seed = Guid.NewGuid().GetHashCode();
            GD.Print("Initial seed generated: "+Seed);
            return;
        }

        Seed = (int)inState.GetValue("General", "Seed", Guid.NewGuid().GetHashCode());

        var boardChildren = _board.GetChildren();

        for (var index = 0; index < boardChildren.Count; index++) {
            var childButton = (BingoButton)boardChildren[index];
            childButton.IsPressed = (bool)inState.GetValue("Buttons", index.ToString(), false);
            childButton.SetPicture((string)inState.GetValue("Sprites", index.ToString(), "res://Egg.png"));
        }

        _seedEdit.Text = Seed == 0 ? "" : Seed.ToString();
    }

    private void SaveState() {
        var outState = new ConfigFile();
        outState.SetValue("General", "Seed", Seed);

        var boardChildren = _board.GetChildren();

        for (var index = 0; index < boardChildren.Count; index++) {
            var childButton = (BingoButton)boardChildren[index];
            outState.SetValue("Buttons", index.ToString(), childButton.IsPressed);
            outState.SetValue("Sprites", index.ToString(), childButton.Filepath);
        }

        outState.Save(LastStatePath);
    }
    
    private void OnGeneratePressed() {
        HandleSeed();
        OnClearPressed();

        using var spriteDir = DirAccess.Open(_spriteDir);
        if (spriteDir == null) return;
        // GD.Print("Opened dir "+_spriteDir);

        var sprites = spriteDir.GetFiles();
        // GD.Print("Number of files in dir: "+sprites.Length);
        var usedNums = new List<int>();
        
        // For each button...
        foreach (var node in _board.GetChildren()) {
            // GD.Print($"On {node.Name}");
            var button = (BingoButton)node;
            
            var spritePath = "";
            // try and grab a unique sprite path
            while (spritePath == "") {
                var fileIdx = _random.Next(sprites.Length);
                var fileName = sprites[fileIdx];
                
                // GD.Print("- File index chosen: "+fileIdx);
                // GD.Print("- File located there: " + fileName);
                
                // If already used, skip
                if (usedNums.Contains(fileIdx)) {
                    // GD.Print("- File index already used, continuing");
                    continue;
                }
                if (fileName.EndsWith(".import")) {
                    fileName = fileName.TrimSuffix(".import");
                }
                spritePath = fileName;
                usedNums.Add(fileIdx);
            }
            
            button.SetPicture($"{_spriteDir}/{spritePath}");
        }
    }

    private void HandleSeed() {
        _lastSeed = Seed;
        _seedEdit = GetNode<LineEdit>("SettingsVBox/SeedHBox/LineEdit");
        if (int.TryParse(_seedEdit.Text, out var parseSeed)) {
            GD.Print("Successfully parsed text as int: "+parseSeed);
            Seed = parseSeed;
        } else {
            if (_seedEdit.Text != "") {
                GD.Print("Successfully parsed text as string: "+_seedEdit.Text);
                using var md5 = MD5.Create();
                var inBytes = Encoding.ASCII.GetBytes(_seedEdit.Text);
                var hashBytes = md5.ComputeHash(inBytes);
                Seed = BitConverter.ToInt32(hashBytes, 0);
            } else {
                GD.Print("Generated seed from New GUID hash code");
                Seed = Guid.NewGuid().GetHashCode();
            }
        }
        
        _random = new Random(Seed);
        _seedEdit.Text = Seed.ToString();
        GD.Print("Generated new Random w/ seed " + Seed);
    }

    private void OnClearPressed() {
        foreach (var node in _board.GetChildren()) {
            var button = (BingoButton)node;
            button.IsPressed = false;
        }
    }

    private void OnResetPressed() {
        Seed = 0;
        _seedEdit.Text = "";
        foreach (var child in _board.GetChildren()) {
            var button = (BingoButton)child;
            button.IsPressed = false;
            button.SetPicture($"res://Egg.png");
        }
    }

    private void OnGuiInput(InputEvent inputEvent) {
        if (inputEvent is not InputEventMouseButton || !inputEvent.IsPressed() ) return;
        
        _seedEdit.ReleaseFocus();
    } 

}
