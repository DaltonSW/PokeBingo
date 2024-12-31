using Godot;
using System;

public partial class BingoButton : AspectRatioContainer {
    private Button _button;

    private TextureRect _sprite;
    
    // Called when the node enters the scene tree for the first time.
    public override void _Ready() {
        _button = GetNode<Button>("Button");
        _sprite = _button.GetNode<TextureRect>("Margin/PokeSprite");
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }

    public void SetPicture(string filepath) {
        var image = GD.Load<Image>(filepath);
        GD.Print($"Trying to set picture located at {filepath} on {Name}");
        var texture = new ImageTexture();
        texture.SetImage(image);
        GD.Print("Texture successfully loaded");
        _sprite.Texture = texture;
        GD.Print("Sprite texture successfully set");
    }

    public void Unpress() {
        _button.ButtonPressed = false;
    }

    private void OnButtonToggled(bool toggled) {
        _sprite.Modulate = toggled ? new Color(1, 1, 1, 0.33f) : Colors.White;
    }
}
