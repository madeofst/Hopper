using Godot;
using System;

namespace Hopper
{
    public class HopCounterBar : TextureRect
    {
        public Player Player { get; set; }

        public HopCounterBar(){}

        public override void _Ready()
        {
            CallDeferred("ConnectNodesAndSignals");
        }

        public void ConnectNodesAndSignals()
        {
            Player = GetNode<Player>("/root/World/Player");
            MakeConnections();
        }

        public void UpdateBar(int HopsRemaining)
        {
            RectSize = new Vector2(RectSize.x, 20 * HopsRemaining);
        }

        public virtual void MakeConnections()
        {
            Player.Connect(nameof(Player.HopCompleted), this, "UpdateBar");
        }
    }
}