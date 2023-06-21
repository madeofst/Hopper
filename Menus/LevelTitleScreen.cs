using Godot;
using System.Linq;
using System.Collections.Generic;
using System;

namespace Hopper
{
    public class LevelTitleScreen : Control
    {
        private string PondLabel 
        { 
            set
            {
                Color colour = new Color();
                if (value == "Hawkins")
                {
                    colour = Color.Color8(36, 51, 41);
                }
                else if (value == "BelAir")
                {
                    colour = Color.Color8(245, 197, 168);
                }
                else //(value == "Liffey")
                {
                    colour = Color.Color8(217, 165, 232);
                }
                GetNode<TextureRect>("Background").Modulate = colour;
                GetNode<TextureRect>("LevelTitle/PondName").Texture = GD.Load<Texture>($"res://Menus/Resources/{value}Pond.png");
            }
        }
        private int stageID;
        private int StageIDLabel 
        { 
            get => stageID; 
            set
            {
                stageID = value;
            }
        }
        public StageData StageData { get; private set; }
        private int iLevel { get; set; }

        public int FillDirection { get; private set; }
        public float Speed { get; private set; }
        public bool Animating { get; private set; }
        public bool Triggered { get; private set; }

        //private TitleElement StageID;
        //private RichTextLabel _StageIDLabel;

        private HBoxContainer LevelButtons;
        private List<TextureButton> LevelSelectors = new List<TextureButton>();

        private ShaderMaterial Shader;

        [Signal]
        public delegate void ShowTouchButtons();
        [Signal]
        public delegate void ShowScoreBox();
        [Signal]
        public delegate void TitleScreenLoaded();
        [Signal]
        public delegate void StartMusic();
        [Signal]
        public delegate void SelectLevel();
        [Signal]
        public delegate void Unpause();
        [Signal]
        public delegate void BackToMap();
        [Signal]
        public delegate void QuitToMenu();

        public override void _Ready()
        {
            Shader = (ShaderMaterial)Material;
            LevelButtons = GetNode<HBoxContainer>("LevelTitle/LevelSelector/LevelButtons");
        }

        public void Init(StageData StageData, int LevelCount, int iLevel, int maxHops, int reqScore)
        {
            StopInput();
            Animating = true;
            Visible = true;
            PondLabel = StageData.Pond;
            StageIDLabel = StageData.ID;
            this.StageData = StageData;
            this.iLevel = iLevel;

            LevelSelectors.Clear();
            foreach (Node n in LevelButtons.GetChildren())
            {
                LevelButtons.RemoveChild(n);
                n.QueueFree();
            }

            for (int i = 0; i < LevelCount; i++)
            {
                TextureButton b = (TextureButton)GD.Load<PackedScene>("res://Menus/LevelSelectButton.tscn").Instance();
                b.Name = i.ToString();
                LevelButtons.AddChild(b);
                LevelSelectors.Add(b);
                b.Connect(nameof(LevelSelectButton.ChangeFocus), this, nameof(UpdateFocus));
            }
        }

        public void UpdateLevelReached(int LevelReached)
        {
            foreach (TextureButton b in LevelSelectors)
            {
                if (int.Parse(b.Name) <= LevelReached)
                {
                    b.FocusMode = FocusModeEnum.All;
                    b.TextureNormal = GD.Load<Texture>("res://Menus/Resources/ExampleLeaf3.png");
                    b.Disabled = false;
                    if (int.Parse(b.Name) == iLevel - 1) b.GrabFocus();
                }
                else
                {
                    b.FocusMode = FocusModeEnum.None;
                    b.Disabled = true;
                }
                b.Visible = true;
            }
        }

        public void UpdateFocus(string name)
        {
            if (!Animating)
            {
                EmitSignal(nameof(SelectLevel), int.Parse(name));
            }
        }

        public void AnimateShow()
        {
            FillDirection = 1;
            Speed = 1.5f;
            Animating = true;
        }

        public void AnimateHide()
        {
            EmitSignal(nameof(StartMusic));

            foreach (LevelSelectButton b in LevelSelectors) b.Visible = false;
            FillDirection = -1;
            Speed = 3;
            Animating = true;
        }

        public override void _PhysicsProcess(float delta)
        {
            float fill = (float)Shader.GetShaderParam("fill");

            if ((FillDirection == 1 && fill >= 1) && !Triggered)
            {
                Triggered = true;
                EmitSignal(nameof(TitleScreenLoaded));
            }

            if (Animating)
            {
                if ((FillDirection == 1 && fill >= 1) || (FillDirection == -1 && fill <= 0))
                {
                    Animating = false;
                    if (FillDirection == 1 && fill >= 1) StartInput();
                }
                else
                {
                    //GD.Print($"{Shader} {Shader.GetShaderParam("fill").ToString()} fill + {delta * Speed * FillDirection}");
                    Shader.SetShaderParam("fill", Mathf.Clamp(fill + delta * Speed * FillDirection, 0, 1));            
                }
            }
            else
            {
                if (FillDirection == -1 && fill <= 0 && Visible == true)
                {
                    Visible = false;
                    Triggered = false;
                    EmitSignal(nameof(Unpause));
                    EmitSignal(nameof(ShowTouchButtons));
                }
                else if (FillDirection == 1 && fill >= 1)
                {
                    
                }
            }
        }

        public override void _Input(InputEvent @event)
        {
            if (!Animating && Visible)
            {
                if ((@event.IsActionPressed("ui_accept") ||
                    @event is InputEventScreenTouch && @event.IsPressed()))
                {
                    StopInput();
                    AnimateHide();
                }
                else if (@event.IsActionPressed("ui_map"))
                {
                    StopInput();
                    EmitSignal("BackToMap");
                }
                else if (@event.IsActionPressed("ui_quit"))
                {
                    StopInput();
                    EmitSignal("QuitToMenu");
                }
                else if (@event.IsActionPressed("ui_cancel"))
                {
                    StopInput();
                    EmitSignal("BackToMap");
                }
            }
        }

        private void StartInput()
        {
            //UpdateLevelReached(StageData.LevelReached);
            SetProcessInput(true);
        }

        private void StopInput()
        {
            SetProcessInput(false);
            foreach (TextureButton b in LevelSelectors)
            {
                b.Disabled = true;
                b.FocusMode = FocusModeEnum.None;
            }
        }

        private void MoveLevelSelection(Vector2 direction)
        {
            //GD.Print($"Select {direction}");
        }

        public void ClickToHide()
        {
            StopInput();
            if (!Animating & Visible) AnimateHide();
        }
    }
}
