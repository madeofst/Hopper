using Godot;
using System.Collections.Generic;
using System.Linq;

namespace Hopper
{
    public class LocationProgress : Node2D
    {
        //Radius of circle for levelsprites
        const int Radius = 18;
        //Offset for each individual sprite
        private Vector2 Offset = new Vector2(-4, -4);

        private List<Sprite> LevelSprites;

        public void Init(int LevelReached)
        {
            LevelSprites = GetChildren().OfType<Sprite>().ToList();
            for(int i = 1; i <= LevelSprites.Count; i++)
            {
                float spacing = Mathf.Tau / 2.7f / LevelSprites.Count;
                Vector2 vec = new Vector2(Radius, 0).Rotated(spacing * i + Mathf.Tau * 0.02f);
                LevelSprites[i-1].Position = vec + Offset;
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