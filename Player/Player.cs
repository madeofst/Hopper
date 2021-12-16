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
        private Tile NewTile;
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
            NewTile = Grid.GetTile(Grid.LimitToBounds(GridPosition + ExtraJump(Movement)));

            string jumpType = "Jump";
            currentJumpCurve = JumpCurve;
            if ((NewTile.GridPosition - GridPosition).Length() > 1) 
            {
                jumpType = "DoubleJump";
                currentJumpCurve = DoubleJumpCurve;
            }

            while (NewTile.Type == Type.Water)
            {
                NewTile = Grid.GetTile(Grid.DetermineWaterExit(NewTile, Movement));
            }

            if (NewTile.GridPosition != GridPosition && NewTile.Type != Type.Rock)
            {
                Animate(Movement, jumpType);
            }
            else
            {
                CurrentAnimation = null;
                //nimating = false;
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

        private void Animate(Vector2 movement, string jumpType)
        {
            if (movement == Vector2.Left)        CurrentAnimation = AnimationPlayer.GetAnimation($"{jumpType}Left");  
            else if (movement == Vector2.Right)  CurrentAnimation = AnimationPlayer.GetAnimation($"{jumpType}Right");
            else if (movement == Vector2.Up)     CurrentAnimation = AnimationPlayer.GetAnimation($"{jumpType}Up");
            else if (movement == Vector2.Down)   CurrentAnimation = AnimationPlayer.GetAnimation($"{jumpType}Down");
            AnimationPlayer.Play(AnimationPlayer.FindAnimation(CurrentAnimation));
            CurrentTile.SplashAnimation.Play("Jump");   
            CurrentTile.LilyAnimation.Play("Jump");
        }

        public void TriggerLandAnimation()
        {
            NewTile.SplashAnimation.Play("Land");
            NewTile.LilyAnimation.Play("Land");
        }

        public void AfterAnimation(string animationName)
        {
            AnimationTimeElapsed = 0;
            if (animationName.BeginsWith("Jump") || animationName.BeginsWith("DoubleJump"))
            {
                GridPosition = NewTile.GridPosition;
                UpdateHopsRemaining(-1);
                UpdateScore();
                if (!CheckGoal())
                {
                    CheckHopsRemaining();
                }
            }
            CurrentAnimation = null;
            //Animating = false;
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
                //CurrentAnimation = MoveInputQueue.Dequeue();
                CheckMovement(MoveInputQueue.Dequeue());
            }
        }

        public override void _PhysicsProcess(float delta)
        {
            if (CurrentAnimation != null)
            {
                AnimationTimeElapsed += delta;
                float animationLength = CurrentAnimation.Length;
                float totalDistance = (NewTile.GridPosition - GridPosition).Length() * NewTile.Size.x;
                float distanceTravelled = totalDistance - (NewTile.GlobalPosition - GlobalPosition).Length();
                float timeRatio = AnimationTimeElapsed/animationLength;
                float pixelsToMove = totalDistance * currentJumpCurve.Interpolate(timeRatio) - distanceTravelled;
                GlobalPosition = GlobalPosition.MoveToward(NewTile.GlobalPosition, pixelsToMove);
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