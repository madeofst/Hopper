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

        public void Init(SaveData SaveData)
        {
            Locations = GetChildren().OfType<Location>().ToList<Location>();
            LoadSavedData(SaveData);
            Pointer = GetNode<Pointer>("Pointer");
            Pointer.SetLocations(Locations);
            Tween = GetNode<Tween>("Tween");
            Camera = GetNode<MapCamera>("MapCamera");
            CallDeferred(nameof(ConnectToPauseMenuAndHUD));
            CallDeferred(nameof(ActivateLocations));
            Pointer.CurrentLocation.UnlockAllPaths();
        }

        private void LoadSavedData(SaveData SaveData)
        {
            foreach (LocationData ld in SaveData.LocationDataList)
            {
                Location location = FindLocationByID(ld.ID);
                if (location == null)
                {
                    GD.Print("Location not found.");
                }
                else
                {
                    location.Active = ld.Active;
                    location.NewlyActivated = ld.NewlyActivated;
                    location.Complete = ld.Complete;
                    location.LevelReached = ld.LevelReached;
                }
            }
        }

        private Location FindLocationByID(int id)
        {
            foreach (Location l in Locations)
            {
                if (l.ID == id) return l;
            }
            return null;
        }

        public void ConnectSaveSignal(Stage Stage)
        {
            Stage.Connect(nameof(Stage.SaveState), this, nameof(SaveState));
        }

        private void SaveState()
        {
            SaveData SaveData = ResourceLoader.Load<SaveData>("res://Saving/DefaultSaveFile.tres");
            
            SaveData.Init(Locations.Count);
            SaveData.CurrentLocationId = Pointer.CurrentLocation.ID;

            for (int i = 0; i < Locations.Count; i++)
            {
                LocationData LocationData = ResourceLoader.Load<LocationData>("res://Saving/DefaultLocationDataFile.tres");
                LocationData.ID = Locations[i].ID;
                LocationData.CurrentLocationName = Locations[i].Name;
                LocationData.Active = Locations[i].Active;
                LocationData.NewlyActivated = Locations[i].NewlyActivated;
                LocationData.Complete = Locations[i].Complete;
                LocationData.LevelReached = Locations[i].LevelReached;
                LocationData.TakeOverPath($"user://LocationDataSaveFile{i}.tres");
                ResourceSaver.Save($"user://LocationDataSaveFile{i}.tres", LocationData);
                SaveData.LocationDataList[i] = ResourceLoader.Load<LocationData>($"user://LocationDataSaveFile{i}.tres");
            }

            SaveData.TakeOverPath("user://SaveFile.tres");
            ResourceSaver.Save("user://SaveFile.tres", SaveData);
        }

        private void ActivateLocations()
        {
            foreach (Location l in Locations)
            {
                if (l.Active) 
                {
                    l.Activate(Locations); //TODO: this may need to be another position
                }
            }
        }

        private void ConnectToPauseMenuAndHUD()
        {
            HUD = GetNode<HUD>("/root/HUD");
            PauseMenu = GetNode<PauseMenu>("/root/PauseMenu");
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
            StartMenu.UpdateLoadButton();
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
                if (l.Active == false)
                {
                    l.Active = true;
                    l.Activate(Locations);
                }
            }
            Pointer.CurrentLocation.UnlockAllPaths();
            Pointer.CurrentLocation.NewlyActivated = false; //TODO: need to make this happen when even one level has been completed
            Pointer.CurrentLocation.Complete = true;
            Pointer.CurrentLocation.UpdateAnimationState();
        }

        public void UpdateLocationProgress(int LevelReached)
        {
            Pointer.CurrentLocation.UpdateLocationProgress(LevelReached);
        }

        public void UpdateActivationState(int LevelReached)
        {
            Pointer.CurrentLocation.UpdateActivationState(LevelReached);
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
            //PauseMenu.SetPosition(Pointer.CurrentLocation.Position - new Vector2(240, 135));
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
            node.GetViewport().MoveChild(node, node.GetViewport().GetChildCount());
        }
    }
}