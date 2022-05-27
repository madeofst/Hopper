using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Hopper
{   
    public class Pointer : Node2D
    {
        private Location Start;
        private Location Target;
        private List<Location> Locations;

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            Start = GetNode<Location>("../Start");
            Target = Start;
            Position = Start.Position;
        }

        public void SetLocations(List<Location> locations)
        {
            Locations = locations; 
        }

        public override void _Process(float delta)
        {
            Position = Position.MoveToward(Target.Position,  delta * 800);
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
                Location l = GetFirstLocationInDirection(direction);
                if (l != null) Target = l;
            } 
        }

        private void LoadWorld()
        {
            Map Map = GetNode<Map>("..");
            SetProcessInput(false);
            World world = (World)GD.Load<PackedScene>("res://World/World.tscn").Instance();
            GetTree().Root.AddChildBelowNode(Map, world);
            world.Init(Target.Levels);
        }

        private bool PointerOnWorld()
        {
            foreach (Location l in Locations)
            {
                if (l.Position == Position && Position != Start.Position) return true;
            }
            return false;
        }

        private Location GetFirstLocationInDirection(Vector2 direction)
        {
            List<float> dotProducts = new List<float>();
            Location target = null;
            foreach (Location l in Locations)
            {
                float a = Position.DirectionTo(l.Position).Dot(direction);
                dotProducts.Add(a);
            }
            float max = dotProducts.Max();
            if (max > 0.2) target = Locations[dotProducts.IndexOf(max)];
            return target;
        }
    }
}