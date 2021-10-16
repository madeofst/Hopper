using Godot;
using System;

namespace Hopper
    {
    public class TimeCounter : Counter
    {
        public TimeCounter(){}
        
        public TimeCounter(Vector2 size) : base(size)
        {
        }

        public override void MakeConnections()
        {
            //World.Connect(nameof(World.TimeUpdate), this, "UpdateText");
        }

    }
}