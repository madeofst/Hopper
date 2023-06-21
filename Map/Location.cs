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
                    ActivationAnimationPlayer.Play("Stopped");
                }
                newlyActivated = value;  
            } 
        }

        [Export]
        public bool Complete;

        [Export]
        public string[] LocationsToUnlock;

        [Export]
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

        public void Activate(List<Location> Locations)
        {
            Active = true;
            
            if (LevelReached == 0) NewlyActivated = true;

            foreach (PondLinkPath p in GetChildren().OfType<PondLinkPath>())
            {
                if (!LocationsToUnlock.Any(loc => loc.Equals(p.Name)))
                {
                    p.Active = true;
                }
            }

            foreach (Location l in Locations)
            {
                foreach (PondLinkPath p in l.GetChildren().OfType<PondLinkPath>())
                {
                    if ((this.Name == p.Name) && (!LocationsToUnlock.Any(loc => loc.Equals(p.Name))))
                    {
                        p.Active = true;
                    }
                }
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


        private Location FindLocationByName(List<Location> Locations, string LocationName)
        {
            foreach (Location l in Locations)
            {
                if (l.Name == LocationName) return l;
            }
            return null;
        }

        public void UnlockAllPaths()
        {
            foreach (PondLinkPath p in GetChildren().OfType<PondLinkPath>())
            {
                p.AnimateReveal();
            }
        }


        public void MarkComplete()
        {
            NewlyActivated = false; //TODO: need to make this happen when even one level has been completed
            Complete = true;
            UpdateAnimationState();
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
            if (levelReached > 0) NewlyActivated = false;
        }

        public void UpdateLocationProgress(int levelReached)
        {
            LevelReached = levelReached;
            LocationProgress.Update(levelReached); //FIXME: Location progress null on world 11 after 1st level
        }
    }
}
