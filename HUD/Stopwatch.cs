using Godot;
using System;

namespace Hopper
{
    public class Stopwatch : TextureProgress
    {
        public World World { get; set; }

        public Stopwatch(){}

        public override void _Ready()
        {
            CallDeferred("ConnectNodesAndSignals");
        }

        public void ConnectNodesAndSignals()
        {
            World = GetNode<World>("/root/World");
            MakeConnections();
        }

        public void UpdateStopwatch(int SecsRemaining)
        {
            Value = SecsRemaining;
        }

        public virtual void MakeConnections()
        {
            World.Connect(nameof(World.TimeUpdate), this, "UpdateStopwatch");
        }
    }
}