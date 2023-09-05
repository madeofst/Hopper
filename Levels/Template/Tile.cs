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
                Position = _gridPosition * Size;
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
        public delegate void TileUpdated(Vector2 gridPosition, Type type, int Score, Vector2 bounceDirection, bool eaten = false, bool activated = false); //TODO: need to fix this now I've changed the parameters
        [Signal]
        public delegate void PlayerStartUpdated(Vector2 gridPosition);
        [Signal]
        public delegate void TileSlidUp(Vector2 gridPosition);
        [Signal]
        public delegate void Eaten();

        //Children in the tree
        public CollisionShape2D CollisionShape;
        public Sprite LilySprite;
        public Sprite BugSprite;
        public Sprite WaterSprite;
        public Sprite BackgroundSprite;
        public Sprite BossIndicatorSprite;
        public Sprite ShadowSprite;
        public Counter Label;
        public Tween Tween;
        public AnimationPlayer LilyAnimation;
        public AnimationPlayer SplashAnimation;
        public AnimationPlayer BugAnimation;

        public TileChangeInstruction TileChangeInstruction;

        private Random rand;

        public Tile(){}   

        public override void _Ready()
        {
            CollisionShape = GetNode<CollisionShape2D>("CollisionShape2D");
            LilySprite = GetNode<Sprite>("LilySprite");
            BugSprite = GetNode<Sprite>("BugSprite");
            WaterSprite = GetNode<Sprite>("WaterSprite");
            BackgroundSprite = GetNode<Sprite>("BackgroundSprite");
            ShadowSprite = GetNode<Sprite>("ShadowSprite");
            BossIndicatorSprite = GetNode<Sprite>("BossIndicatorSprite");
            Label = GetNode<Counter>("Label");
            Tween = GetNode<Tween>("Tween");
            LilyAnimation = GetNode<AnimationPlayer>("LilySprite/AnimationPlayer");
            SplashAnimation = GetNode<AnimationPlayer>("SplashSprite/AnimationPlayer");
            BugAnimation = GetNode<AnimationPlayer>("BugSprite/AnimationPlayer");
            
            rand = new Random((int)(GridPosition.x * GridPosition.y * 100 + Position.x / Position.y));
            if (Type == Type.Score)
            {
                //Label.BbcodeText = $"[center]{PointValue.ToString()}[/center]";
                BugAnimation.Play($"Hover{rand.Next(1, 3)}");
            }
            else if (Type == Type.Rock)
            {
                LilySprite.Frame = rand.Next(0, 4);
            }
            else if (Type == Type.Direct)
            {
                DetermineIdleFrameFromBounceDirection();
            }

            Connect("mouse_entered", this, "OnMouseEnter");
            AnimationPlayer LilySpriteAnimator = GetNode<AnimationPlayer>("LilySprite/AnimationPlayer");
            if (Type == Type.Goal && Activated)
            {
                LilySpriteAnimator.Play("Activate");
            }
        }

        private void DetermineIdleFrameFromBounceDirection()
        {
            float Wrapped = Mathf.Wrap(BounceDirection.Angle(), 0, Mathf.Tau);
            float FullAngle = Wrapped / Mathf.Tau * 4;
            int IntFullAngle = Mathf.RoundToInt(FullAngle);
            LilySprite.Frame = IntFullAngle;
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
                        EmitSignal(nameof(TileUpdated), GridPosition, Type.Lily, PointValue, BounceDirection, false, false);
                    }
                    else
                    {
                        Type += 1;
                        if (Type == Type.Score) PointValue = 1;
                        if (Type == Type.Direct) BounceDirection = Vector2.Right;
                        EmitSignal(nameof(TileUpdated), GridPosition, Type, PointValue, BounceDirection, false, false);
                    }
                }
                else if (ev.ButtonIndex == (int)ButtonList.Right)
                {
                    EmitSignal(nameof(PlayerStartUpdated), GridPosition);
                }
                else if (ev.ButtonIndex == (int)ButtonList.WheelUp && Type == Type.Direct)
                {
                    EmitSignal(nameof(TileUpdated), GridPosition, Type, PointValue, BounceDirection.Rotated(Mathf.Pi / 2).Round(), false, false);
                }
                else if (ev.ButtonIndex == (int)ButtonList.WheelDown && Type == Type.Direct)
                {
                    EmitSignal(nameof(TileUpdated), GridPosition, Type, PointValue, BounceDirection.Rotated(-(Mathf.Pi / 2)).Round(), false, false);
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
                EmitSignal(nameof(Eaten));
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

        public void SlideUp()
        {
            if (Type == Type.Direct)
            {
                LilySprite.Texture = GD.Load<Texture>($"res://Levels/Resources/Director_32x32_{LilySprite.Frame + 1}.png");
                LilySprite.Hframes = 1;
                LilySprite.Vframes = 1;
                LilySprite.Frame = 0;
                LilySprite.Position = new Vector2(LilySprite.Position.x, LilySprite.Position.y + 16);
            }
            else if (Type == Type.Score)
            {
                (BugSprite.Material as ShaderMaterial).SetShaderParam("offset", -8f);
                BugSprite.Texture = GD.Load<Texture>("res://Levels/Resources/Dragonfly Single Frame.png");
                BugSprite.Hframes = 1;
                BugSprite.Vframes = 1;
                BugSprite.Frame = 0;
            }
/*             else if (Type == Type.Goal && Activated)
            {
                GetNode<AnimationPlayer>("LilySprite/AnimationPlayer").Play("Idle");
            } */

            var LilySpriteMaterial = LilySprite.Material as ShaderMaterial;
            if (LilySprite.Texture != null)
            {
                LilySpriteMaterial.SetShaderParam("image_height_px", (float)LilySprite.Texture.GetSize().y);
            }

            LilySpriteMaterial.SetShaderParam("slide_up_progress", 0);
            Tween.InterpolateProperty(LilySpriteMaterial,
                                      "shader_param/slide_up_progress", 
                                      0,
                                      1, 
                                      0.3f, 
                                      Tween.TransitionType.Linear, 
                                      Tween.EaseType.In);
            
            (WaterSprite.Material as ShaderMaterial).SetShaderParam("slide_up_progress", 0);
            Tween.InterpolateProperty(WaterSprite.Material,
                                      "shader_param/slide_up_progress", 
                                      0,
                                      1, 
                                      0.3f, 
                                      Tween.TransitionType.Linear, 
                                      Tween.EaseType.In);

            (BugSprite.Material as ShaderMaterial).SetShaderParam("slide_up_progress", 0);
            Tween.InterpolateProperty(BugSprite.Material,
                                      "shader_param/slide_up_progress", 
                                      0,
                                      1, 
                                      0.3f, 
                                      Tween.TransitionType.Linear, 
                                      Tween.EaseType.In);

            (ShadowSprite.Material as ShaderMaterial).SetShaderParam("progress", 0);
            Tween.InterpolateProperty(ShadowSprite.Material,
                                      "shader_param/progress", 
                                      0,
                                      1, 
                                      0.3f, 
                                      Tween.TransitionType.Linear, 
                                      Tween.EaseType.In);
            Tween.Start();
        }
        
        public void SlideAcross(TileChangeInstruction I)
        {
            Animate("RESET");
            
            if (Type == Type.Score)
            {
                BugSprite.Texture = GD.Load<Texture>("res://Levels/Resources/Dragonfly Single Frame.png");
                BugSprite.Hframes = 1;
                BugSprite.Vframes = 1;
                BugSprite.Frame = 0;
                
            }
            else if (Type == Type.Direct)
            {
                LilySprite.Texture = GD.Load<Texture>($"res://Levels/Resources/Director_32x32_{LilySprite.Frame + 1}.png");
                LilySprite.Hframes = 1;
                LilySprite.Vframes = 1;
                LilySprite.Frame = 0;
                LilySprite.Position = new Vector2(LilySprite.Position.x, LilySprite.Position.y + 16);
            }
            else if (Type == Type.Rock)
            {
                LilySprite.Texture = GD.Load<Texture>($"res://Levels/Resources/UpdatedPalette/Rocks_32x32_{LilySprite.Frame + 1}.png");
                LilySprite.Hframes = 1;
                LilySprite.Vframes = 1;
                LilySprite.Frame = 0;
            }
            else if (Type == Type.Jump)
            {
                LilySprite.Texture = GD.Load<Texture>("res://Levels/Resources/UpdatedPalette/LilyPad_Spring_Single_Frame.png");
                LilySprite.Hframes = 1;
                LilySprite.Vframes = 1;
                LilySprite.Frame = 0;
            }
            else if (Type == Type.Goal && Activated)
            {
                (LilySprite.Material as ShaderMaterial).SetShaderParam("image_width_px", 64f);
                LilySprite.Texture = GD.Load<Texture>($"res://Levels/Resources/UpdatedPalette/LilyPad_Goal_Animate_{LilySprite.Frame + 1}.png");
                LilySprite.Hframes = 1;
                LilySprite.Vframes = 1;
                LilySprite.Frame = 0;
            }

            TileChangeInstruction = I;
            WaterSprite.Visible = true;
            BackgroundSprite.Visible = true;
            (LilySprite.Material as ShaderMaterial).SetShaderParam("slide_across_progress", 0);
            Tween.InterpolateProperty(LilySprite.Material,
                                      "shader_param/slide_across_progress", 
                                      0,
                                      1, 
                                      0.2f, 
                                      Tween.TransitionType.Linear, 
                                      Tween.EaseType.In);
            (WaterSprite.Material as ShaderMaterial).SetShaderParam("slide_across_progress", 0);
            Tween.InterpolateProperty(WaterSprite.Material,
                                      "shader_param/slide_across_progress", 
                                      0,
                                      1, 
                                      0.2f, 
                                      Tween.TransitionType.Linear, 
                                      Tween.EaseType.In);

            (BugSprite.Material as ShaderMaterial).SetShaderParam("slide_across_progress", 0);
            Tween.InterpolateProperty(BugSprite.Material,
                                      "shader_param/slide_across_progress", 
                                      0,
                                      1, 
                                      0.2f, 
                                      Tween.TransitionType.Linear, 
                                      Tween.EaseType.In);
            Tween.Start();
        }

        public void AfterSlide()
        {
            if (TileChangeInstruction != null)
            {
                GetParent<Grid>().UpdateTile(TileChangeInstruction);
                TileChangeInstruction = null;
            }
            else
            {
                WaterSprite.Visible = false;
                BackgroundSprite.Visible = false;
                EmitSignal(nameof(TileSlidUp), GridPosition);

                if (Type == Type.Direct)
                {
                    LilySprite.Texture = GD.Load<Texture>($"res://Levels/Resources/Director_32x32.png");
                    LilySprite.Hframes = 4;
                    LilySprite.Vframes = 1;
                    DetermineIdleFrameFromBounceDirection();
                    LilySprite.Position = new Vector2(LilySprite.Position.x, LilySprite.Position.y - 16);
                }
                else if (Type == Type.Score)
                {
                    BugSprite.Texture = GD.Load<Texture>($"res://Levels/Resources/Dragonfly Sprite Sheet.png");
                    BugSprite.Hframes = 7;
                    BugSprite.Vframes = 4;
                    //BugAnimation.Play($"Hover{rand.Next(1, 3)}");
                }
            }
        }

        public void Animate(string AnimationName)
        {
            BossIndicatorSprite.GetNode<AnimationPlayer>("AnimationPlayer").Play(AnimationName);
        }
    }
}