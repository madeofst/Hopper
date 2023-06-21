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
        private Tween Tween;
        private MapCamera Camera;

        public void Init(SaveData SaveData)
        {
            Locations = GetChildren().OfType<Location>().ToList<Location>();
            
            LoadSavedData(SaveData);

            Pointer = GetNode<Pointer>("Pointer");
            Pointer.SetLocations(Locations);
            Pointer.CurrentLocation = FindLocationByID(SaveData.CurrentLocationId);

            Camera = GetNode<MapCamera>("MapCamera");
            Camera.Connect(nameof(MapCamera.CameraArrived), this, nameof(ActivateInitialLocations));

            Tween = GetNode<Tween>("Tween");

            CallDeferred(nameof(ConnectToPauseMenuAndHUD));
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
                    location.Complete = ld.Complete;
                    location.LevelReached = ld.LevelReached;
                    if (ld.Active) location.Activate(Locations);
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

        private void ActivateInitialLocations()
        {
            if (Pointer.CurrentLocation.Complete)
            {
                UnlockStage(Pointer.CurrentLocation.LocationsToUnlock);
            }
        }

        private void ConnectToPauseMenuAndHUD()
        {
            HUD = GetNode<HUD>("/root/HUD");
            ConnectPauseSignals();
        }

        public void ConnectPauseSignals()
        {
            if (HUD.OverlayMenu.QuitButton.IsConnected("pressed", this, nameof(QuitToMenu)))
                DisconnectPauseSignals();
            HUD.OverlayMenu.QuitButton.Connect("pressed", this, nameof(QuitToMenu));
        }

        public void DisconnectPauseSignals()
        {
            HUD.OverlayMenu.QuitButton.Disconnect("pressed", this, nameof(QuitToMenu));
        }

        public void QuitToMenu()
        {
            StartMenu StartMenu = GetNode<StartMenu>("/root/StartMenu");
            StartMenu.UpdateLoadButton();
            Pointer.MoveToMenuPosition(StartMenu.RectPosition);
            Camera.MoveTo(Pointer.Position);
            HUD.Close();
            QueueFree();
            StartMenu.ShowMenu();
        }

        public void UnlockStage(string[] StagesToUnlock)
        {
            if (!Pointer.CurrentLocation.Complete)
            {
                Pointer.CurrentLocation.MarkComplete();
                Pointer.CurrentLocation.UnlockAllPaths();
            }
            Pointer.SetProcessInput(true);
        }

        public void ActivateLocation(string[] LocationName)
        {
            Location Location = Locations.Find(l => l.Name == LocationName[0]);
            if (Location != null)
            {
                Location.Activate(Locations);
            }
            else
            {
                GD.Print("Location not found.");
            }
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
        }

        private void Pause()
        {
            Pointer.SetProcessInput(false);
            SetProcessInput(false);
        }

        private void Unpause()
        {
        }

        private void MoveToTop(Node node = null)
        {
            if (node == null) node = this;
            node.GetViewport().MoveChild(node, node.GetViewport().GetChildCount());
        }
    }
}