using System;
using Godot;

public class Player : Sprite
{
    //References to existing nodes
    private World World;
    private Grid Grid;
    private Counter HopCounter;
    private Counter ScoreCounter;

    //Player parameters
    public int HopsRemaining { get; set; } = 3;
    public int Score { get; set; } = 0;
    private Vector2 _GridPosition;
    public Vector2 GridPosition
    { 
        get
        {
            return _GridPosition;
        }
        set
        {
            _GridPosition = value;
            Position = _GridPosition * Grid.TileSize + Grid.Offset + Grid.TileSize/2;
        } 
    }

    //Signals
    [Signal]
    public delegate void MovementCompleted();
    [Signal]
    public delegate void GoalReached();

    public override void _Ready()
    {
        Name = "Player";
        CallDeferred("SetupPlayer");
    }

    private void SetupPlayer()
    {
        World = GetNode<World>("..");
        Grid = GetNode<Grid>("../Grid");
        HopCounter = GetNode<Counter>("../HopCounter");
        ScoreCounter = GetNode<Counter>("../ScoreCounter");
        Texture = GD.Load<Texture>("res://frog.png");

        GridPosition = new Vector2(0, 0);
        Scale = new Vector2(0.9f, 0.9f);

        Grid.InitializeGrid();
    }

    private void AfterMovement()
    {
        UpdateHopsRemaining(-1);
        EmitSignal(nameof(MovementCompleted));
        UpdateScore();  //to be outside player and triggered by signal
        CheckGoal();    //maybe this should be in grid
        CheckHopsRemaining();
    }

    public void UpdateHopsRemaining(int addedHops)
    {
        HopsRemaining += addedHops;
        if (HopsRemaining > Grid.CurrentLevel.MaxHops) 
        {
            HopsRemaining = Grid.CurrentLevel.MaxHops;
        }
        HopCounter.UpdateText(HopsRemaining);
    }

    private void UpdateScore()
    {
        Tile currentTile = Grid.Tile(GridPosition);
        Score += currentTile.PointValue;
        ScoreCounter.UpdateText(Score);

        if (currentTile.Type == Type.Score)
        {
            currentTile.Type = Type.Blank;
        }
    }

    private void CheckGoal()
    {
        if (Grid.Tile(GridPosition).Type == Type.Goal)
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
            if (@event.IsActionPressed("ui_left"))
            {
                if(GridPosition.x > 0)
                {
                    GridPosition += new Vector2(-1, 0);
                    AfterMovement();
                }
            }
            else if (@event.IsActionPressed("ui_right"))
            {
                if(GridPosition.x < Grid.GridWidth - 1)
                {
                    GridPosition += new Vector2(1, 0);
                    AfterMovement();
                }
            }
            else if (@event.IsActionPressed("ui_up"))
            {
                if(GridPosition.y > 0)
                {
                    GridPosition += new Vector2(0, -1);
                    AfterMovement();
                }
            }
            else if (@event.IsActionPressed("ui_down"))
            {
                if(GridPosition.y < Grid.GridHeight - 1)
                {
                    GridPosition += new Vector2(0, 1);
                    AfterMovement();
                }
            }
            
            //FOR TESTING ONLY
            if (@event.IsActionPressed("ui_select"))
            {
                Grid.UpdateGrid();
            }
        }
        }
    }


}