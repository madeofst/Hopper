using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Hopper
{   
    public class Pointer : Node2D
    {
        public Location Start;
        public Location Target;
        private List<Location> Locations;

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            Start = GetNode<Location>("../Start");
            Target = Start;
        }

        public void SetLocations(List<Location> locations)
        {
            Locations = locations; 
        }

        public override void _Process(float delta)
        {
            Position = Position.MoveToward(Target.Position,  delta * 375);
        }

        public override void _Input(InputEvent @event)
        {
            Vector2 direction = Vector2.Inf;

            if (@event.IsActionPressed("ui_left"))
                direction = Vector2.Left;
            else if (@event.IsActionPressed("ui_right"))
                direction = Vector2.Right;
            else if (@event.IsActionPressed("ui_down"))
                direction = Vector2.Down;
            else if (@event.IsActionPressed("ui_up"))
                direction = Vector2.Up;
            else if (@event.IsActionPressed("ui_accept"))
            {
                if (PointerOnWorld()) LoadWorld();
            }

            if (direction != Vector2.Inf)
            {
                Location l = GetNearestLocationInDirection(direction);
                if (l != null) Target = l;
            } 
        }

        private void LoadWorld()
        {
            Map Map = GetNode<Map>("..");
            Map.SetProcessInput(false);
            SetProcessInput(false);
            World world = (World)GD.Load<PackedScene>("res://World/World.tscn").Instance();
            world.Visible = false;
            GetTree().Root.AddChildBelowNode(Map, world);
            world.Connect(nameof(World.UnlockNextWorld), Map, nameof(Map.UnlockWorld), new Godot.Collections.Array{Target.LocationsToUnlock});
            world.Init(Target.ID, Target.Levels, Position);

        }

        private bool PointerOnWorld()
        {
            foreach (Location l in Locations)
            {
                if (l.Position == Position && Position != Start.Position) return true;
            }
            return false;
        }

        private Location GetNearestLocationInDirection(Vector2 direction)
        {
            List<float> distances = new List<float>();
            Location target = null;
            foreach (Location l in Locations)
            {
                float a = Position.DirectionTo(l.Position).Dot(direction);
                if (a > 0.2 && l.Active)
                {
                    distances.Add(Position.DistanceTo(l.Position));
                }
                else
                {
                    distances.Add(float.MaxValue);
                }
            }
            float min = distances.Min();
            if (min < float.MaxValue) target = Locations[distances.IndexOf(min)];
            return target;
        }
    }
}