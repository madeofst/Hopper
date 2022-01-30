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
    public int FillDirection { get; private set; }
    public int Speed { get; private set; }
    public bool Animating { get; private set; }

    private HBoxContainer LevelID;
    private HBoxContainer MaximumHops;
    private HBoxContainer RequiredScore;

    private RichTextLabel _LevelIDLabel;
    private RichTextLabel _MaximumHopsLabel;
    private RichTextLabel _RequiredScoreLabel;

    private List<Control> Containers;
    private int i = 0;

    private milliTimer timer;

    private ShaderMaterial Shader;

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
        Shader = (ShaderMaterial)Material;
        
        Containers = new List<Control>() 
        {
            LevelID,
            MaximumHops,
            RequiredScore
        };
    }

    public void Init(int levelID, int maxHops, int reqScore)
    {
        Visible = true;
        LevelIDLabel = levelID;
        MaximumHopsLabel = maxHops;
        RequiredScoreLabel = reqScore;
    }

    public void AnimateShow()
    {
        FillDirection = 1;
        Speed = 1;
        Animating = true;

        float i = 0.2f;
        foreach (Control c in Containers)
        {
            c.Modulate = new Color(c.Modulate, 0);
            Tween tween = c.GetNode<Tween>("Tween");
            tween.InterpolateProperty(c, "rect_scale", Vector2.Zero, Vector2.One, 0.9f, Tween.TransitionType.Elastic, Tween.EaseType.Out, i);
            tween.InterpolateProperty(c, "modulate", new Color(1, 1, 1, 0), new Color(1, 1, 1, 1), 0.5f, Tween.TransitionType.Sine, Tween.EaseType.In, i - 0.2f);
            tween.Start();
            i += 0.3f;
            
        }
    }

    public void AnimateHide()
    {
        FillDirection = -1;
        Speed = 3;
        Animating = true;

        float i = 0f;
        foreach (Control c in Containers)
        {
            Tween tween = c.GetNode<Tween>("Tween");
            tween.InterpolateProperty(c, "rect_scale", Vector2.One, Vector2.Zero, 0.5f, Tween.TransitionType.Expo, Tween.EaseType.Out, i);
            tween.InterpolateProperty(c, "modulate", new Color(1, 1, 1, 1), new Color(1, 1, 1, 0), 0.5f, Tween.TransitionType.Sine, Tween.EaseType.In, i);
            tween.Start();
            i += 0.1f;
        }
    }

    public override void _PhysicsProcess(float delta)
    {
        float fill = (float)Shader.GetShaderParam("fill");
        if ((FillDirection == 1 && fill >= 1) || (FillDirection == -1 && fill <= 0)) Animating = false;

        if (Animating)
        {           
            Shader.SetShaderParam("fill", fill + delta * Speed * FillDirection);
            //GD.Print($"{fill} - {delta * 10}");

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
            AnimateHide();
            EmitSignal(nameof(ActivatePlayer));
        }
    }
}
