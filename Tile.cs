using System;
using Godot;

public enum Type
{
    Black,
    White
}

public class Tile
{
    public Type Type;
    public Vector2 GridPosition;
    public Vector2 ScreenPosition;
    public Vector2 Size;

    //TODO: May want screen position in here
}