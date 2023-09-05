using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Hopper
{   
    public class Pointer : Node2D
    {
        public Location Start;
        public Location CurrentLocation 
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
            {
                Position = value.Position;
            } 
        }

        private List<Location> Locations;
        //private List<PondLinkPath> Paths;

        private AnimationTree AnimationTree;
        private AnimationNodeStateMachinePlayback AnimationState;

        private PondLinkPath currentPath = null;
        private PathFollow2D currentPathFollow = null;

        private float MovementDirection = 1;

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            Start = GetNode<Location>("../Start");  //TODO: This needs to be updated on load
            AnimationTree = GetNode<AnimationTree>("AnimationTree");
            AnimationState = (AnimationNodeStateMachinePlayback)AnimationTree.Get("parameters/playback");
            SetProcessInput(false);
            //CurrentLocation = Start;
        }

        public void SetLocations(List<Location> locations)
        {
            Locations = locations;
        }

        public void SetCurrentLocation(Location location)
        {
            CurrentLocation = location;
        }

        public void MoveToMenuPosition(Vector2 position)
        {
            //This just helps the visual transition from map to menu
            //CurrentLocation = Start;
            //Position = position;
        }

        public override void _PhysicsProcess(float delta)
        {
            Vector2 movementVector = Vector2.Zero;
            if (currentPathFollow != null)
            {
                //float unitOffset = Mathf.Clamp(currentPathFollow.UnitOffset + (MovementDirection * 1f * delta), 0 , 1);
                float Offset = currentPathFollow.Offset + (MovementDirection * 160f * delta);
                if (currentPathFollow.UnitOffset <= 1) currentPathFollow.Offset = Offset;
                movementVector = (currentPathFollow.GlobalPosition - Position).Normalized();
                Position = currentPathFollow.GlobalPosition;
                
                //GD.Print(Position);

                if ((currentPathFollow.UnitOffset >= 1 && MovementDirection == 1) || 
                    (currentPathFollow.UnitOffset <= 0 && MovementDirection == -1))
                {
                    ResetPaths();
                    currentPathFollow = null;
                    currentPath = null;
                    AnimationState.Travel("Idle");
                }
                else
                {
                    AnimationTree.Set("parameters/Idle/blend_position", movementVector);
                    AnimationTree.Set("parameters/Jump/blend_position", movementVector);
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
                if (PointerOnStage()) LoadStage();
            }
            else if (@event.IsActionPressed("ui_quit"))
            {
                GetParent<Map>().QuitToMenu();
            }

            if (currentPath != null)
            {
                currentPathFollow = currentPath.GetNode<PathFollow2D>("PathFollow2D");
                AnimationState.Travel("Jump");
            }
        }

        private List<PondLinkPath> GetPaths()
        {
            //GD.Print(CurrentLocation.GetChildren());
            return CurrentLocation.GetChildren().OfType<PondLinkPath>().ToList();
        }

        private PondLinkPath GetPath(string actionName)
        {
            if (CurrentLocation != null)
            {
                foreach (var path in GetPaths())
                {
                    foreach (string Direction in path.Directions)
                    {
                        if (Direction == actionName && path.Active) return path;
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

        private void LoadStage()
        {
            SetProcessInput(false);
            
            Map Map = GetNode<Map>("..");
            Map.SetProcessInput(false);

            Stage Stage = (Stage)GD.Load<PackedScene>("res://Stage/Stage.tscn").Instance();
            Stage.Visible = false;
            GetNode<Viewport>("/root/StageContainer/ViewportContainer/Viewport").AddChild(Stage);

            Stage.Connect(nameof(Stage.UnlockNextStage), 
                          Map, 
                          nameof(Map.UnlockConnectedStages));
            Stage.Connect(nameof(Stage.UpdateLocationProgress),
                          Map,
                          nameof(Map.UpdateLocationProgress));
                          
            Stage.Init(new StageData(CurrentLocation.ID, CurrentLocation.Pond, CurrentLocation.LevelReached), 
                       CurrentLocation.Levels, 
                       Position, 
                       false, 
                       "", 
                       Map);

            //Map.HUD.OverlayMenu.ChangeMode(OverlayMenuMode.LevelTitle);
        }

        private bool PointerOnStart()
        {
            if (Position == Start.Position) return true;
            return false;
        }

        private bool PointerOnStage()
        {
            foreach (Location l in Locations)
            {
                if (l.Position.IsEqualApprox(Position) && Position != Start.Position)
                {
                    //CurrentLocation = l;
                    return true;
                }
            }
            return false;
        }
    }
}