using Godot;
using System;

public class LevelEditor : Node2D
{
    private Grid Grid { get; set; }

    public override void _Ready()
    {
        NewGrid();
        CallDeferred("InitializeBlankGrid");
    }

    private void NewGrid()
    {
/*         Grid = new EditableGrid();
        AddChild(Grid); */
    }
    
    private void InitializeBlankGrid()
    {
        //Grid.InitializeGrid();
    }
}
