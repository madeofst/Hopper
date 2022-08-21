using Godot;
using System.Collections.Generic;
using System.Linq;

namespace Hopper
{
    public class LocationProgress : Node2D
    {
        //Radius of circle for levelsprites
        const int Radius = 21;

        private List<Sprite> LevelSprites;

        public void Init(int LevelReached)
        {
            LevelSprites = GetChildren().OfType<Sprite>().ToList();
            for(int i = 0; i < LevelSprites.Count; i++)
            {
                float spacing = -Mathf.Tau / 2.7f / 5;
                float angularOffset = -Mathf.Tau - (LevelSprites.Count * spacing / 2) + 1.34f;
                float rotationAngle = angularOffset + spacing * i;
                float ellipseWarpFactor = 1 - Mathf.Pow(Mathf.Sin(rotationAngle), 2f) * 0.2f;
                Vector2 vec = new Vector2(Radius * ellipseWarpFactor, 0).Rotated(rotationAngle);
                LevelSprites[i].Position = vec;
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