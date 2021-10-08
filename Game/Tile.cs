using System;
using Godot;

public enum Type
{
    Blank,
    Goal,
    Score,
    Jump
}

public class Tile : Area2D
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

    public CollisionShape2D CollisionShape;

    public Tile(){}   
    
    public Tile(string Name)
    {
        this.Name = Name;
    }   

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
        WaterSprite.Owner = this;

        Grid = GetNode<Grid>("/root/World/Grid");
        AddChild(LilySprite);
        LilySprite.Owner = this;

        Label.RectPosition = LilySprite.Position + new Vector2(-Label.RectSize.x/2, Label.RectSize.y/10);
        AddChild(Label);
        Label.MouseFilter = Control.MouseFilterEnum.Ignore;
        Label.Owner = this;
        
        CollisionShape = new CollisionShape2D();
        AddChild(CollisionShape);
        CollisionShape.Shape = ResourceLoader.Load<RectangleShape2D>("res://Levels/Template/TileRectangle.tres");
        CollisionShape.Position = LilySprite.Position;
        CollisionShape.Owner = this;
    }

    public override void _InputEvent(Godot.Object viewport, InputEvent @event, int shapeIdx)
    {
        if (@event is InputEventMouseButton && @event.IsPressed())
        {
            GD.Print($"Clicked Tile {GridPosition}");
        }
    }
}