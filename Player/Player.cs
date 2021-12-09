using System;
using Godot;

namespace Hopper
{
    public class Player : Node2D
    {
        //References to existing nodes
        //private World World;
        public Level CurrentLevel;
        public Grid Grid;
        private Sprite PlayerSprite;

        //Player parameters
        public int HopsRemaining { get; set; } = 3;
        public int TotalScore { get; set; } = 0;
        public int LevelScore { get; set; } = 0;
        private Vector2 _GridPosition;
        private Tile _CurrentTile;
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
                Position = _GridPosition * Grid.TileSize + Grid.RectPosition + Grid.TileSize / 2;
            }
        }
        private Tile CurrentTile 
        { 
            get
            {
                return Grid.GetTile(GridPosition);   
            } 
        }

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
            //if (World is null)
            //{
                //World = GetNode<World>("..");
                PlayerSprite = GetNode<Sprite>("PlayerSprite");
                //PlayerSprite.Texture = GD.Load<Texture>("res://Player/Resources/Frog1_32x32_front.png");
            //}
            CurrentLevel = currentLevel;
            Grid = CurrentLevel.Grid;            
            GridPosition = CurrentLevel.PlayerStartPosition;
            HopsRemaining = CurrentLevel.StartingHops;
            LevelScore = 0;

            EmitSignal(nameof(ScoreUpdated), TotalScore, LevelScore);
            EmitSignal(nameof(HopCompleted), HopsRemaining);
            Active = true;
        }

        private void CheckMovement(Vector2 Movement)
        {
            Tile newTile = Grid.GetTile(Grid.LimitToBounds(GridPosition + ExtraJump(Movement)));
            while (newTile.Type == Type.Water)
            {
                newTile = Grid.GetTile(Grid.DetermineWaterExit(newTile, Movement));
            }

            if (newTile.GridPosition != GridPosition && newTile.Type != Type.Rock)
            {
                GridPosition = newTile.GridPosition;
                UpdateHopsRemaining(-1);
                UpdateScore();
                if (!CheckGoal())
                {
                    CheckHopsRemaining();
                }
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

        public override void _Input(InputEvent @event)
        {
            if (Active) //FIXME: world is not null??
            {
                if (@event.IsActionPressed("ui_left")) 
                    CheckMovement(new Vector2(-1, 0));
                else if (@event.IsActionPressed("ui_right")) 
                    CheckMovement(new Vector2(1, 0));
                else if (@event.IsActionPressed("ui_up")) 
                    CheckMovement(new Vector2(0, -1));
                else if (@event.IsActionPressed("ui_down"))
                    CheckMovement(new Vector2(0, 1));
            }
        }
    }
}