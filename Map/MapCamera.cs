using Godot;
using System;

namespace Hopper
{
    public class MapCamera : Camera2D
    {
        private Pointer Pointer;
        private Vector2 GameSize = new Vector2(480, 270);
        private float WindowScale;
        private Vector2 ActualCamPosition;

        private ViewportContainer ViewportContainer;

        [Signal]
        public delegate void CameraArrived();

        private bool SignalSent = false;

        public override void _Ready()
        {
            WindowScale = (OS.WindowSize / GameSize).x;
            Pointer = GetNode<Pointer>("../Pointer");
            ActualCamPosition = GlobalPosition;
            ViewportContainer = GetNode<ViewportContainer>("/root/MapContainer/ViewportContainer");
        }
        public override void _PhysicsProcess(float delta)
        {
            ActualCamPosition = ActualCamPosition.MoveToward(Pointer.Position,  delta * 180); // TODO: Camera needs to work
            Vector2 SubPixelPosition = ActualCamPosition.Round() - ActualCamPosition;
            ViewportContainer.Material.Set("shader_param/cam_offset", SubPixelPosition);
            GlobalPosition = ActualCamPosition.Round();

            if (ActualCamPosition.IsEqualApprox(Pointer.Position) && !SignalSent)
            {
                EmitSignal(nameof(CameraArrived));
                SignalSent = true;
            } 
        }

        public void MoveTo(Vector2 position)
        {
            Position = position;
        }
    }
}
