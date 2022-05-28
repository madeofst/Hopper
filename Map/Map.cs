using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Hopper
{
    public class Map : Node2D
    {
        public List<Location> Locations;
        public Pointer Pointer;
        
        public override void _Ready()
        {
            Locations = GetChildren().OfType<Location>().ToList<Location>();
            Pointer = GetNode<Pointer>("Pointer");
            Pointer.SetLocations(Locations);
        }

        public void UnlockWorld(string[] worldsToUnlock)
        {
            foreach (var s in worldsToUnlock)
            {
                Location l = GetNode<Location>((string)s);
                l.Active = true;
                l.NewlyActivated = true;
            }
            Pointer.Target.NewlyActivated = false;
            Pointer.Target.Complete = true;
        }

        public override void _Process(float delta)
        {
            foreach (Location l in Locations)
            {
                if (Pointer.Position == l.Position)
                {
                    l.Scale = new Vector2(1.2f, 1.2f);
                }
                else
                {
                    l.Scale = new Vector2(1f, 1f);
                }
            }
        }
    }
}