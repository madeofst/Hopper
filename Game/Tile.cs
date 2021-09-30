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
                Sprite.Texture = GD.Load<Texture>("res://Game/Resources/LilyPad1.png");
                var rand = new RandomNumberGenerator();
                rand.Randomize();
                Sprite.Rotation = rand.RandfRange(0, (float)Math.PI*2);
                PointValue = 0;
            }
            else if (_type == Type.Goal)
            {
                Sprite.Texture = GD.Load<Texture>("res://Game/Resources/LilyPad2.png");
                Sprite.Rotation = 0;
                PointValue = 50;
            }
            else if (_type == Type.Score)
            {
                Sprite.Texture = GD.Load<Texture>("res://Game/Resources/LilyPadCoin.png");
                Sprite.Rotation = 0;
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
            Sprite.Position = (_gridPosition * Size) + (Size/2);
        }
    }
    public Vector2 Size;
    public Vector2 GridOffset;
    public Grid Grid;
    public Sprite Sprite;
    private int _PointValue;
    public int PointValue
    {
        get { return _PointValue; }
        set
        {
            _PointValue = value;
            if (_type == Type.Score)
            {
                Label.BbcodeText = $"[center]{_PointValue.ToString()}[/center]";
            }
            else
            {
                Label.Text = "";
            }
        }
    }
    public Counter Label;

    public Tile(){}   

    public void BuildTile(Type type, Vector2 size, Vector2 gridPosition)
    {
        Size = size;
        Label = new Counter(Size);
        Label.BbcodeEnabled = true;

        Sprite = new Sprite();
        //Sprite.Scale = new Vector2(0.95f,0.95f);

        //GridOffset = offset;
        Type = type;
        GridPosition = gridPosition;

        Grid = GetNode<Grid>("/root/World/Grid");
        AddChild(Sprite);

        Label.RectPosition = Sprite.Position + new Vector2(-Label.RectSize.x/2, Label.RectSize.y/10);
        AddChild(Label);
    }
}