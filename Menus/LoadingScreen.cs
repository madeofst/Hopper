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

        public override void _Ready()
        {
            StartMenu = (StartMenu)GD.Load<PackedScene>("res://Menus/StartMenu.tscn").Instance();
            PauseMenu = (PauseMenu)GD.Load<PackedScene>("res://Menus/PauseMenu.tscn").Instance();
            LevelTitleScreen = (LevelTitleScreen)GD.Load<PackedScene>("res://Menus/LevelTitleScreen.tscn").Instance();
            AudioRepo = (AudioRepository)GD.Load<PackedScene>("res://Music/AudioRepository.tscn").Instance();
            CallDeferred(nameof(AddMenusToTree));
        }

        private void AddMenusToTree()
        {
            AudioRepo.Visible = false;
            GetViewport().AddChild(AudioRepo);
            GetViewport().AddChild(StartMenu);
            PauseMenu.Visible = false;
            GetViewport().AddChild(PauseMenu);
            LevelTitleScreen.Visible = false;
            GetViewport().AddChild(LevelTitleScreen);
        }
    }
}