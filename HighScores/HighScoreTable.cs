using Godot;
using System;
using System.Collections.Generic;

public class HighScoreTable : MarginContainer
{
    private HighScoreData data = new HighScoreData();
    private List<HighScore> highScores { get; set; }

    private List<RichTextLabel> UserNameControlList = new List<RichTextLabel>(10);
    private List<RichTextLabel> ScoreControlList = new List<RichTextLabel>(10);

    public override void _Ready()
    {
        highScores = data.List;
        CallDeferred("GetChildReferences");
    }

    private void GetChildReferences()
    {
        for (int i = 0; i < UserNameControlList.Capacity; i++)
        {
            string labelPath = $"VSplitContainer/VBoxContainer/HSplitContainer{i+1}/";
            string userNamePath = $"{labelPath}UserNameLabel{i+1}";
            
            UserNameControlList.Add(GetNode<RichTextLabel>($"{labelPath}UserNameLabel{i+1}"));
            UserNameControlList[i].Text = highScores[i].UserName;

            ScoreControlList.Add(GetNode<RichTextLabel>($"{labelPath}ScoreLabel{i+1}"));
            ScoreControlList[i].Text = highScores[i].Score.ToString();
        }
    }
}
