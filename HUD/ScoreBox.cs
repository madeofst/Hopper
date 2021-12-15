using Godot;
using System;

public class ScoreBox : Control
{
    public ScoreLabel PlayerLevelScore { get; set; }
    public ScoreLabel LevelMinScore { get; set; }

    public override void _Ready()
    {
        PlayerLevelScore = GetNode<ScoreLabel>("PlayerLevelScore");
        LevelMinScore = GetNode<ScoreLabel>("LevelMinScore");
    }

    public void UpdatePlayerScore(int totalScore, int levelScore)
    {
        PlayerLevelScore.BbcodeText = levelScore.ToString();
    }

    internal void Animate()
    {
        PlayerLevelScore.Shake();
        LevelMinScore.Shake();
    }
}
