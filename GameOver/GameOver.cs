using Godot;
using System;
using System.Collections.Generic;

public class GameOver : MarginContainer
{
    private LineEdit textInput;
    private HighScoreData data = new HighScoreData();
    public string userName { get; set; }
    public int score { get; set; }

    public override void _Ready()
    {
        RichTextLabel ScoreLabel = GetNode<RichTextLabel>("VBoxContainer/HBoxContainer1/ScoreLabel");        
        ScoreLabel.Text = score.ToString();
    }

    public void onSubmitButtonPressed()
    {
        GD.Print("Submit button pressed");
        textInput = GetNode<LineEdit>("VBoxContainer/HBoxContainer/UserNameEntry");
        userName = textInput.Text;

        List<HighScore> List = data.List;
        List.Add(new HighScore(){ 
            UserName = userName, 
            Score = score
        });
        data.List = List;

        HighScoreTable table = (HighScoreTable)GD.Load<PackedScene>("res://HighScores/HighScoreTable.tscn").Instance();
        GetTree().Root.AddChild(table);
        QueueFree();

    }
}
