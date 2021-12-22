using System;
using Godot;

namespace Hopper
{
    public class ResourceRepository : Node2D
    {
        [Export]
        public PackedScene LilyScene { get; set; }
        [Export]
        public PackedScene GoalOffScene { get; private set; }
        [Export]
        public PackedScene GoalOnScene { get; private set; }
        [Export]
        public PackedScene ScoreScene { get; private set; }
        [Export]
        public PackedScene RockScene { get; private set; }
        [Export]
        public PackedScene WaterScene { get; private set; }
        [Export]
        public PackedScene JumpScene { get; private set; }

        public ResourceRepository()
        {
            
        }

        public override void _Ready()
        {
        }

        internal PackedScene LoadByType(Type type)
        {
            switch (type)
            {
                case Type.Lily:
                    return LilyScene;
                case Type.Goal:
                    return GoalOffScene;
                case Type.Score:
                    return ScoreScene;
                case Type.Rock:
                    return RockScene;
                case Type.Water:
                    return WaterScene;
                case Type.Jump:
                    return JumpScene;
            }
            return null;
        }
    }
}