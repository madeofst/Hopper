using Godot;

namespace Hopper
{
    public class LevelData : Resource
    {
        [Export]
        public int Width { get; set; }

        [Export]
        public int Height { get; set; }

        [Export]
        public int TileSize { get; set; }

        [Export]
        public Type[] TileType;

        [Export]
        public int[] TilePointValue;

        [Export]
        public int StartingHops { get; set; }

        [Export]
        public int MaximumHops { get; set; }

        [Export]
        public int ScoreRequired { get; set; }

        [Export]
        public Vector2 PlayerStartPosition { get; set; }

        public LevelData(){}

        public void Init(int length)
        {
            TileType = new Type[length];
            TilePointValue = new int[length];
        }

        public void UpdateTile(Vector2 tilePosition, Type tileType, int tilePointValue)
        {
            int i = (int)((tilePosition.y * Width) + tilePosition.x);
            TileType[i] = tileType;
            TilePointValue[i] = tilePointValue;
        }
    }
}