using Godot;
using System;

namespace Hopper
{
    public class LoadingScreen : Node2D
    {
        private StartMenu StartMenu;
        private PauseMenu PauseMenu;
        private LevelTitleScreen LevelTitleScreen;

        public override void _Ready()
        {
            StartMenu = (StartMenu)GD.Load<PackedScene>("res://Menus/StartMenu.tscn").Instance();
            PauseMenu = (PauseMenu)GD.Load<PackedScene>("res://Menus/PauseMenu.tscn").Instance();
            LevelTitleScreen = (LevelTitleScreen)GD.Load<PackedScene>("res://Menus/LevelTitleScreen.tscn").Instance();
            CallDeferred(nameof(AddMenusToTree));
        }

        private void AddMenusToTree()
        {
            GetTree().Root.AddChild(StartMenu);
            PauseMenu.Visible = false;
            GetTree().Root.AddChild(PauseMenu);
            LevelTitleScreen.Visible = false;
            GetTree().Root.AddChild(LevelTitleScreen);
        }
    }
}