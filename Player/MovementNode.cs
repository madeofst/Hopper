using System;
using Godot;

namespace Hopper
{
    public class MovementNode
    {
        public Tile Tile;
        public Vector2 MovementDirection;
        public bool Submerged;

        public MovementNode(Tile Tile, Vector2 MovementDirection, bool Submerged)
        {
            this.Tile = Tile;
            this.MovementDirection = MovementDirection;
            this.Submerged = Submerged;
        }
    }
}