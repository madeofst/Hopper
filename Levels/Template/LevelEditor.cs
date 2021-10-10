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
        LevelData levelData = ResourceLoader.Load<LevelData>("res://Levels/Template/LevelData.tres");
        levelData.TileType = new Type[2];
        levelData.TileType[0] = Type.Goal;
        levelData.TileType[1] = Type.Score;
        ResourceSaver.Save("res://Levels/Template/Level_1_Data.tres", levelData);

        LevelData levelData1 = ResourceLoader.Load<LevelData>("res://Levels/Template/Level_1_Data.tres");
    }
}
