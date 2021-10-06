using System;
using Godot;

public class Player : Node2D
{
    //References to existing nodes
    private World World;
    private Grid Grid;
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
        CallDeferred("SetupPlayer");
    }

    private void SetupPlayer()
    {
        World = GetNode<World>("..");
        Grid = GetNode<Grid>("../Grid");
        PlayerSprite = GetNode<Sprite>("PlayerSprite");

        //Initialize properties of player
        PlayerSprite.Texture = GD.Load<Texture>("res://Game/Resources/32x32/Frog1_32x32_front.png");
        GridPosition = new Vector2(0, 0);

        Grid.InitializeGrid();
        EmitSignal(nameof(ScoreUpdated), Score);
        EmitSignal(nameof(HopCompleted), HopsRemaining);
        //Grid.PrintGrid(); //For debugging grid
    }

    private void AfterMovement(Vector2 movementDirection)
    {
        Bounce();
        UpdateHopsRemaining(-1);
        UpdateScore();  //to be outside player and triggered by signal
        CheckGoal();    //maybe this should be in grid
        CheckHopsRemaining();
    }

    private void Bounce()
    {
        ;
    }

    public void UpdateHopsRemaining(int addedHops)
    {
        HopsRemaining += addedHops;
        if (HopsRemaining > Grid.CurrentLevel.MaxHops)
        {
            HopsRemaining = Grid.CurrentLevel.MaxHops;
        }
        EmitSignal(nameof(HopCompleted), HopsRemaining);
    }

    public void UpdateScore()
    {
        Score += CurrentTile.PointValue;
        //GD.Print(GridPosition, " ", CurrentTile.Type, " ", CurrentTile.PointValue, " ", Score);
        if (CurrentTile.Type == Type.Score) //TODO: this needs to come out of here (probably)
        {
            CurrentTile.Type = Type.Blank;
        }
        EmitSignal(nameof(ScoreUpdated), Score);
    }

    private void CheckGoal()
    {
        if (CurrentTile.Type == Type.Goal)
        {
            UpdateHopsRemaining(Grid.CurrentLevel.HopsToAdd);
            EmitSignal(nameof(GoalReached));
            Grid.UpdateGrid();
            World.Timer.Reset();
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
        if (World != null)
        {
            if (!World.GameOver)
            {
                Vector2 MovementDirection;

                if (@event.IsActionPressed("ui_left"))
                {
                    if (GridPosition.x > 0)
                    {
                        GridPosition += MovementDirection = new Vector2(-1, 0);
                        AfterMovement(MovementDirection);
                    }
                }
                else if (@event.IsActionPressed("ui_right"))
                {
                    if (GridPosition.x < Grid.GridWidth - 1)
                    {
                        GridPosition += MovementDirection = new Vector2(1, 0);
                        AfterMovement(MovementDirection);
                    }
                }
                else if (@event.IsActionPressed("ui_up"))
                {
                    if (GridPosition.y > 0)
                    {
                        GridPosition += MovementDirection = new Vector2(0, -1);
                        AfterMovement(MovementDirection);
                    }
                }
                else if (@event.IsActionPressed("ui_down"))
                {
                    if (GridPosition.y < Grid.GridHeight - 1)
                    {
                        GridPosition += MovementDirection = new Vector2(0, 1);
                        AfterMovement(MovementDirection);
                    }
                }

                //FOR TESTING ONLY
                /*             if (@event.IsActionPressed("ui_select"))
                            {
                                Grid.UpdateGrid();
                            } */
            }
        }
    }


}