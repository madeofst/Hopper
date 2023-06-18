using Godot;
using System.Linq;
using System.Collections.Generic;

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
        public delegate void ActivatePlayer();
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
            Visible = true;
            PondLabel = StageData.Pond;
            StageIDLabel = StageData.ID;

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
                if (iLevel == i + 1) LevelSelectors[i].GrabFocus();
                b.Connect(nameof(LevelSelectButton.ChangeFocus), this, nameof(UpdateFocus));
            }

            UpdateLevelReached(StageData.LevelReached);
        }

        public void UpdateLevelReached(int LevelReached)
        {
            foreach (TextureButton b in LevelSelectors)
            {
                if (int.Parse(b.Name) <= LevelReached)
                {
                    b.FocusMode = FocusModeEnum.All;
                    b.TextureNormal = GD.Load<Texture>("res://Menus/Resources/ExampleLeaf3.png");
                }
                else
                {
                    b.FocusMode = FocusModeEnum.None;
                    b.Disabled = true;
                }
            }
        }

        public void UpdateFocus(string name)
        {
            EmitSignal(nameof(SelectLevel), int.Parse(name));
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
                }
                else
                {
                    GD.Print($"{Shader} {Shader.GetShaderParam("fill").ToString()} fill + {delta * Speed * FillDirection}");
                    Shader.SetShaderParam("fill", Mathf.Clamp(fill + delta * Speed * FillDirection, 0, 1));            
                }
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
                 @event is InputEventScreenTouch && @event.IsPressed())
                 && !Animating && Visible)
            {
                SetProcessInput(false);
                EmitSignal(nameof(Unpause));
                AnimateHide();
            }
            else if (@event.IsActionPressed("ui_map"))
            {
                EmitSignal("BackToMap");
            }
            else if (@event.IsActionPressed("ui_quit"))
            {
                EmitSignal("QuitToMenu");
            }
        }

        private void MoveLevelSelection(Vector2 direction)
        {
            //GD.Print($"Select {direction}");
        }

        public void ClickToHide()
        {
            SetProcessInput(false);
            if (!Animating & Visible) AnimateHide();
        }
    }
}
