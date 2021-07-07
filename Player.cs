using System;
using Godot;

public class Player : Sprite
{
    private World World;
    private Grid Grid;
    private Counter HopCounter;
    private Counter ScoreCounter;
    public int HopsRemaining = 3;
    public int MaxHops = 7;
    public int Score = 0;
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
        GridPosition = new Vector2(3, 3); //Magic number
        Texture = GD.Load<Texture>("res://frog.png");
        Scale = new Vector2(0.9f, 0.9f);

        Grid.SetupGrid();
    }

    private void AfterMovement()
    {
        UpdateHopsRemaining(-1);
        UpdateScore();
        CheckGoal();
        CheckHopsRemaining();
    }

    public void UpdateHopsRemaining(int addedHops)
    {
        HopsRemaining += addedHops;
        if (HopsRemaining > MaxHops) HopsRemaining = MaxHops;
        HopCounter.UpdateText(HopsRemaining);
    }

    private void CheckGoal()
    {
        if (Grid.Tiles[(int)GridPosition.x, (int)GridPosition.y].Type == Type.Goal)
        {
            UpdateHopsRemaining(3);
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