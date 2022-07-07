using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Hopper
{   
    public class Pointer : Node2D
    {
        public Location Start;
        public Location CurrentPond 
        { 
            get 
            {
                foreach (Location l in Locations)
                {
                    if (l.Position.IsEqualApprox(Position))
                    {
                        return l;
                    }
                }
                return null;
            } 
            set
            {} 
        }

        private List<Location> Locations;
        private List<PondLinkPath> Paths;

        private PondLinkPath currentPath = null;
        private PathFollow2D currentPathFollow = null;

        private float MovementDirection = 1;

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            Start = GetNode<Location>("../Start");
            CurrentPond = Start;
        }

        public void SetLocations(List<Location> locations)
        {
            Locations = locations;
        }

        public void MoveToMenuPosition(Vector2 position)  //FIXME: What's going on here?
        {
            CurrentPond = Start;
            Position = position;
        }

        public override void _PhysicsProcess(float delta)
        {
            if (currentPathFollow != null)
            {
                float unitOffset = Mathf.Clamp(currentPathFollow.UnitOffset + (MovementDirection * 1.6f * delta), 0 , 1);
                currentPathFollow.UnitOffset = unitOffset;
                Position = currentPathFollow.GlobalPosition;
                
                if ((unitOffset >= 1 && MovementDirection == 1) || 
                    (unitOffset <= 0 && MovementDirection == -1))
                {
                    ResetPaths();
                    currentPathFollow = null;
                    currentPath = null;
                }
            }
        }

        public override void _Input(InputEvent @event)
        {           
            if (@event.IsActionPressed("ui_left"))
            {
                currentPath = GetPath("Left");
            }
            else if (@event.IsActionPressed("ui_right"))
            {
                currentPath = GetPath("Right");
            }
            else if (@event.IsActionPressed("ui_down"))
            {
                currentPath = GetPath("Down");
            }
            else if (@event.IsActionPressed("ui_up"))
            {
                currentPath = GetPath("Up");
            }
            else if (@event.IsActionPressed("ui_accept"))
            {
                if (PointerOnWorld()) LoadWorld();
            }

            if (currentPath != null)
            {
                currentPathFollow = currentPath.GetNode<PathFollow2D>("PathFollow2D");
            }
        }

        private List<PondLinkPath> GetPaths()
        {
            return CurrentPond.GetChildren().OfType<PondLinkPath>().ToList();
        }

        private PondLinkPath GetPath(string actionName)
        {
            if (CurrentPond != null)
            {
                foreach (var path in GetPaths())
                {
                    foreach (string Direction in path.Directions)
                    {
                        if (Direction == actionName) return path;
                    }
                }
            }
            return null;
        }

        private void ResetPaths()
        {
            foreach (var path in GetPaths())
            {
                path.GetNode<PathFollow2D>("PathFollow2D").UnitOffset = 0;
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
            world.Connect(nameof(World.UnlockNextWorld), Map, nameof(Map.UnlockWorld), new Godot.Collections.Array{CurrentPond.LocationsToUnlock});
            world.Init(CurrentPond.ID, CurrentPond.Levels, Position);
        }

        private bool PointerOnStart()
        {
            if (Position == Start.Position) return true;
            return false;
        }

        private bool PointerOnWorld()
        {
            foreach (Location l in Locations)
            {
                if (l.Position.IsEqualApprox(Position) && Position != Start.Position)
                {
                    CurrentPond = l;
                    return true;
                }
            }
            return false;
        }
    }
}