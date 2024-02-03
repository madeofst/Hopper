using Godot;
using System;

namespace Hopper
{
    public class TileChangeInstruction : Godot.Object
    {
        public int ActionOnTurn;
        public Vector2 TileGridPosition;
        public Type TileType;
        public int Score;
        public bool Eaten;
        public Vector2 BounceDirection;
        public bool Activated;

        public TileChangeInstruction(int actionOnTurn, Tile tile)
        {
            ActionOnTurn = actionOnTurn;
            TileGridPosition = tile.GridPosition;
            TileType = tile.Type;
            Score = tile.PointValue;
            Eaten = !tile.BugSprite.Visible;
            BounceDirection = tile.BounceDirection;
            Activated = tile.Activated;
        }

        public TileChangeInstruction(int actionOnTurn, Vector2 tileGridPosition, Type tileType, int score, bool eaten, Vector2 bounceDirection, bool activated)
        {
            ActionOnTurn = actionOnTurn;
            TileGridPosition = tileGridPosition;
            TileType = tileType;
            Score = score;
            Eaten = eaten;
            BounceDirection = bounceDirection;
            Activated = activated;
        }

        public int[] Serialize()
        {           
            return new int[] {
                ActionOnTurn, 
                (int)TileGridPosition.x, (int)TileGridPosition.y, 
                (int)TileType, 
                Score, 
                Convert.ToInt32(Eaten),
                (int)BounceDirection.x, (int)BounceDirection.y,
                Convert.ToInt32(Activated)
            };
        }
    }
}
