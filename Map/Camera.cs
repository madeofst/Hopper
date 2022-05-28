using Godot;
using System;

namespace Hopper
{
    public class Camera : Camera2D
    {
        Pointer Pointer;

        public override void _Ready()
        {
            Pointer = GetNode<Pointer>("../Pointer");
        }
        public override void _Process(float delta)
        {
            Position = Pointer.Position;
        }
    }
}
