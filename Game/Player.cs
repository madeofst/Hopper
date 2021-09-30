using System;
using Godot;

public class Player : Sprite
{
    //References to existing nodes
    private World World;
    private Grid Grid;

    //Player parameters
    public int HopsRemaining { get; set; } = 3;

    internal object ScoreIncrease()
    {
        throw new NotImplementedException();
    }

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
            Position = _GridPosition * Grid.TileSize + Grid.RectPosition + Grid.TileSize/2;
        } 
    }

    //Signals
    [Signal]
    public delegate void MovementCompleted();
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

        //Initialize properties of player
        Texture = GD.Load<Texture>("res://Game/Resources/frog2.png");
        GridPosition = new Vector2(0, 0);

        Grid.InitializeGrid();
        EmitSignal(nameof(ScoreUpdated), Score);
        EmitSignal(nameof(HopCompleted), HopsRemaining);
        //Grid.PrintGrid(); //For debugging grid
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
        EmitSignal(nameof(HopCompleted), HopsRemaining);
    }

    public void UpdateScore()
    {
        Tile currentTile = Grid.Tile(GridPosition);
        Score += currentTile.PointValue;

        GD.Print(GridPosition, " ", currentTile.Type, " ", currentTile.PointValue, " ", Score);

        //TODO: this needs to come out of here
        if (currentTile.Type == Type.Score)
        {
            currentTile.Type = Type.Blank;
        }

        EmitSignal(nameof(ScoreUpdated), Score);

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
/*             if (@event.IsActionPressed("ui_select"))
            {
                Grid.UpdateGrid();
            } */
        }
        }
    }


}