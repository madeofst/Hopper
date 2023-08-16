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
        [Export]
        public Vector2 BounceDirection;

        [Signal]
        public delegate void TileUpdated(Vector2 gridPosition, Type type, int Score, Vector2 bounceDirection, bool eaten = false); //TODO: need to fix this now I've changed the parameters
        [Signal]
        public delegate void PlayerStartUpdated(Vector2 gridPosition);

        //Children in the tree
        public CollisionShape2D CollisionShape;
        public Sprite LilySprite;
        public Sprite BugSprite;
        public Counter Label;
        public AnimationPlayer LilyAnimation;
        public AnimationPlayer SplashAnimation;
        public AnimationPlayer BugAnimation;

        public Tile(){}   

        public override void _Ready()
        {
            CollisionShape = GetNode<CollisionShape2D>("CollisionShape2D");
            LilySprite = GetNode<Sprite>("LilySprite");
            BugSprite = GetNode<Sprite>("BugSprite");
            Label = GetNode<Counter>("Label");
            LilyAnimation = GetNode<AnimationPlayer>("LilySprite/AnimationPlayer");
            SplashAnimation = GetNode<AnimationPlayer>("SplashSprite/AnimationPlayer");
            BugAnimation = GetNode<AnimationPlayer>("BugSprite/AnimationPlayer");
            
            Random rand = new Random((int)(GridPosition.x * GridPosition.y * 100 + Position.x / Position.y));
            if (Type == Type.Score)
            {
                Label.BbcodeText = $"[center]{PointValue.ToString()}[/center]";
                BugAnimation.Play($"Hover{rand.Next(1, 3)}");
            }
            else if (Type == Type.Rock)
            {
                LilySprite.Frame = rand.Next(0, 4);
            }
            else if (Type == Type.Direct)
            {
                //BounceDirection = Vector2.Right; //TODO: need to add bounce direction to leveldata
                float Wrapped = Mathf.Wrap(BounceDirection.Angle(), 0, Mathf.Tau);
                float FullAngle = ((Wrapped / Mathf.Tau) * 4);
                int IntFullAngle = Mathf.RoundToInt(FullAngle);
                LilySprite.Frame = IntFullAngle;
            }


            Connect("mouse_entered", this, "OnMouseEnter");
            AnimationPlayer LilySpriteAnimator = GetNode<AnimationPlayer>("LilySprite/AnimationPlayer");
            if (Type == Type.Goal && Activated == true) LilySpriteAnimator.Play("Activate");
        }

        public override void _InputEvent(Godot.Object viewport, InputEvent @event, int shapeIdx)
        {
            InputEventMouseButton ev = null;
            if (@event is InputEventMouseButton) ev = (InputEventMouseButton)@event;
            if (ev != null && ev.IsPressed() && Editable)
            {
                if (ev.ButtonIndex == (int)ButtonList.Left)
                {
                    if (Type == Type.Direct) //TODO: The type here must always equal the max enum value
                    {
                        EmitSignal(nameof(TileUpdated), GridPosition, Type.Lily, PointValue, BounceDirection, false);
                    }
                    else
                    {
                        Type += 1;
                        if (Type == Type.Score) PointValue = 1;
                        if (Type == Type.Direct) BounceDirection = Vector2.Right;
                        EmitSignal(nameof(TileUpdated), GridPosition, Type, PointValue, BounceDirection, false);
                    }
                }
                else if (ev.ButtonIndex == (int)ButtonList.Right)
                {
                    EmitSignal(nameof(PlayerStartUpdated), GridPosition);
                }
                else if (ev.ButtonIndex == (int)ButtonList.WheelUp && Type == Type.Direct)
                {
                    EmitSignal(nameof(TileUpdated), GridPosition, Type, PointValue, BounceDirection.Rotated(Mathf.Pi / 2).Round(), false);
                }
                else if (ev.ButtonIndex == (int)ButtonList.WheelDown && Type == Type.Direct)
                {
                    EmitSignal(nameof(TileUpdated), GridPosition, Type, PointValue, BounceDirection.Rotated(-(Mathf.Pi / 2)).Round(), false);
                }
            }
        }

        public void OnMouseEnter()
        {
            if(Editable) Input.SetDefaultCursorShape(Input.CursorShape.PointingHand);
        }

        public void ToIdle()
        {
            GetNode<AnimationPlayer>("LilySprite/AnimationPlayer").Play("Idle");
        }

        public void Eat()
        {
            if (Type == Type.Score && BugSprite.Visible == true)
            {
                GetNode<CPUParticles2D>("CPUParticles2D").Emitting = true;
                GetNode<CPUParticles2D>("CPUParticles2D2").Emitting = true;
            }
        }

        public void SetAsEaten()
        {
            BugSprite.Visible = false;
            PointValue = 0;
            Label.Visible = false;
        }

        public Vector2 RotateBounceDirection()
        {
            BounceDirection = BounceDirection.Rotated(Mathf.Pi / 2).Round();
            return BounceDirection;
        }

        public void RotateBounceDirectionVisual()
        {
            LilySprite.Frame = (LilySprite.Frame + LilySprite.Hframes + 1) % LilySprite.Hframes;
        }

        public void UpdateBounceDirection(Vector2 updatedBounceDirection)
        {
            BounceDirection = updatedBounceDirection;
        }
    }
}