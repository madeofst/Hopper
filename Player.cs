using System;
using Godot;

public class Player : Sprite
{
    private Grid Grid;
    private HopCounter HopCounter;
    public int HopsRemaining = 3;
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
        Grid = GetNode<Grid>("../Grid");
        HopCounter = GetNode<HopCounter>("../HopCounter");
        GridPosition = new Vector2(3, 3);
        Texture = GD.Load<Texture>("res://icon.png");
        Scale = new Vector2(0.9f, 0.9f);

        Grid.SetupGrid();
    }

    public override void _Input(InputEvent @event)
    {
        if (@event.IsActionPressed("ui_left"))
        {
            if(GridPosition.x > 0) 
            {
                GridPosition += new Vector2(-1, 0);
                UpdateHopsRemaining(-1);
                CheckPosition();
            }
        }
        else if (@event.IsActionPressed("ui_right"))
        {
            if(GridPosition.x < Grid.GridWidth - 1)
            {
                GridPosition += new Vector2(1, 0);
                UpdateHopsRemaining(-1);
                CheckPosition();
            }
        }
        else if (@event.IsActionPressed("ui_up"))
        {
            if(GridPosition.y > 0)
            {
                GridPosition += new Vector2(0, -1);
                UpdateHopsRemaining(-1);
                CheckPosition();
            }
        }
        else if (@event.IsActionPressed("ui_down"))
        {
            if(GridPosition.y < Grid.GridHeight - 1)
            {
                GridPosition += new Vector2(0, 1);
                UpdateHopsRemaining(-1);
                CheckPosition();
            }
        }
        
        if (@event.IsActionPressed("ui_select"))
        {
            Grid.UpdateGrid();
        }
    }

    public void UpdateHopsRemaining(int addedHops)
    {
        HopsRemaining += addedHops;
        HopCounter.UpdateText(HopsRemaining.ToString());
    }

    private void CheckPosition()
    {
        if (HopsRemaining <= 0)
        {
            HopCounter.UpdateText("GAME OVER.");
        }

        if (Grid.Tiles[(int)GridPosition.x, (int)GridPosition.y].Type == Type.White)
        {
            UpdateHopsRemaining(3);
            Grid.UpdateGrid();
        }
    }
}