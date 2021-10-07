using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class HighScoreTable : MarginContainer
{
    private HighScoreData Data { get; set; }
    private List<HighScore> HighScores { get; set; }
    private Menu Menu { get; set; }

    private List<RichTextLabel> UserNameControlList { get; set; }
    private List<RichTextLabel> ScoreControlList { get; set; }

    public override void _Ready()
    {
        Data = new HighScoreData();
        HighScores = Data.List;

        Menu = GetNode<Menu>("/root/Menu");

        CallDeferred("GetChildReferences");
    }

    private void GetChildReferences()
    {
        UserNameControlList = new List<RichTextLabel>();
        ScoreControlList = new List<RichTextLabel>();

        for (int i = 0; i < 10; i++)
        {
            string labelPath = $"VBoxContainer/VBoxContainer/HSplitContainer{i+1}/";
            string userNamePath = $"{labelPath}UserNameLabel{i+1}";
            
            RichTextLabel UserNameLabel = GetNode<RichTextLabel>($"{labelPath}UserNameLabel{i+1}");
            UserNameControlList.Add(UserNameLabel);
            UserNameControlList[i].Text = HighScores[i].UserName;

            RichTextLabel ScoreLabel = GetNode<RichTextLabel>($"{labelPath}ScoreLabel{i+1}");
            ScoreControlList.Add(ScoreLabel);
            ScoreControlList[i].Text = HighScores[i].Score.ToString();
        }
    }

    private void OnBackButtonPressed()
    {
        Menu.Show();
        QueueFree();
    }
}
