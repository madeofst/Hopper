using Godot;
using System;
using System.Collections.Generic;

public class Map : Node2D
{
    public Godot.Collections.Array Locations;
    
    public override void _Ready()
    {
        Locations = GetChildren();
        for (int i = 0; i < Locations.Count; i++)
        {
            if (Locations[i].GetType() != typeof(Location))
            {
                Locations.Remove(Locations[i]);
            }
            else
            {
                GD.Print(Locations[i].GetType());
            }
        }
    }
}