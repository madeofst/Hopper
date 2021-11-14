using System;
using Godot;

namespace Hopper
{
    public class Tile : Area2D
    {
        [Export]
        public Type Type { get; set; }
        [Export]
        private Vector2 _gridPosition;
        public Vector2 GridPosition
        { 
            get { return _gridPosition; }
            set
            {
                _gridPosition = value;
                Position = (_gridPosition * Size);
            }
        }
        [Export]
        public int PointValue { get; set; }
        [Export]
        public bool Editable { get; set; } = false;
        [Export]
        public bool Activated { get; set; } = false;
        public Vector2 Size = new Vector2(32, 32);
        [Export]
        public int JumpLength;

        [Signal]
        public delegate void TileUpdated(Vector2 gridPosition, Type type, int Score);

        //Children in the tree
        public CollisionShape2D CollisionShape;
        public Sprite LilySprite;
        public Sprite WaterSprite;
        public Counter Label;

        public Tile(){}   
        
/*         public Tile(Type type)
        {
            Type = type;
        }

        public Tile(Type type, Vector2 position)
        {
            Type = type;
            GridPosition = position;
        }

        public Tile(Type type, Vector2 position, int pointValue)
        {
            Type = type;
            GridPosition = position;
            PointValue = pointValue;
        }
 */
/*         public Tile(string Name)
        {
            this.Name = Name;
        } */

        public override void _Ready()
        {
            CollisionShape = GetNode<CollisionShape2D>("CollisionShape2D");
            LilySprite = GetNode<Sprite>("LilySprite");
            WaterSprite = GetNode<Sprite>("WaterSprite");
            Label = GetNode<Counter>("Label");
            
            if (Type == Type.Score) Label.BbcodeText = $"[center]{PointValue.ToString()}[/center]";

            CallDeferred("Init");
            Connect("mouse_entered", this, "OnMouseEnter");
            
            AnimationPlayer LilySpriteAnimator = GetNode<AnimationPlayer>("LilySprite/AnimationPlayer");
            if (Type == Type.Goal && Activated == true) LilySpriteAnimator.Play("Lights");
        }

        public void Init()
        {
            
        }

        /* public virtual void BuildTile(Type type, Vector2 size, Vector2 gridPosition, int score = 0)
        {
            Size = size;
            Label = new Counter(Size);
            Label.BbcodeEnabled = true;
                    
            LilySprite = new Sprite();
            WaterSprite = new Sprite();
            Type = type;
            GridPosition = gridPosition;
            PointValue = score;

            WaterSprite.Position = LilySprite.Position;
            WaterSprite.Texture = GD.Load<Texture>("res://Levels/Resources/Water1_32x32.png");
            AddChild(WaterSprite);
            WaterSprite.Owner = this;

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
        } */

        public override void _InputEvent(Godot.Object viewport, InputEvent @event, int shapeIdx)
        {
            InputEventMouseButton ev = null;
            if (@event is InputEventMouseButton) ev = (InputEventMouseButton)@event;
            if (ev != null && ev.IsPressed() && Editable)
            {
                if (ev.ButtonIndex == (int)ButtonList.Left)
                {
                    if (Type == Type.Jump) //Max enum value //TODO: this needs writing for new tile loading
                    {
                        EmitSignal("TileUpdated", GridPosition, Type.Lily, PointValue);
                    }    //Type = Type.Lily; //Min enum value
                    else
                    {
                        Type += 1;
                        EmitSignal("TileUpdated", GridPosition, Type, PointValue);
                    }
                }
                if (ev.ButtonIndex == (int)ButtonList.WheelUp && Type == Type.Score)
                {
                    if (PointValue < 1000) EmitSignal("TileUpdated", GridPosition, Type, PointValue + 100);
                    //GD.Print(ev.AsText());
                }
                if (ev.ButtonIndex == (int)ButtonList.WheelDown && Type == Type.Score)
                {
                    if (PointValue > 100)  EmitSignal("TileUpdated", GridPosition, Type, PointValue - 100);
                    //GD.Print(ev.AsText());
                }
            }
        }

        public void OnMouseEnter()
        {
            if(Editable) Input.SetDefaultCursorShape(Input.CursorShape.PointingHand);
        }
    }
}