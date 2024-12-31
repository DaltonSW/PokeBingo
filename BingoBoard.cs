using System;
using System.Collections.Generic;
using Godot;

public partial class BingoBoard : VBoxContainer {
    [Export(PropertyHint.Dir)] private string _spriteDir = "res://Sprites/";

    private GridContainer _board;

    [Export] public int Seed { get; private set; }
    private int _lastSeed;
    private Random _random;

    private LineEdit _seedEdit;

    private PokeBingo _parent;
    // Called when the node enters the scene tree for the first time.
    public override void _Ready() {
        _parent = GetTree().GetRoot().GetNode<PokeBingo>("PokeBingo");
        Seed = Guid.NewGuid().GetHashCode();
        _board = GetNode<GridContainer>("Board");
        _seedEdit = GetNode<LineEdit>("SettingsVBox/SeedHBox/LineEdit");
        GD.Print("Initial seed generated: "+Seed);
    }
    
    private void GenerateBoard() {
        HandleSeed();

        using var spriteDir = DirAccess.Open(_spriteDir);
        if (spriteDir == null) return;
        GD.Print("Opened dir "+_spriteDir);

        var sprites = spriteDir.GetFiles();
        GD.Print("Number of files in dir: "+sprites.Length);
        var usedNums = new List<int>();
        
        // For each button...
        foreach (var node in _board.GetChildren()) {
            GD.Print($"On {node.Name}");
            var button = (BingoButton)node;
            
            var spritePath = "";
            // try and grab a unique sprite path
            while (spritePath == "") {
                var fileIdx = _random.Next(sprites.Length);
                var fileName = sprites[fileIdx];
                
                GD.Print("- File index chosen: "+fileIdx);
                GD.Print("- File located there: " + fileName);
                
                // If already used, skip
                if (usedNums.Contains(fileIdx)) {
                    GD.Print("- File index already used, continuing");
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
        if (!int.TryParse(_seedEdit.Text, out var parseSeed) || parseSeed == _lastSeed) {
            Seed = Guid.NewGuid().GetHashCode();
        }
        else {
            Seed = parseSeed;
        }
        _random = new Random(Seed);
        _seedEdit.Text = Seed.ToString();
        GD.Print("Generated new Random w/ seed " + Seed);
    }

    private void ClearBoard() {
        foreach (var node in _board.GetChildren()) {
            var button = (BingoButton)node;
            button.Unpress();
        }
    }

    private void OnGeneratePressed() => GenerateBoard();

    private void OnClearPressed() => ClearBoard();
}
