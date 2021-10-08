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

    public Level(Type[] Types) //TODO: the parameter will be in the form of a resource or possible just a json file with a list of types
    {
        //TODO: check resource is valid (e.g. square number of types, valid types etc.)
    }
    
}