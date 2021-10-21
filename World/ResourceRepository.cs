using System;
using Godot;

namespace Hopper
{
    public class ResourceRepository
    {
        public PackedScene LilyScene { get; set; }
        public PackedScene GoalOffScene { get; private set; }
        public PackedScene GoalOnScene { get; private set; }
        public PackedScene ScoreScene { get; private set; }
        public PackedScene RockScene { get; private set; }
        public PackedScene JumpScene { get; private set; }

        public ResourceRepository()
        {
            LilyScene = ResourceLoader.Load<PackedScene>("res://Levels/Template/Tile_Lily.tscn");
            GoalOffScene = ResourceLoader.Load<PackedScene>("res://Levels/Template/Tile_Goal_Off.tscn");
            GoalOnScene = ResourceLoader.Load<PackedScene>("res://Levels/Template/Tile_Goal_On.tscn");
            ScoreScene = ResourceLoader.Load<PackedScene>("res://Levels/Template/Tile_Score.tscn");
            RockScene = ResourceLoader.Load<PackedScene>("res://Levels/Template/Tile_Rock.tscn");
            JumpScene = ResourceLoader.Load<PackedScene>("res://Levels/Template/Tile_Jump_Ready.tscn");
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
                case Type.Jump:
                    return JumpScene;
            }
            return null;
        }
    }
}