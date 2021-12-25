using Godot;
using System;
using System.Collections.Generic;

public class LevelTitleScreen : Control
{
    private int levelID;
    private int LevelID 
    { 
        get => levelID; 
        set
        {
            levelID = value;
            LevelIDLabel.BbcodeText = value.ToString();
        }
    }
    private int maximumHops;
    private int MaximumHops
    {
        get => maximumHops; 
        set
        {
            maximumHops = value;
            MaximumHopsLabel.BbcodeText = value.ToString();
        }
    }
    private int requiredScore;
    private int RequiredScore
    {
        get => requiredScore; 
        set
        {
            requiredScore = value;
            RequiredScoreLabel.BbcodeText = value.ToString();
        }
    }

    public bool AnimationActive { get; private set; }

    private RichTextLabel LevelIDLabel;
    private RichTextLabel MaximumHopsLabel;
    private RichTextLabel RequiredScoreLabel;

    private List<RichTextLabel> Labels;
    private int i = 0;

    private milliTimer timer = new milliTimer();

    public override void _Ready()
    {
        LevelIDLabel = GetNode<RichTextLabel>("LevelTitle/Text/MarginContainer/LevelID/Value");
        MaximumHopsLabel = GetNode<RichTextLabel>("LevelTitle/Text/MaximumHops/Value");
        RequiredScoreLabel = GetNode<RichTextLabel>("LevelTitle/Text/ScoreTarget/Value");
    }

    public void Init(int levelID, int maxHops, int reqScore)
    {
        Visible = true;
        LevelID = levelID;
        MaximumHops = maxHops;
        RequiredScore = reqScore;
    }

    public void Animate()
    {
        Labels = new List<RichTextLabel>() 
        {
            LevelIDLabel,
            MaximumHopsLabel,
            RequiredScoreLabel
        };
        foreach (RichTextLabel l in Labels) l.Visible = false;
        AnimationActive = true;
        timer.Start(0.3f);
    }

    public override void _PhysicsProcess(float delta)
    {
        if (timer.Finished() && AnimationActive)
        {
            if (i >= Labels.Count)
            {
                if (i > Labels.Count + 4)
                {
                    i = 0;
                    AnimationActive = false;
                    Visible = false;
                }
                else
                {
                    timer.Reset();
                    i++;
                }
            }
            else
            {
                Labels[i].Visible = true;
                i++;
                timer.Reset();
            }
        }
    }
}
