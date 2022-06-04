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
            Position = Position.MoveToward(Pointer.Target.Position,  delta * 600);
        }

        public void MoveTo(Vector2 position)
        {
            Position = position;
        }
    }
}
