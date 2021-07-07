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
                PointValue = 0;
            }
            else if (_type == Type.Goal)
            {
                Sprite.Texture = GD.Load<Texture>("res://WhiteSquare.png");
                PointValue = 50;
            }
            else if (_type == Type.Score)
            {
                Sprite.Texture = GD.Load<Texture>("res://Coin.png");
                PointValue = 100;
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
    //public Grid Grid;
    public Sprite Sprite;
    public int PointValue;
    public Counter Label;

    public Tile(){}   

    public void BuildTile(Type type, Vector2 size, Vector2 offset, Vector2 gridPosition)
    {
        Sprite = new Sprite();
        Sprite.Scale = new Vector2(0.95f,0.95f);

        Size = size;
        GridOffset = offset;
        Type = type;
        GridPosition = gridPosition;

        //Grid = GetNode<Grid>("/root/World/Grid");
        //AddChild(Sprite);

        Label = new Counter();
        //Label.SetPosition(Position);
        Label.Text = PointValue.ToString();
        AddChild(Label);
    }
}