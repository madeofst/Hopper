using Godot;
using System;

public class ScoreBox : Control
{
    public ScoreLabel PlayerLevelScore { get; set; }
    public ScoreLabel BugsRemaining { get; set; }

    public override void _Ready()
    {
        PlayerLevelScore = GetNode<ScoreLabel>("PlayerLevelScore");
        BugsRemaining = GetNode<ScoreLabel>("BugsRemaining");
    }

    public void UpdatePlayerScore(int levelScore)
    {
        if (levelScore <= 0)
        {
            BugsRemaining.UpdateText("0", true);
        } 
        else
        {
            BugsRemaining.UpdateText(levelScore.ToString(), true);
        }
    }

    internal void Animate()
    {
        PlayerLevelScore.Shake();
        BugsRemaining.Shake();
    }
}
