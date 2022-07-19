using Godot;
using System;
using System.Collections.Generic;

namespace Hopper
{
    public class PauseMenu : Control
    {
        public enum PauseMenuMode
        {
            Map,
            Stage,
            Editor
        }

        public PauseMenuMode Mode;

        public AnimatedButton RestartButton;
        public AnimatedButton QuitButton;
        public AnimatedButton MapButton;
        private List<AnimatedButton> Buttons;

        private int FillDirection { get; set; }
        private int Speed { get; set; }
        public bool Animating { get; private set; }
        public bool Loaded { get; private set; }
        
        private ShaderMaterial Shader;

        [Signal]
        public delegate void Unpause();
        [Signal]
        public delegate void Restart();
        [Signal]
        public delegate void Map();
        [Signal]
        public delegate void Quit();

        public override void _Ready()
        {
            RestartButton = GetNode<AnimatedButton>("MenuButtons/Buttons/RestartButton");
            QuitButton = GetNode<AnimatedButton>("MenuButtons/Buttons/QuitButton");
            MapButton = GetNode<AnimatedButton>("MenuButtons/Buttons/MapButton");
            Shader = (ShaderMaterial)Material;
            
            Buttons = new List<AnimatedButton>() 
            {
                RestartButton,
                MapButton,
                QuitButton
            };
        }

        public void SetMode(PauseMenuMode pauseMenuMode)
        {
            Mode = pauseMenuMode;
            RestartButton.Visible = false;
            QuitButton.Visible = false;
            MapButton.Visible = false;

            if (Mode == PauseMenuMode.Map)
            {
                Buttons = new List<AnimatedButton>() 
                {
                    QuitButton
                };
            }
            else if (Mode == PauseMenuMode.Stage)
            {
                MapButton.GetNode<RichTextLabel>("Labels/TextLabel").Text = "Back to map";
                Buttons = new List<AnimatedButton>() 
                {
                    RestartButton,
                    MapButton,
                    QuitButton
                };
            }
            else if (Mode == PauseMenuMode.Editor)
            {
                MapButton.GetNode<RichTextLabel>("Labels/TextLabel").Text = "To editor";
                Buttons = new List<AnimatedButton>() 
                {
                    RestartButton,
                    MapButton
                };
            }
            else
            {
                throw new NotImplementedException();
            }

            foreach (AnimatedButton b in Buttons) b.Visible = true;
        }

        public void AnimateShow()
        {
            FillDirection = 1;
            Speed = 1;
            Animating = true;

            float delay = 0.2f;
            foreach (AnimatedButton b in Buttons)
            {
                b.Modulate = new Color(b.Modulate, 0);
                b.Tween.InterpolateProperty(b, "rect_scale", Vector2.Zero, Vector2.One, 0.9f, Tween.TransitionType.Elastic, Tween.EaseType.Out, delay);
                b.Tween.InterpolateProperty(b, "modulate", new Color(1, 1, 1, 0), new Color(1, 1, 1, 1), 0.5f, Tween.TransitionType.Sine, Tween.EaseType.In, delay - 0.2f);
                b.Tween.Start();
                delay += 0.3f;
            }
            Buttons[0].GrabFocus();
        }

        public void AnimateHide()
        {
            FillDirection = -1;
            Speed = 3;
            Animating = true;
            Loaded = false;

            float delay = 0f;
            foreach (AnimatedButton b in Buttons)
            {
                b.Tween.InterpolateProperty(b, "rect_scale", Vector2.One, Vector2.Zero, 0.5f, Tween.TransitionType.Expo, Tween.EaseType.Out, delay);
                b.Tween.InterpolateProperty(b, "modulate", new Color(1, 1, 1, 1), new Color(1, 1, 1, 0), 0.5f, Tween.TransitionType.Sine, Tween.EaseType.In, delay);
                b.Tween.Start();
                delay += 0.1f;
            }
        }

        public override void _PhysicsProcess(float delta)
        {
            float fill = (float)Shader.GetShaderParam("fill");

            if ((FillDirection == 1 && fill >= 1) && !Loaded)
            {
                Animating = false;
                Loaded = true;
            }

            if (!Loaded)
            {
                if (FillDirection == -1 && fill <= 0)
                {
                    if (Visible == true)
                    {
                        Animating = false;
                        Visible = false;
                        Loaded = false;
                        EmitSignal(nameof(Unpause));
                    }
                }
                else if (Animating)
                {           
                    Shader.SetShaderParam("fill", Mathf.Clamp(fill + delta * Speed * FillDirection, 0, 1));
                }
            }
        }

        public override void _Input(InputEvent @event)
        {
            if (@event.IsActionPressed("ui_cancel") && Visible && Loaded && !Animating)
            {   
                AnimateHide();
            }
            else if (@event.IsActionPressed("ui_restart") && Visible && Loaded && !Animating)
            {
                EmitSignal(nameof(Restart));
            }
            else if (@event.IsActionPressed("ui_map") && Visible && Loaded && !Animating)
            {
                EmitSignal(nameof(Map));
            }
            else if (@event.IsActionPressed("ui_quit") && Visible && Loaded && !Animating)
            {
                EmitSignal(nameof(Quit));
            }
        }

    }
}