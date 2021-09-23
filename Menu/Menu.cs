using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class Menu : MarginContainer
{
    public override void _Ready()
    {
    }

    public void newGamePressed()
    {
        GD.Print("New game button pressed.");
        World world = (World)GD.Load<PackedScene>("res://Game/World.tscn").Instance();
        GetTree().Root.AddChild(world);
        Hide();
    }

    public void highScoresPressed()
    {
        GD.Print("High scores button pressed.");
        HighScoreTable table = (HighScoreTable)GD.Load<PackedScene>("res://HighScores/HighScoreTable.tscn").Instance();
        GetTree().Root.AddChild(table);
        Hide();

/*         HighScoreData data = new HighScoreData();
        
        data.Init();
        data.Add(new HighScore {
            UserName = "Zo",
            Score = 23000
        });

        List<HighScore> highScores = data.List;
        int id = 1;
        foreach (HighScore hs in highScores)
        {
            GD.Print($"{id}: {hs.UserName} - {hs.Score}.");
            id ++;
        } */
    }
}
