using System;
using Godot;

public enum Type
{
    Blank,
    Goal,
    Score,
    Jump
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
                LilySprite.Texture = GD.Load<Texture>("res://Game/Resources/32x32/LilyPad1_32x32.png");
                var rand = new RandomNumberGenerator();
                rand.Randomize();
                //Sprite.Rotation = rand.RandfRange(0, (float)Math.PI*2);
                PointValue = 0;
                JumpLength = 0;
            }
            else if (_type == Type.Goal)
            {
                LilySprite.Texture = GD.Load<Texture>("res://Game/Resources/32x32/LilyPad2_32x32.png");
                LilySprite.Rotation = 0;
                PointValue = 50;
                JumpLength = 0;
            }
            else if (_type == Type.Score)
            {
                LilySprite.Texture = GD.Load<Texture>("res://Game/Resources/32x32/LilyPad1_32x32.png");
                LilySprite.Rotation = 0;
                PointValue = 100;
                JumpLength = 0;
            }
            else if (_type == Type.Jump)
            {
                LilySprite.Texture = GD.Load<Texture>("res://Game/Resources/32x32/LilyPad3_32x32.png");
                LilySprite.Rotation = 0;
                PointValue = 0;
                JumpLength = 2;
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
            LilySprite.Position = (_gridPosition * Size) + (Size/2);
        }
    }
    public Vector2 Size;
    public Vector2 GridOffset;
    public Grid Grid;
    public Sprite LilySprite;
    public Sprite WaterSprite;
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
    public int JumpLength;

    public Tile(){}   

    public virtual void BuildTile(Type type, Vector2 size, Vector2 gridPosition)
    {
        Size = size;
        Label = new Counter(Size);
        Label.BbcodeEnabled = true;
        
        LilySprite = new Sprite();
        Type = type;
        GridPosition = gridPosition;

        WaterSprite = new Sprite();
        WaterSprite.Position = LilySprite.Position;
        WaterSprite.Texture = GD.Load<Texture>("res://Game/Resources/32x32/Water1_32x32.png");
        AddChild(WaterSprite);

        Grid = GetNode<Grid>("/root/World/Grid");
        AddChild(LilySprite);

        Label.RectPosition = LilySprite.Position + new Vector2(-Label.RectSize.x/2, Label.RectSize.y/10);
        AddChild(Label);


    }
}