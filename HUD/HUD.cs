using Godot;
using System;
using System.Collections.Generic;

namespace Hopper
{
    public class HUD : Control
    {
        private AnimationPlayer AnimationPlayer;
        private RichTextLabel PopUp;
        private HopCounter HopCounter;
        private ScoreBox ScoreBox;

        private LevelTitleScreen LevelTitleScreen;

        public Control TouchButtons;
        public OverlayMenu OverlayMenu;

        //Touch controls
        public TouchScreenButton Restart;
        public TouchScreenButton Quit;

        private bool PositionLocked = true;

        public override void _Ready()
        {
            LevelTitleScreen = GetNode<LevelTitleScreen>("../LevelTitleScreen");
            ConnectToLevelTitle();
            AnimationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");    
            PopUp = GetNode<RichTextLabel>("PopUpText/RichTextLabel");
            HopCounter = GetNode<HopCounter>("HopCounter");
            ScoreBox = GetNode<ScoreBox>("ScoreBox");
            Restart = GetNode<TouchScreenButton>("TouchButtons/Restart");
            TouchButtons = GetNode<Control>("TouchButtons");
            OverlayMenu = GetNode<OverlayMenu>("OverlayMenu");
        }

        private void ConnectToLevelTitle()
        {
            LevelTitleScreen.Connect(nameof(LevelTitleScreen.ShowTouchButtons), this, nameof(ShowTouchButtons));
            LevelTitleScreen.Connect(nameof(LevelTitleScreen.ShowScoreBox), this, nameof(ShowScoreBox));
        }

        private void DisconnectFromLevelTitle()
        {
            LevelTitleScreen.Disconnect(nameof(LevelTitleScreen.ShowTouchButtons), this, nameof(ShowTouchButtons));
            LevelTitleScreen.Disconnect(nameof(LevelTitleScreen.ShowScoreBox), this, nameof(ShowScoreBox));
        }

        internal void Close()
        {
            DisconnectFromLevelTitle();
            QueueFree();
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

        public void ShowTouchButtons()
        {
            TouchButtons.Visible = true;
        }

        public void HideHopCounter()
        {
            HopCounter.Visible = false;
        }

        public void ShowHopCounter()
        {
            HopCounter.Visible = true;
        }

        public void UpdateScore(int updatedScore)
        {
            ScoreBox.UpdatePlayerScore(updatedScore);
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
            ScoreBox.BugsRemaining.UpdateText(score.ToString(), postGoal);
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
