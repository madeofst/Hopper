using Godot;
using System;

namespace Hopper
{
    public class LoadingScreen : Node2D
    {
        private StartMenu StartMenu;
        private LevelTitleScreen LevelTitleScreen;
        private AudioRepository AudioRepo;
        private bool DEVMODE;
        private Node2D MapContainer;

        public override void _Ready()
        {
            Input.SetMouseMode(Input.MouseMode.Hidden);
            MapContainer = (Node2D)GD.Load<PackedScene>("res://Map/MapContainer.tscn").Instance();
            StartMenu = (StartMenu)GD.Load<PackedScene>("res://Menus/StartMenu.tscn").Instance();
            LevelTitleScreen = (LevelTitleScreen)GD.Load<PackedScene>("res://Menus/LevelTitleScreen.tscn").Instance();
            AudioRepo = GetNode<AudioRepository>("/root/AudioRepository");
            DEVMODE = GetNode<ResourceRepository>("/root/ResourceRepository").DEVMODE;
            
            if (DEVMODE)
            {
                CallDeferred(nameof(AfterAnimation));
            }
        }

        private void AfterAnimation()
        {
            GetViewport().AddChild(MapContainer);
            AudioRepo.Visible = false;
            GetViewport().AddChild(StartMenu);
            StartMenu.UpdateLoadButton();
            LevelTitleScreen.Visible = false;
            GetViewport().AddChild(LevelTitleScreen);
        }


    }
}