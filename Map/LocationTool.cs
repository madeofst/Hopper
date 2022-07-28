using Godot;
using System.Collections.Generic;
using System.Linq;

namespace Hopper
{
    [Tool]
    public class LocationTool: Node2D
    {
        private Location Stage;
        private List<PondLinkPath> Paths;

        public override void _Ready()
        {
            Stage = GetParent<Location>();
            Paths = Stage.GetChildren().OfType<PondLinkPath>().ToList();
        }

        public void UpdateTexture(Texture texture)
        {
            GD.Print("Update Texture");
            Stage.GetNode<Sprite>("Sprite").Texture = texture;
        }

    }
}