using Godot;
using System;

namespace Hopper
{
    public class TileChangeInstruction
    {
        public int ActionOnTurn;
        public Vector2 TileGridPosition;
        public Type TileType;
        public int Score;
        public bool Eaten;
        public Vector2 BounceDirection;

        public TileChangeInstruction(int actionOnTurn, Vector2 tileGridPosition, Type tileType, int score, bool eaten, Vector2 bounceDirection)
        {
            ActionOnTurn = actionOnTurn;
            TileGridPosition = tileGridPosition;
            TileType = tileType;
            Score = score;
            Eaten = eaten;
            BounceDirection = bounceDirection;
        }

        public int[] Serialize()
        {           
            return new int[] {
                ActionOnTurn, 
                (int)TileGridPosition.x, (int)TileGridPosition.y, 
                (int)TileType, 
                Score, 
                Convert.ToInt32(Eaten),
                (int)BounceDirection.x, (int)BounceDirection.y
            };
        }
    }
}
