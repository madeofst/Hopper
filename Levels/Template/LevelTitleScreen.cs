using Godot;
using System;
using System.Collections.Generic;

public class LevelTitleScreen : Control
{
    private int levelID;
    private int LevelIDLabel 
    { 
        get => levelID; 
        set
        {
            levelID = value;
            _LevelIDLabel.BbcodeText = value.ToString();
        }
    }
    private int maximumHops;
    private int MaximumHopsLabel
    {
        get => maximumHops; 
        set
        {
            maximumHops = value;
            _MaximumHopsLabel.BbcodeText = value.ToString();
        }
    }
    private int requiredScore;
    private int RequiredScoreLabel
    {
        get => requiredScore; 
        set
        {
            requiredScore = value;
            _RequiredScoreLabel.BbcodeText = value.ToString();
        }
    }

    public bool AnimationActive { get; private set; }

    private HBoxContainer LevelID;
    private HBoxContainer MaximumHops;
    private HBoxContainer RequiredScore;

    private RichTextLabel _LevelIDLabel;
    private RichTextLabel _MaximumHopsLabel;
    private RichTextLabel _RequiredScoreLabel;

    private List<HBoxContainer> Containers;
    private int i = 0;

    private milliTimer timer;

    [Signal]
    public delegate void ActivatePlayer();

    public override void _Ready()
    {
        timer = new milliTimer();

        LevelID = GetNode<HBoxContainer>("LevelTitle/AllContainers/LevelID/LevelID");
        _LevelIDLabel = GetNode<RichTextLabel>("LevelTitle/AllContainers/LevelID/LevelID/Value");
        MaximumHops = GetNode<HBoxContainer>("LevelTitle/AllContainers/Text/MaximumHops");
        _MaximumHopsLabel = GetNode<RichTextLabel>("LevelTitle/AllContainers/Text/MaximumHops/Value");
        RequiredScore = GetNode<HBoxContainer>("LevelTitle/AllContainers/Text/ScoreTarget");
        _RequiredScoreLabel = GetNode<RichTextLabel>("LevelTitle/AllContainers/Text/ScoreTarget/Value");
    }

    public void Init(int levelID, int maxHops, int reqScore)
    {
        Visible = true;
        LevelIDLabel = levelID;
        MaximumHopsLabel = maxHops;
        RequiredScoreLabel = reqScore;
    }

    public void Animate()
    {
        Containers = new List<HBoxContainer>() 
        {
            //LevelID,
            MaximumHops,
            RequiredScore
        };
        foreach (HBoxContainer c in Containers) c.Visible = false;
        AnimationActive = true;
        timer.Start(0.3f);
    }

    public override void _PhysicsProcess(float delta)
    {
        if (timer.Finished() && AnimationActive)
        {
            if (i >= Containers.Count)
            {
                i = 0;
                AnimationActive = false;
            }

            if (i < Containers.Count)
            {
                Containers[i].Visible = true;
                i++;
                timer.Reset();
            }
        }
    }

    public override void _Input(InputEvent @event)
    {
        if (@event.IsActionReleased("ui_accept"))
        {
            FadeAndHide();
            EmitSignal(nameof(ActivatePlayer));
        }
    }

    private void FadeAndHide()
    {
        Visible = false;
    }
}
