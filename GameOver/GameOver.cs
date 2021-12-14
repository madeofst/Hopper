using Godot;
using System;
using System.Collections.Generic;

namespace Hopper
{
    public class GameOver : MarginContainer
    {
        private LineEdit TextInput;
        private HighScoreData Data = new HighScoreData();
        public AudioStreamPlayer2D Music { get; set; }
        
        public string userName { get; set; }
        public int Score { get; set; }

        public RichTextLabel ScoreLabel { get; set; }

        public override void _Ready()
        {
            Music = GetNode<AudioStreamPlayer2D>("AudioStreamPlayer2D");
            ScoreLabel = GetNode<RichTextLabel>("VBoxContainer/HBoxContainer1/ScoreLabel");        
        }

        public void onSubmitButtonPressed()
        {
            GD.Print("Submit button pressed");
            TextInput = GetNode<LineEdit>("VBoxContainer/HBoxContainer/UserNameEntry");
            userName = TextInput.Text;

            List<HighScore> List = Data.List;
            List.Add(new HighScore(){ 
                UserName = userName, 
                Score = Score
            });
            Data.List = List;

            HighScoreTable table = (HighScoreTable)GD.Load<PackedScene>("res://HighScores/HighScoreTable.tscn").Instance();
            GetTree().Root.AddChild(table);
            QueueFree();

        }
    }
}