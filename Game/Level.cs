using System;
using Godot;

public class Level
{
    public int ID;
    public int GridSize;
    public int GoalsToNextLevel;
    public int ScoreTileCount;

    public int MaxHops;
    public int HopsToAdd;

    //Sprite set or something to definte the look

    public Level(int id, int gridSize, int maxHops, int scoreTileCount, int goalsToNextLevel = 10, int hopsToAdd = 0)
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

    
}