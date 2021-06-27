using Godot;
using System;

public class World : Node2D
{

    private Grid Grid { get; set; }
    private Player Player { get; set; }
    

    public override void _Ready()
    {
        Grid = new Grid(7, GetViewportRect().Size);
        AddChild(Grid);
        Player = new Player();
        AddChild(Player);
    }

}
