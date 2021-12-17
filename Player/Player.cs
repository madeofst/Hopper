using System;
using Godot;
using System.Collections.Generic;

namespace Hopper
{
    public class Player : Node2D
    {
        //References to existing nodes
        public Level CurrentLevel;
        public Grid Grid;
        private Sprite PlayerSprite;
        private AnimationPlayer AnimationPlayer;

        //Player parameters
        public int HopsRemaining { get; set; } = 3;
        public int TotalScore { get; set; } = 0;
        public int LevelScore { get; set; } = 0;
        private Vector2 _GridPosition;
        private Tile _CurrentTile;
        private Tile LandTile, SplashTile, ExitTile;
        private bool Active = false;

        public Vector2 GridPosition
        {
            get
            {
                return _GridPosition;
            }
            set
            {
                _GridPosition = value;
                Position = _GridPosition * Grid.TileSize + Grid.RectPosition;// + Grid.TileSize / 2;
            }
        }
        private Tile CurrentTile 
        { 
            get
            {
                return Grid.GetTile(GridPosition);   
            } 
        }

        //Animation parameters
        //public bool Animating { get; private set; } = false;
        public Queue<Vector2> MoveInputQueue;
        public Animation CurrentAnimation;
        public float AnimationTimeElapsed = 0;
        [Export]
        public Curve JumpCurve;
        [Export]
        public Curve DoubleJumpCurve;
        private Curve currentJumpCurve;
        

        //Signals
        [Signal]
        public delegate void GoalReached();
        [Signal]
        public delegate void ScoreUpdated(int totalScore, int levelScore = 0);
        [Signal]
        public delegate void HopCompleted(int hopsRemaining);
        [Signal]
        public delegate void TileChanged(Type NewType);
        [Signal]
        public delegate void HopsExhausted();

        public override void _Ready()
        {
            Name = "Player";
            //CallDeferred("Init");
        }

        public void Init(Level currentLevel) //Called each time a new level starts
        {
            PlayerSprite = GetNode<Sprite>("PlayerSprite");
            AnimationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");

            CurrentLevel = currentLevel;
            Grid = CurrentLevel.Grid;            
            GridPosition = CurrentLevel.PlayerStartPosition;
            HopsRemaining = CurrentLevel.StartingHops;
            LevelScore = 0;

            MoveInputQueue = new Queue<Vector2>();

            EmitSignal(nameof(ScoreUpdated), TotalScore, LevelScore);
            EmitSignal(nameof(HopCompleted), HopsRemaining);
            
            ResetAnimation();
            Active = true;
        }

        private void CheckMovement(Vector2 Movement)
        {
            LandTile = Grid.GetTile(Grid.LimitToBounds(GridPosition + ExtraJump(Movement)));

            string jumpPrefix = "Jump";
            string jumpSuffix = "";
            currentJumpCurve = JumpCurve;
            if ((LandTile.GridPosition - GridPosition).Length() > 1) 
            {
                jumpPrefix = "DoubleJump";
                currentJumpCurve = DoubleJumpCurve;
            }

            if (LandTile.Type == Type.Water) SplashTile = LandTile;

            while (LandTile.Type == Type.Water)
            {
                LandTile = Grid.GetTile(Grid.DetermineWaterExit(LandTile, Movement));
                ExitTile = Grid.GetTile(LandTile.GridPosition - Movement);
            }

            if (SplashTile != null) jumpSuffix = "Splash";

            if (LandTile.GridPosition != GridPosition && LandTile.Type != Type.Rock)
            {
                Animate(Movement, jumpPrefix, jumpSuffix);
            }
            else
            {
                LandTile = CurrentTile;
                CurrentAnimation = null;
            }
        }

        private Vector2 ExtraJump(Vector2 movementDirection)
        {
            if (CurrentTile.Type == Type.Jump)
            {
                Vector2 jumpMovement = movementDirection * CurrentTile.JumpLength;
                while (Grid.GetTile(Grid.LimitToBounds(GridPosition + jumpMovement)).Type == Type.Rock &&
                       jumpMovement.Abs() >= Vector2.Zero)
                {
                    jumpMovement -= movementDirection;
                }
                return jumpMovement;
            }
            return movementDirection;
        }

        public void ExitAnimation()
        {
            CurrentAnimation = null;
            SplashTile = null;
            GlobalPosition = ExitTile.GlobalPosition;
            GridPosition = ExitTile.GridPosition;
            Animate((LandTile.GridPosition - ExitTile.GridPosition), "Jump", "Exit");
        }

