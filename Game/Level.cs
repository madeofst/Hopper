using System;
using Godot;

public class Level : Node2D
{
    public Grid Grid;

    public int ID;
    public int GridSize;
    public int GoalsToNextLevel;
    public int ScoreTileCount;

    public int MaxHops;
    public int HopsToAdd;

    public Type[] Types; //TODO: to be populated by the constructor (or init method(s))
    
    public Level(int id, int gridSize, int maxHops, int scoreTileCount, int goalsToNextLevel = 10, int hopsToAdd = 0) //TODO: turn this into a Generate() method?
    {
        ID = id;
        GridSize = gridSize;
        MaxHops = maxHops;
        GoalsToNextLevel = goalsToNextLevel;
        if (hopsToAdd == 0)
        {
            HopsToAdd = (int)Math.Floor((decimal)MaxHops/2);
        }
        else
        {
            HopsToAdd = hopsToAdd;
        }
        ScoreTileCount = scoreTileCount;
    }

    public Level(){}

    public override void _Ready()
    {
        Grid = GetNode<Grid>("Grid");
    }
    
    public void BuildGrid(int size, int tileSize, LevelData levelData = null)
    {
        //build grid using levelData.TileType info (if provided)
        Grid.ClearExistingChildren();
        Grid.NewDefineGridParameters(new Vector2(tileSize, tileSize), size);
        int i = 0;
        for (int y = 0; y < size - 1; y++)
        {
            for (int x = 0; x < size - 1; x++)
            {
            Grid.Tiles[x, y] = new Tile($"Tile{x}-{y}");
            Grid.AddChild(Grid.Tiles[x, y]);
            if (levelData != null)
            {
                Grid.Tiles[x, y].BuildTile(levelData.TileType[i], new Vector2(tileSize, tileSize), new Vector2(x, y));
            }
            else
            {
                Grid.Tiles[x, y].BuildTile(Type.Blank, new Vector2(tileSize, tileSize), new Vector2(x, y));
            }
            Grid.Tiles[x, y].Owner = this;
            i++;
            }
        }
        i++;
    }
}