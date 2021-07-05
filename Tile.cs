using System;
using Godot;

public enum Type
{
    Blank,
    Goal,
    Score
}

public class Tile : Node2D
{
    private Type _type;
    public Type Type
    {
        get { return _type; }
        set{
            _type = value;
            if (_type == Type.Blank)
            {
                Sprite.Texture = GD.Load<Texture>("res://BlackSquare.png");
            }
            else if (_type == Type.Goal)
            {
                Sprite.Texture = GD.Load<Texture>("res://WhiteSquare.png");
            }
            else if (_type == Type.Score)
            {
                Sprite.Texture = GD.Load<Texture>("res://Coin.png");
            }
        }
    }
    private Vector2 _gridPosition;
    public Vector2 GridPosition
    { 
        get { return _gridPosition; }
        set
        {
            _gridPosition = value;
            Sprite.Position = (_gridPosition * Size) + GridOffset + (Size/2);
        }
    }
    public Vector2 Size;
    public Vector2 GridOffset;
    public Grid Grid;
    public Sprite Sprite;

    public Tile()
    {

    }   

    public void BuildTile(Type type, Vector2 size, Vector2 offset, Vector2 gridPosition)
    {
        Grid = GetNode<Grid>("/root/World/Grid");
        Size = size;
        GridOffset = offset;
        Sprite = new Sprite();
        Sprite.Scale = new Vector2(0.95f,0.95f);
        Grid.AddChild(Sprite);
        GridPosition = gridPosition;
        Type = type;
    }
}