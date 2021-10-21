using System;
using Godot;

namespace Hopper
{
    public class Player : Node2D
    {
        //References to existing nodes
        private World World;
        public Level CurrentLevel;
        public Grid Grid;
        private Sprite PlayerSprite;

        //Player parameters
        public int HopsRemaining { get; set; } = 3;
        public int Score { get; set; } = 0;
        private Vector2 _GridPosition;
        private Tile _CurrentTile;

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
                return Grid.Tile(GridPosition);   
            } 
        }

        //Signals
        [Signal]
        public delegate void GoalReached();
        [Signal]
        public delegate void ScoreUpdated(int score);
        [Signal]
        public delegate void HopCompleted(int hopsRemaining);

        public override void _Ready()
        {
            Name = "Player";
            //CallDeferred("Init");
        }

        public void Init()
        {
            World = GetNode<World>("..");
            CurrentLevel = World.CurrentLevel;
            Grid = CurrentLevel.Grid;
            PlayerSprite = GetNode<Sprite>("PlayerSprite");

            //Initialize properties of player
            PlayerSprite.Texture = GD.Load<Texture>("res://Player/Resources/Frog1_32x32_front.png");
            //GridPosition = new Vector2(0, 0);
            
            GridPosition = CurrentLevel.PlayerStartPosition;
            HopsRemaining = CurrentLevel.StartingHops;

            EmitSignal(nameof(ScoreUpdated), Score);
            EmitSignal(nameof(HopCompleted), HopsRemaining);
        }

        private void CheckMovement(Vector2 Movement)
        {
            Vector2 NewPosition = LimitToBounds(GridPosition + ExtraJump(Movement));
            if (Grid.Tile(NewPosition).Type != Type.Rock)
            {
                GridPosition = NewPosition;
                UpdateHopsRemaining(-1);
                UpdateScore();
                CheckGoal();   
                CheckHopsRemaining();
            }
        }

        private Vector2 LimitToBounds(Vector2 Position)
        {
            return new Vector2(
                Mathf.Clamp(Position.x, 0, Grid.GridWidth - 1),
                Mathf.Clamp(Position.y, 0, Grid.GridHeight - 1)
            );
        }

        private Vector2 ExtraJump(Vector2 movementDirection)
        {
            if (CurrentTile.Type == Type.Jump)
            {
                movementDirection = movementDirection * CurrentTile.JumpLength;
            };
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
            Score += CurrentTile.PointValue;
            if (CurrentTile.Type == Type.Score) 
            {
                CurrentTile.Type = Type.Lily;
            }
            EmitSignal(nameof(ScoreUpdated), Score);
        }

        private void CheckGoal()
        {
            if (CurrentTile.Type == Type.Goal)
            {
                EmitSignal(nameof(GoalReached));
                UpdateHopsRemaining(CurrentLevel.StartingHops); //FIXME: Hops added should come from next level
            }
        }

        private void CheckHopsRemaining()
        {
            if (HopsRemaining <= 0)
            {
                World.GameOver = true;
            }
        }

        public override void _Input(InputEvent @event)
        {
            if (World != null && !World.GameOver)
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