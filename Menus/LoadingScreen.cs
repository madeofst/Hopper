using Godot;
using System;

namespace Hopper
{
    public class LoadingScreen : Node2D
    {
        private StartMenu StartMenu;
        private PauseMenu PauseMenu;
        private LevelTitleScreen LevelTitleScreen;
        private AudioRepository AudioRepo;
        private Node2D MapContainer;

        public override void _Ready()
        {
            MapContainer = (Node2D)GD.Load<PackedScene>("res://Map/MapContainer.tscn").Instance();
            StartMenu = (StartMenu)GD.Load<PackedScene>("res://Menus/StartMenu.tscn").Instance();
            PauseMenu = (PauseMenu)GD.Load<PackedScene>("res://Menus/PauseMenu.tscn").Instance();
            LevelTitleScreen = (LevelTitleScreen)GD.Load<PackedScene>("res://Menus/LevelTitleScreen.tscn").Instance();
            AudioRepo = (AudioRepository)GD.Load<PackedScene>("res://Music/AudioRepository.tscn").Instance();
            CallDeferred(nameof(AddMenusToTree));
        }

        private void AddMenusToTree()
        {
            GetViewport().AddChild(MapContainer);
            AudioRepo.Visible = false;
            GetViewport().AddChild(AudioRepo);
            GetViewport().AddChild(StartMenu);
            StartMenu.UpdateLoadButton();
            PauseMenu.Visible = false;
            GetViewport().AddChild(PauseMenu);
            LevelTitleScreen.Visible = false;
            GetViewport().AddChild(LevelTitleScreen);
        }


    }
}