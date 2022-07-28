using Godot;
using System;

namespace Hopper
{
    public class MapCamera : Camera2D
    {
        Pointer Pointer;

        public override void _Ready()
        {
            Pointer = GetNode<Pointer>("../Pointer");
        }
        public override void _Process(float delta)
        {
            Position = Position.MoveToward(Pointer.Position,  delta * 180); // TODO: Camera needs to work
        }

        public void MoveTo(Vector2 position)
        {
            Position = position;
        }
    }
}
