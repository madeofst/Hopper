using Godot;
using System.Linq;
using System;
using System.Collections.Generic;

namespace Hopper
{
    public class Location : Node2D
    {
        [Export]
        public int ID;

        [Export]
        public string Pond;
        
        [Export]
        public string[] Levels;

        private Texture texture;
        
        [Export]
        public Texture Texture 
        { 
            get => texture; 
            set
            {
                GD.Print("Try update texture");
                texture = value;
                //tool.UpdateTexture(texture);
            }
        }

        private Sprite Sprite;
        private AnimationPlayer AnimationPlayer;

        [Export]
        public bool Active;

        [Export]
        public bool NewlyActivated;

        [Export]
        public bool Complete;

        [Export]
        public string[] LocationsToUnlock;

        private LocationTool tool;

        public override void _Ready()
        {
            Sprite = GetNode<Sprite>("Sprite");
            AnimationPlayer = Sprite.GetNode<AnimationPlayer>("AnimationPlayer");
            Sprite.Texture = Texture;
            //tool = GetNode<LocationTool>("Tool");
        }

        public void Activate(Location CurrentStage)
        {
            Active = true;
            NewlyActivated = true;
            foreach (PondLinkPath p in GetChildren().OfType<PondLinkPath>())
            {
                if (p.Name == CurrentStage.Name) p.Active = true;
            }
        }

        public void UnlockAllPaths()
        {
            foreach (PondLinkPath p in GetChildren().OfType<PondLinkPath>())
            {
                p.Active = true;
            }
        }

        public override void _Process(float delta)
        {
            if (NewlyActivated) 
                AnimationPlayer.Play("New");
            else if (Complete)
                AnimationPlayer.Play("Complete");
            else if (Active) 
                AnimationPlayer.Play("Active");
            else
                AnimationPlayer.Play("Inactive");
        }

    }
}
