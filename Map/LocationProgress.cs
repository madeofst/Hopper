using Godot;
using System.Collections.Generic;
using System.Linq;

namespace Hopper
{
    public class LocationProgress : Node2D
    {
        //Radius of circle for levelsprites
        const int Radius = 21;
        //Offset for each individual sprite
        //private Vector2 Offset = new Vector2(-4, -4);

        private List<Sprite> LevelSprites;

        public void Init(int LevelReached)
        {
            LevelSprites = GetChildren().OfType<Sprite>().ToList();
            for(int i = 0; i < LevelSprites.Count; i++)
            {
                float spacing = -Mathf.Tau / 2.7f / 5 * i;
                float angularOffset = LevelSprites.Count / 2 + 0.15f;
                float rotationAngle = spacing + angularOffset;
                float ellipseWarpFactor = 1 - Mathf.Pow(Mathf.Sin(rotationAngle), 2f) * 0.15f;
                Vector2 vec = new Vector2(Radius * ellipseWarpFactor, 0).Rotated(rotationAngle);
                LevelSprites[i].Position = vec; //+ Offset;
            }
            Update(LevelReached);
        }

        public void Update(int LevelReached)
        {
            for(int i = 0; i < LevelReached; i++)
            {
                LevelSprites[i].Frame = 1;
            }
        }

    }
}