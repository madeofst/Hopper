using Godot;
using System;

public class LevelEditor : Node2D
{
    private Level CurrentLevel { get; set; }

    public override void _Ready()
    {
    }

    private void NewBlankLevel()
    {
        if (CurrentLevel != null) CurrentLevel.QueueFree();
        CurrentLevel = NewLevel(7);
        CurrentLevel.Editable = true;
        //CurrentLevel.Grid.UpdateGrid();
    }

    private Level NewLevel(int size, int tileSize = 32, string name = null)
    {
        if (size > 7) return null;
        Level level = (Level)GD.Load<PackedScene>("res://Levels/Template/Level.tscn").Instance();
        AddChild(level);
        LevelData levelData;
        if (name == null)
        {
            levelData = LoadBlankLevelData(size);
        }
        else
        {
            levelData = LoadBlankLevelData(size); //TODO: pass the level name here
        }
        if (levelData == null) return null; //TODO: maybe also check if all types are valid here
        level.BuildGrid(size, tileSize, levelData);
        return level;
    }

    private LevelData LoadBlankLevelData(int size) //TODO: make this load any level
    {
        LevelData levelData = ResourceLoader.Load<LevelData>("res://Levels/Template/LevelData.tres");
        levelData.TileType = new Type[size*size];
        for (int i = 0; i < levelData.TileType.Length - 1; i++)
        {
            levelData.TileType[i] = Type.Blank;
        }
        return levelData;
    }
}
