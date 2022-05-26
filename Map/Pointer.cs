using Godot;
using System;

public class Pointer : Node2D
{
    private Vector2 Target;
    private Godot.Collections.Array Locations;
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        Locations = GetNode<Map>("..").Locations; 
        Position = GetNode<Location>("../Start").Position;
        Target = Position;
    }

    public void UpdateTarget(Vector2 targetPosition)
    {
        Target = targetPosition;
    }

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(float delta)
    {
        Position = Position.MoveToward(Target,  delta * 800);
    }

    public override void _Input(InputEvent @event)
    {
        if (@event.IsActionPressed("ui_left"))
        {   
/*             foreach (Location l in Locations)
            {
                //if (Position - l.Position) //TODO: find if position is in region and if so move to it
            } */
            Target = GetNode<Location>("../World1").Position;
        }
        else if (@event.IsActionPressed("ui_right"))
        {
            Target = GetNode<Location>("../World2").Position;
        }
        else if (@event.IsActionPressed("ui_down"))
        {
            Target = GetNode<Location>("../Start").Position;
        }
    }
}
