using Godot;
using System;

public class Menu : MarginContainer
{
    public override void _Ready()
    {
    }

    public void newGamePressed()
    {
        GD.Print("New game button pressed.");
        World world = (World)GD.Load<PackedScene>("res://World.tscn").Instance();
        GetTree().Root.AddChild(world);
        Hide();
    }

    public void highScoresPressed()
    {
        GD.Print("High scores button pressed.");
    }
}
