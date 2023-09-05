using Godot;
using System;

namespace Hopper
{
    public class LoadingScreen : Node2D
    {
        private StartMenu StartMenu;
        private LevelTitleScreen LevelTitleScreen;
        private AudioRepository AudioRepo;
        private Node2D MapContainer;
        private Node2D StageContainer;

        public override void _Ready()
        {
            StageContainer = (Node2D)GD.Load<PackedScene>("res://Stage/StageContainer.tscn").Instance();
            MapContainer = (Node2D)GD.Load<PackedScene>("res://Map/MapContainer.tscn").Instance();
            StartMenu = (StartMenu)GD.Load<PackedScene>("res://Menus/StartMenu.tscn").Instance();
            LevelTitleScreen = (LevelTitleScreen)GD.Load<PackedScene>("res://Menus/LevelTitleScreen.tscn").Instance();
            AudioRepo = (AudioRepository)GD.Load<PackedScene>("res://Music/AudioRepository.tscn").Instance();
            CallDeferred(nameof(AddMenusToTree));
        }

        private void AddMenusToTree()
        {
            GetViewport().AddChild(MapContainer);
            GetViewport().AddChild(StageContainer);
            AudioRepo.Visible = false;
            GetViewport().AddChild(AudioRepo);
            GetViewport().AddChild(StartMenu);
            StartMenu.UpdateLoadButton();
            LevelTitleScreen.Visible = false;
            GetViewport().AddChild(LevelTitleScreen);
        }


    }
}