using System;
using Godot;

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

        public bool Animating { get; private set; } = false;
        public float AnimationTimeElapsed = 0;
        [Export]
        public Curve jumpCurve;

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

            EmitSignal(nameof(ScoreUpdated), TotalScore, LevelScore);
            EmitSignal(nameof(HopCompleted), HopsRemaining);
            
            ResetAnimation();
            Active = true;
        }

        private void CheckMovement(Vector2 Movement)
        {
            NewTile = Grid.GetTile(Grid.LimitToBounds(GridPosition + ExtraJump(Movement)));
            while (NewTile.Type == Type.Water)
            {
                NewTile = Grid.GetTile(Grid.DetermineWaterExit(NewTile, Movement));
            }

            if (NewTile.GridPosition != GridPosition && NewTile.Type != Type.Rock)
            {
                Animate(Movement);
            }
        }

        private void Animate(Vector2 movement)
        {
            Animating = true;
            if (movement == Vector2.Left)
            {
                AnimationPlayer.Play("JumpLeft");
            }
            else if (movement == Vector2.Right) 
            {
                AnimationPlayer.Play("JumpRight");
            }
            else if (movement == Vector2.Up)
            { 
                AnimationPlayer.Play("JumpUp");
            }
            else if (movement == Vector2.Down)
            {
                AnimationPlayer.Play("JumpDown");
            }
        }

        public void AfterAnimation(string animationName)
        {
            GD.Print($"Animation {animationName} finished");
            AnimationTimeElapsed = 0;
            if (animationName.BeginsWith("Jump"))
            {
                GridPosition = NewTile.GridPosition;
                UpdateHopsRemaining(-1);
                UpdateScore();
                if (!CheckGoal())
                {
                    CheckHopsRemaining();
                }
                Animating = false;
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
                //Active = false;
            }
        }

        public void ResetAnimation()
        {
            AnimationPlayer.Play("IdleDown");
        }

        public override void _PhysicsProcess(float delta)
        {
            if (Animating)
            {
                AnimationTimeElapsed += delta;
                float animationLength = AnimationPlayer.GetAnimation("JumpLeft").Length;
                float totalDistance = (NewTile.GridPosition - GridPosition).Length() * NewTile.Size.x;
                float distanceTravelled = totalDistance - (NewTile.GlobalPosition - GlobalPosition).Length();
                float timeRatio = AnimationTimeElapsed/animationLength;
                float pixelsToMove = totalDistance * jumpCurve.Interpolate(timeRatio) - distanceTravelled;
                GlobalPosition = GlobalPosition.MoveToward(NewTile.GlobalPosition, pixelsToMove);
            }
        }

        public override void _Input(InputEvent @event)
        {
            //TODO: Maybe add inputs to an event queue and trigger it in physics process

            if (Active && !Animating) //FIXME: world is not null??
            {
                if (@event.IsActionPressed("ui_left")) CheckMovement(Vector2.Left);
                else if (@event.IsActionPressed("ui_right")) CheckMovement(Vector2.Right);
                else if (@event.IsActionPressed("ui_up")) CheckMovement(Vector2.Up);
                else if (@event.IsActionPressed("ui_down")) CheckMovement(Vector2.Down);
            }
        }
    }
}