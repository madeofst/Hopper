using System;
using System.Collections.Generic;
using Godot;

public class LevelData : Resource
{
    [Export]
    public Type[] TileType;
    
    [Export]
    public int Number { get; set; }

}