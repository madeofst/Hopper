using Godot;
using System;

namespace Hopper
{
    public class HUD : Control
    {
        private AnimationPlayer AnimationPlayer;
        private RichTextLabel PopUp;
        public HopCounter HopCounter;
        public ScoreBox ScoreBox;
        public MapCamera Camera;

        public Button Restart;
        public Button Quit;

        private bool PositionLocked = true;

        public override void _Ready()
        {
            AnimationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");    
            PopUp = GetNode<RichTextLabel>("PopUpText/RichTextLabel");
            HopCounter = GetNode<HopCounter>("HopCounter");
            ScoreBox = GetNode<ScoreBox>("ScoreBox");
            Camera = GetNode<MapCamera>("../Map/MapCamera");
        }

        public void ShowPopUp(string text)
        {
            PopUp.BbcodeText = $"[center]{text}[/center]";
            AnimationPlayer.Play("ShowPopUpText");
        }

        public void LockPosition(Vector2 position)
        {
            PositionLocked = true;
            RectPosition = position;
        }

        public void UnlockPosition()
        {
            PositionLocked = false;
        }

        public override void _Process(float delta)
        {
            if (!PositionLocked && Camera != null) 
                RectPosition = Camera.Position - new Vector2(240, 135);
        }
    }
}
