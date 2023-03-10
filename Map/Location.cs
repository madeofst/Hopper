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
            }
        }

        private Sprite Sprite;
        private Sprite ActivationSprite;
        private AnimationPlayer AnimationPlayer;
        private AnimationPlayer ActivationAnimationPlayer;

        [Export]
        public bool Active;

        [Export]
        private bool newlyActivated;

        public bool NewlyActivated 
        { 
            get => newlyActivated; 
            set
            {
                if (value)
                {
                    ActivationAnimationPlayer.Play("NewlyActivated");
                }
                else
                {
                    ActivationAnimationPlayer.Stop();
                }
                newlyActivated = value;  
            } 
        }

        [Export]
        public bool Complete;

        [Export]
        public string[] LocationsToUnlock;

        public int LevelReached;
        //private LocationTool tool;
        private LocationProgress LocationProgress;


        public override void _Ready()
        {
            Sprite = GetNode<Sprite>("Sprite");
            AnimationPlayer = Sprite.GetNode<AnimationPlayer>("AnimationPlayer");
            ActivationSprite = GetNode<Sprite>("ActivationSprite");
            ActivationAnimationPlayer = ActivationSprite.GetNode<AnimationPlayer>("AnimationPlayer");
            Sprite.Texture = Texture;
        }

        public void Activate(Location CurrentStage)
        {
            Active = true;
            NewlyActivated = true;
            foreach (PondLinkPath p in GetChildren().OfType<PondLinkPath>())
            {
                if (p.Name == CurrentStage.Name) p.Active = true;
            }
            
            LocationProgress = GetNode<LocationProgress>("LocationProgress");
            foreach (string level in Levels)
            {
                Sprite LevelSprite = (Sprite)GD.Load<PackedScene>("res://Map/LevelSprite.tscn").Instance();
                LocationProgress.AddChild(LevelSprite);
            }
            LocationProgress.Init(LevelReached);
            UpdateAnimationState();
        }

        public void UnlockAllPaths()
        {
            foreach (PondLinkPath p in GetChildren().OfType<PondLinkPath>())
            {
                p.Active = true;
            }
        }

        public void UpdateAnimationState()
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

        internal void UpdateActivationState(int levelReached)
        {
            if (levelReached > 0) ActivationAnimationPlayer.Play("Stopped");
        }

        public void UpdateLocationProgress(int levelReached)
        {
            LevelReached = levelReached;
            LocationProgress.Update(levelReached);
        }
    }
}
