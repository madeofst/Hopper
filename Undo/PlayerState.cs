using System;
using Godot;
using System.Collections.Generic;

namespace Hopper
{
    public class PlayerState
    {
        public Vector2 GridPosition;
        public Vector2 FacingDirection;
        public int Score;

        public PlayerState(Vector2 gridPosition, Vector2 facingDirection, int score)
        {
            GridPosition = gridPosition;
            FacingDirection = facingDirection;
            Score = score;
        }

        public void Print()
        {
            GD.Print("Grid position " + GridPosition);
            GD.Print("Facing direction " + FacingDirection);
            GD.Print("Score " + Score);
        }

    }
}