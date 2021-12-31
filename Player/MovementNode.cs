using System;
using Godot;

namespace Hopper
{
    public class MovementNode
    {
        public Tile Tile;
        public Vector2 MovementDirection;

        public MovementNode(Tile Tile, Vector2 MovementDirection)
        {
            this.Tile = Tile;
            this.MovementDirection = MovementDirection;
        }
    }
}