using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class Pointer : Node2D
{
    private Vector2 Target { 
        get; 
        set; }
    private List<Location> Locations;
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        Position = GetNode<Location>("../Start").Position;
        Target = Position;
    }

    internal void SetLocations(List<Location> locations)
    {
        Locations = locations; 
    }

    public void UpdateTarget(Vector2 targetPosition)
    {
        Target = targetPosition;
    }

    public override void _Process(float delta)
    {
        Position = Position.MoveToward(Target,  delta * 800);
    }

    public override void _Input(InputEvent @event)
    {
        Vector2 direction = Vector2.Inf;

        if (@event.IsActionPressed("ui_left"))
            direction = Vector2.Left;
        else if (@event.IsActionPressed("ui_right"))
            direction = Vector2.Right;
        else if (@event.IsActionPressed("ui_down"))
            direction = Vector2.Down;
        else if (@event.IsActionPressed("ui_up"))
            direction = Vector2.Up;

        if (direction != Vector2.Inf)
        {
            Location l = GetFirstLocationInDirection(direction);
            if (l != null) Target = l.Position;
        } 
    }

    private Location GetFirstLocationInDirection(Vector2 direction)
    {
        List<float> dotProducts = new List<float>();
        Location target = null;
        foreach (Location l in Locations)
        {
            float a = Position.DirectionTo(l.Position).Dot(direction);
            dotProducts.Add(a);
        }
        float max = dotProducts.Max();
        if (max > 0.2) target = Locations[dotProducts.IndexOf(max)];
        return target;
    }
}
