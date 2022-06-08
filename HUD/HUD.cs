using Godot;
using System;

namespace Hopper
{
    public class HUD : Control
    {
        private AnimationPlayer AnimationPlayer;
        private RichTextLabel PopUp;
        private HopCounter HopCounter;
        private ScoreBox ScoreBox;
        private MapCamera Camera;

        //Touch controls
        public TouchScreenButton Restart;
        public TouchScreenButton Quit;

        private bool PositionLocked = true;

        public override void _Ready()
        {
            AnimationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");    
            PopUp = GetNode<RichTextLabel>("PopUpText/RichTextLabel");
            HopCounter = GetNode<HopCounter>("HopCounter");
            ScoreBox = GetNode<ScoreBox>("ScoreBox");
            Camera = GetNode<MapCamera>("../Map/MapCamera");
            Restart = GetNode<TouchScreenButton>("TouchButtons/Restart");
        }

        public override void _Process(float delta)
        {
            if (!PositionLocked && Camera.Name != null)
                RectPosition = Camera.Position - new Vector2(240, 135);
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

        public void HideScoreBox()
        {
            ScoreBox.Visible = false;
        }

        public void ShowScoreBox()
        {
            ScoreBox.Visible = true;
        }

        public void HideHopCounter()
        {
            HopCounter.Visible = false;
        }

        public void ShowHopCounter()
        {
            HopCounter.Visible = true;
        }

        public void UpdateScore(int totalScore, int levelScore, int minScore)
        {
            ScoreBox.UpdatePlayerScore(totalScore, levelScore, minScore);
        }

        public void SetButtonToRestart()
        {
            Restart.Normal = GD.Load<Texture>("res://HUD/Resources/Restart.png");
            Restart.Action = "ui_restart";
        }

        public void SetButtonToEnter()
        {
            Restart.Normal = GD.Load<Texture>("res://HUD/Resources/Enter.png");
            Restart.Action = "ui_accept";
        }

        public void UpdateMinScore(int score, bool postGoal)
        {
            ScoreBox.LevelMinScore.UpdateText(score.ToString(), postGoal);
        }

        public void AnimateScoreBox()
        {
            ScoreBox.Animate();
        }

        public void SetMaxHops(int maxHops)
        {
            HopCounter.SetMaxHops(maxHops);
        }

        public void UpdateHop(int hopsRemaining)
        {
            HopCounter.UpdateHop(hopsRemaining);
        }

        public void CountInActiveHops()
        {
            HopCounter.CountIn();
        }
    }
}
