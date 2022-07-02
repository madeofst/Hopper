using Godot;
using System;
using System.Collections.Generic;

namespace Hopper
{
    public class LevelTitleScreen : Control
    {
        private int worldID;
        private int WorldIDLabel 
        { 
            get => worldID; 
            set
            {
                worldID = value;
                _WorldIDLabel.BbcodeText = value.ToString();
            }
        }
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

        public int FillDirection { get; private set; }
        public int Speed { get; private set; }
        public bool Animating { get; private set; }
        public bool Triggered { get; private set; }

        private TitleElement WorldID;
        private TitleElement LevelID;
        private TitleElement MaximumHops;
        private TitleElement RequiredScore;

        private RichTextLabel _WorldIDLabel;
        private RichTextLabel _LevelIDLabel;
        private RichTextLabel _MaximumHopsLabel;
        private RichTextLabel _RequiredScoreLabel;

        private List<TitleElement> Containers;

        private ShaderMaterial Shader;

        [Signal]
        public delegate void ShowTouchButtons();
        [Signal]
        public delegate void ShowScoreBox();
        [Signal]
        public delegate void ActivatePlayer();
        [Signal]
        public delegate void LoadNextLevel();
        [Signal]
        public delegate void StartMusic();

        public override void _Ready()
        {
            LevelID = GetNode<TitleElement>("LevelTitle/AllContainers/LevelID/LevelID");
            _LevelIDLabel = LevelID.GetNode<RichTextLabel>("LevelValue");
            _WorldIDLabel = LevelID.GetNode<RichTextLabel>("WorldValue");
            MaximumHops = GetNode<TitleElement>("LevelTitle/AllContainers/Text/MaximumHops");
            _MaximumHopsLabel = GetNode<RichTextLabel>("LevelTitle/AllContainers/Text/MaximumHops/Value");
            RequiredScore = GetNode<TitleElement>("LevelTitle/AllContainers/Text/ScoreTarget");
            _RequiredScoreLabel = GetNode<RichTextLabel>("LevelTitle/AllContainers/Text/ScoreTarget/Value");
            Shader = (ShaderMaterial)Material;
            
            Containers = new List<TitleElement>() 
            {
                LevelID,
                MaximumHops,
                RequiredScore
            };
        }

        public void Init(int worldID, int levelID, int maxHops, int reqScore)
        {
            Visible = true;
            WorldIDLabel = worldID;
            LevelIDLabel = levelID;
            MaximumHopsLabel = maxHops;
            RequiredScoreLabel = reqScore;
        }

        public void AnimateShow()
        {
            FillDirection = 1;
            Speed = 1;
            Animating = true;

            float delay = 0.2f;
            foreach (TitleElement c in Containers)
            {
                c.Modulate = new Color(c.Modulate, 0);
                c.Tween.InterpolateProperty(c, "rect_scale", Vector2.Zero, Vector2.One, 0.9f, Tween.TransitionType.Elastic, Tween.EaseType.Out, delay);
                c.Tween.InterpolateProperty(c, "modulate", new Color(1, 1, 1, 0), new Color(1, 1, 1, 1), 0.5f, Tween.TransitionType.Sine, Tween.EaseType.In, delay - 0.2f);
                c.Tween.Start();
                delay += 0.3f;
            }
        }

        public void AnimateHide()
        {
            EmitSignal(nameof(StartMusic));

            FillDirection = -1;
            Speed = 3;
            Animating = true;

            float delay = 0f;
            foreach (TitleElement c in Containers)
            {
                c.Tween.InterpolateProperty(c, "rect_scale", Vector2.One, Vector2.Zero, 0.5f, Tween.TransitionType.Expo, Tween.EaseType.Out, delay);
                c.Tween.InterpolateProperty(c, "modulate", new Color(1, 1, 1, 1), new Color(1, 1, 1, 0), 0.5f, Tween.TransitionType.Sine, Tween.EaseType.In, delay);
                c.Tween.Start();
                delay += 0.1f;
            }
        }

        public override void _PhysicsProcess(float delta)
        {
            float fill = (float)Shader.GetShaderParam("fill");

            if ((FillDirection == 1 && fill >= 1) && !Triggered)
            {
                Triggered = true;
                EmitSignal(nameof(LoadNextLevel));
            }

            if ((FillDirection == 1 && fill >= 1) || (FillDirection == -1 && fill <= 0)) Animating = false;
            
            foreach (TitleElement c in Containers)
            {
                if (c.Tween.IsActive()) Animating = true;
            }

            if (Animating)
            {           
                Shader.SetShaderParam("fill", Mathf.Clamp(fill + delta * Speed * FillDirection, 0, 1));            
            }
            else
            {
                if (FillDirection == -1 && fill <= 0 && Visible == true)
                {
                    Visible = false;
                    Triggered = false;
                    EmitSignal(nameof(ActivatePlayer));
                    EmitSignal(nameof(ShowTouchButtons));
                }
                else if (FillDirection == 1 && fill >= 1)
                {
                    SetProcessInput(true);
                }
            }
        }

        public override void _Input(InputEvent @event)
        {
            if ((@event.IsActionPressed("ui_accept") ||
                 @event.IsActionReleased("ui_left") ||
                 @event.IsActionReleased("ui_up") ||
                 @event.IsActionReleased("ui_right") ||
                 @event.IsActionReleased("ui_down")) ||
                 @event is InputEventScreenTouch && @event.IsPressed()
                 && !Animating && Visible)
            {
                SetProcessInput(false);
                AnimateHide();
            }
        }

        public void ClickToHide()
        {
            SetProcessInput(false);
            if (!Animating & Visible) AnimateHide();
        }
    }
}
