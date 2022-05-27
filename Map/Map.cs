using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class Map : Node2D
{
    public List<Location> Locations;
    public Pointer Pointer;
    
    public override void _Ready()
    {
        Locations = GetChildren().OfType<Location>().ToList<Location>();
        Pointer = GetNode<Pointer>("Pointer");
        Pointer.SetLocations(Locations);
    }

    public override void _Process(float delta)
    {
        foreach (Location l in Locations)
        {
            if (Pointer.Position == l.Position)
            {
                l.Scale = new Vector2(1.2f, 1.2f);
            }
            else
            {
                l.Scale = new Vector2(1f, 1f);
            }
        }
    }
}