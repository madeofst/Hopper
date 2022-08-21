using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Hopper
{
    public class Map : Node2D
    {
        public HUD HUD;
        public List<Location> Locations;
        public Pointer Pointer;
        private PauseMenu PauseMenu;
        private Tween Tween;
        private MapCamera Camera;
        
        public override void _Ready()
        {
            Locations = GetChildren().OfType<Location>().ToList<Location>();
            Pointer = GetNode<Pointer>("Pointer");
            Pointer.SetLocations(Locations);
            Tween = GetNode<Tween>("Tween");
            Camera = GetNode<MapCamera>("MapCamera");
            CallDeferred(nameof(ConnectToPauseMenuAndHUD));
            CallDeferred(nameof(ActivateLocations));
        }

        private void ActivateLocations()
        {
            foreach (Location l in Locations)
            {
                if (l.Active) l.Activate(Pointer.Start);
            }
        }

        private void ConnectToPauseMenuAndHUD()
        {
            HUD = GetNode<HUD>("../HUD");
            PauseMenu = GetNode<PauseMenu>("../PauseMenu");
            ConnectPauseSignals();
        }

        public void ConnectPauseSignals()
        {
            PauseMenu.QuitButton.Connect("pressed", this, nameof(QuitToMenu));
            PauseMenu.Connect(nameof(PauseMenu.Quit), this, nameof(QuitToMenu));
            PauseMenu.Connect(nameof(PauseMenu.Unpause), this, nameof(Unpause));
        }

        public void DisconnectPauseSignals()
        {
            PauseMenu.QuitButton.Disconnect("pressed", this, nameof(QuitToMenu));
            PauseMenu.Disconnect(nameof(PauseMenu.Quit), this, nameof(QuitToMenu));
            PauseMenu.Disconnect(nameof(PauseMenu.Unpause), this, nameof(Unpause));
        }

        private void QuitToMenu()
        {
            StartMenu StartMenu = GetNode<StartMenu>("/root/StartMenu");
            Pointer.MoveToMenuPosition(StartMenu.RectPosition);
            Camera.MoveTo(Pointer.Position);
            PauseMenu.RectPosition = StartMenu.RectPosition;
            HUD.Close();
            PauseMenu.AnimateHide();
            QueueFree();
            StartMenu.ShowMenu();
        }

        public void UnlockStage(string[] StagesToUnlock)
        {
            foreach (var s in StagesToUnlock)
            {
                Location l = GetNode<Location>((string)s);
                if (l.Active == false) l.Activate(Pointer.CurrentLocation);
            }
            Pointer.CurrentLocation.UnlockAllPaths();
            Pointer.CurrentLocation.NewlyActivated = false;
            Pointer.CurrentLocation.Complete = true;
        }

        public void UpdateLocationProgress(int LevelReached)
        {
            Pointer.CurrentLocation.UpdateLocationProgress(LevelReached);
        }

        public override void _Process(float delta)
        {
            foreach (Location l in Locations)
            {
                if (Pointer.Position == l.Position)
                {
                    l.Scale = new Vector2(1.2f, 1.2f);
                }
                else
                {
                    l.Scale = new Vector2(1f, 1f);
                }
            }
        }

        public void FadeIn()
        {
            Tween.InterpolateProperty(this, "modulate", new Color(1, 1, 1, 0), new Color(1, 1, 1, 1), 0.5f, Tween.TransitionType.Sine, Tween.EaseType.In);
			Tween.Start();
            Pointer.Position = Pointer.Start.Position;
        }

        public override void _Input(InputEvent @event)
        {
            if (@event.IsActionPressed("ui_cancel")) Pause();
        }

        private void Pause()
        
        {
            Pointer.SetProcessInput(false);
            SetProcessInput(false);
            MoveToTop(PauseMenu);
            PauseMenu.SetPosition(Pointer.CurrentLocation.Position - new Vector2(240, 135));
            PauseMenu.Visible = true;
            PauseMenu.SetMode(PauseMenu.PauseMenuMode.Map);
            PauseMenu.AnimateShow();
        }

        private void Unpause()
        {
            if (PauseMenu.Mode == PauseMenu.PauseMenuMode.Map)
            {
                Pointer.SetProcessInput(true);
                SetProcessInput(true);
            }
        }

        private void MoveToTop(Node node = null)
        {
            if (node == null) node = this;
            GetTree().Root.MoveChild(node, GetTree().Root.GetChildCount());
        }
    }
}