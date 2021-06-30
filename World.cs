using Godot;
using System;

public class World : Node2D
{
    public bool GameOver = false;
    private Sprite GameOverLabel { get; set; }
    private Grid Grid { get; set; }
    private Player Player { get; set; }
    

    public override void _Ready()
    {
        Grid = new Grid(7, GetViewportRect().Size);
        AddChild(Grid);
        Player = new Player();
        AddChild(Player);
        CallDeferred("GetChildReferences");
    }

    public override void _Process(float delta)
    {
        if (GameOver)
        {
            MoveChild(GameOverLabel, GetChildCount());
            GameOverLabel.Visible = true;
        }
    }

    public void GetChildReferences()
    {
        GameOverLabel = GetNode<Sprite>("GameOverLabel");
    }

}
