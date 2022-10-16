using Godot;
using System;

namespace Hopper
{
    public class Stopwatch : TextureProgress
    {
        public Stage Stage { get; set; }

        public Stopwatch(){}

        public override void _Ready()
        {
            // CallDeferred("ConnectNodesAndSignals"); //FIXME: needs fixing
        }

        public void ConnectNodesAndSignals()
        {
            Stage = GetNode<Stage>("/root/GameContainer/ViewportContainer/Viewport/ViewportContainer/Viewport/Stage");
            MakeConnections();
        }

        public void UpdateStopwatch(int SecsRemaining)
        {
            Value = SecsRemaining;
        }

        public virtual void MakeConnections()
        {
            //Stage.Connect(nameof(Stage.TimeUpdate), this, "UpdateStopwatch");
        }
    }
}