        private void Animate(Vector2 movement, string jumpPrefix, string jumpSuffix = "")
        {
            if      (movement == Vector2.Left)   CurrentAnimation = AnimationPlayer.GetAnimation($"{jumpPrefix}Left{jumpSuffix}");  
            else if (movement == Vector2.Right)  CurrentAnimation = AnimationPlayer.GetAnimation($"{jumpPrefix}Right{jumpSuffix}");
            else if (movement == Vector2.Up)     CurrentAnimation = AnimationPlayer.GetAnimation($"{jumpPrefix}Up{jumpSuffix}");
            else if (movement == Vector2.Down)   CurrentAnimation = AnimationPlayer.GetAnimation($"{jumpPrefix}Down{jumpSuffix}");
            if (CurrentTile.Type == Type.Jump)
            {
                CurrentTile.LilyAnimation.Play("Spring");
            }
            else
            {
                CurrentTile.LilyAnimation.Play("Jump");
            }
            CurrentTile.SplashAnimation.Play("Jump");
            AnimationPlayer.Play(AnimationPlayer.FindAnimation(CurrentAnimation));
        }

        public void TriggerLandAnimation()
        {
            LandTile.SplashAnimation.Play("Land");
            if (LandTile.Type != Type.Water) LandTile.LilyAnimation.Play("Land");
        }

        public void TriggerSplashAnimation()
        {
            if (SplashTile != null) SplashTile.SplashAnimation.Play("Splash");
        }

        public void TriggerExitSplashAnimation()
        {
            if (ExitTile != null) ExitTile.SplashAnimation.Play("Exit");
        }

        public void TriggerJumpSpringAnimation()
        {
            
        }

        public void AfterAnimation(string animationName)
        {
            AnimationTimeElapsed = 0;
            if (animationName.Right(animationName.Length - 6) == "Splash")
            {
                ExitAnimation();
            }
            else
            {
                if (animationName.BeginsWith("Jump") || animationName.BeginsWith("DoubleJump"))
                {
                    GridPosition = LandTile.GridPosition;
                    UpdateHopsRemaining(-1);
                    UpdateScore();
                    if (!CheckGoal())
                    {
                        CheckHopsRemaining();
                    }
                }
                CurrentAnimation = null;
                LandTile = null;
            }
        }

        public void UpdateHopsRemaining(int addedHops)
        {
            HopsRemaining += addedHops;
            if (HopsRemaining > CurrentLevel.MaximumHops)
            {
                HopsRemaining = CurrentLevel.MaximumHops;
            }
            EmitSignal(nameof(HopCompleted), HopsRemaining);
        }

        public void UpdateScore()
        {
            if (CurrentTile.PointValue > 0)
            {
                LevelScore += CurrentTile.PointValue;
                TotalScore += CurrentTile.PointValue;
                if (CurrentTile.Type == Type.Score) 
                {
                    EmitSignal(nameof(TileChanged), Type.Lily);
                }
                EmitSignal(nameof(ScoreUpdated), TotalScore, LevelScore);
            }
        }

        private bool CheckGoal()
        {
            if (CurrentTile.Type == Type.Goal && CurrentTile.Activated)
            {
                EmitSignal(nameof(GoalReached));
                UpdateHopsRemaining(CurrentLevel.StartingHops); //FIXME: Hops added should come from next level
                return true;
            }
            return false;
        }

        private void CheckHopsRemaining()
        {
            if (HopsRemaining <= 0)
            {
                EmitSignal(nameof(HopsExhausted));
            }
        }

        public void ResetAnimation()
        {
            AnimationPlayer.Play("IdleDown");
        }

        public override void _Process(float delta)
        {
            if(CurrentAnimation == null && MoveInputQueue.Count > 0)
            {
                CheckMovement(MoveInputQueue.Dequeue());
            }
        }

        public override void _PhysicsProcess(float delta)
        {
            if (CurrentAnimation != null)
            {
                //GD.Print(GlobalPosition);
                Tile endTile = LandTile;
                if (SplashTile != null) endTile = SplashTile;
                AnimationTimeElapsed += delta;
                float animationLength = CurrentAnimation.Length;
                float totalDistance = (endTile.GridPosition - GridPosition).Length() * endTile.Size.x;
                float distanceTravelled = totalDistance - (endTile.GlobalPosition - GlobalPosition).Length();
                float timeRatio = AnimationTimeElapsed/animationLength;
                float pixelsToMove = totalDistance * currentJumpCurve.Interpolate(timeRatio) - distanceTravelled;
                GlobalPosition = GlobalPosition.MoveToward(endTile.GlobalPosition, pixelsToMove);
            }
        }

        public override void _Input(InputEvent @event)
        {
            if (Active) //FIXME: world is not null??
            {
                if (@event.IsActionPressed("ui_left")) MoveInputQueue.Enqueue(Vector2.Left);
                else if (@event.IsActionPressed("ui_right")) MoveInputQueue.Enqueue(Vector2.Right);
                else if (@event.IsActionPressed("ui_up")) MoveInputQueue.Enqueue(Vector2.Up);
                else if (@event.IsActionPressed("ui_down")) MoveInputQueue.Enqueue(Vector2.Down);
            }
        }
    }
}