using Godot;
using System;

namespace Hopper
{
    public class LevelFactory
    {
        //Fixed parameters
        const int defaultWidth = 7;
        const int defaultHeight = 7;
        const int defaultTileSize = 32;
        const int defaultStartingHops = 3;
        const int defaultMaximumHops = 10;
        const int defaultScoreRequired = 0;
        const int defaultX = 0;
        const int defaultY = 0;
        Vector2 defaultPlayerStartPosition;
        
        //Parameters for generating levels
        const int defaultScoreTileCount = 3;
        const int defaultGoalCount = 1;

        public LevelFactory()
        {
            defaultPlayerStartPosition = new Vector2(defaultX, defaultY);
        }

        public Level New(int width = defaultWidth, 
                         int height = defaultHeight, 
                         int tileSize = defaultTileSize)
        {
            if (height > 7 || width > 7) return null;
            
            LevelData levelData = LoadBlankLevelData(width, height, tileSize);
            if (levelData == null) return null; //TODO: maybe also check if all types are valid here
            levelData.StartingHops = defaultStartingHops;
            levelData.MaximumHops = defaultMaximumHops;
            levelData.ScoreRequired = defaultScoreRequired;
            levelData.PlayerStartPosition = defaultPlayerStartPosition;

            Level level = (Level)GD.Load<PackedScene>("res://Levels/Template/Level.tscn").Instance();
            if (level == null) return null;
            level.LevelData = levelData;

            return level;
        }

        public Level Generate(int width = defaultWidth, 
                              int height = defaultHeight, 
                              int tileSize = defaultTileSize, 
                              int startingHops = defaultStartingHops,
                              int scoreRequired = defaultScoreRequired, 
                              int scoreTileCount = defaultScoreTileCount, 
                              int goalCount = defaultGoalCount)
        {
            return null;
        }

        public Level Load(string path)
        {
            LevelData levelData = ResourceLoader.Load<LevelData>(path);
            if (levelData == null) return null;
            Level level = (Level)GD.Load<PackedScene>("res://Levels/Template/Level.tscn").Instance();
            level.LevelData = levelData;
            return level;
        }

        public Error Save(Level level)
        {
            if (level.Name == "") level.Name = "DefaultLevelName";
            LevelData levelData = ResourceLoader.Load<LevelData>("res://Levels/Template/LevelData.tres");
            level.UpdateLevelData();
            levelData = level.LevelData;
            return ResourceSaver.Save($"res://Levels/{level.Name}_Data.tres", levelData);
        }

        private LevelData LoadBlankLevelData(int width, int height, int tileSize) //TODO: make this load any level
        {
            LevelData levelData = ResourceLoader.Load<LevelData>("res://Levels/Template/LevelData.tres");
            
            levelData.Width = width;
            levelData.Height = height;
            levelData.TileSize = tileSize;
            levelData.Init(width * height);

            for (int i = 0; i < levelData.TileType.Length - 1; i++)
            {
                levelData.TileType[i] = Type.Blank;
                levelData.TilePointValue[i] = 0;
            }

            return levelData;
        }
    }
}