using Godot;
using System;
using System.Linq;

namespace Hopper
{
    public class LevelFactory
    {
        const string defaultLevelSaveFolder = "res://Levels/";
        const string defaultLevelSaveSuffix = "_Data.tres";
        const int defaultWidth = 7;
        const int defaultHeight = 7;
        const int defaultTileSize = 32;
        const int defaultStartingHops = 6;
        const int defaultMaximumHops = 10;
        const int defaultScoreRequired = 100;
        const int defaultScore = 100;
        const int defaultX = 0;
        const int defaultY = 0;
        Vector2 defaultPlayerStartPosition;
        
        const int defaultScoreTileCount = 3;
        const int defaultGoalCount = 1;
        private RandomNumberGenerator rand = new RandomNumberGenerator();
        ResourceRepository Resources;

        public LevelFactory(ResourceRepository resources)
        {
            Resources = resources;
            defaultPlayerStartPosition = new Vector2(defaultX, defaultY);
            rand.Randomize();
        }

        public Level New(int width = defaultWidth, 
                         int height = defaultHeight, 
                         int tileSize = defaultTileSize)
        {
            if (height > 7 || width > 7) return null;
            
            LevelData levelData = NewBlankLevelData(width, height, tileSize);
            return GetLevelScene(levelData);
        }

        private Level GetLevelScene(LevelData levelData)
        {
            Level level = (Level)GD.Load<PackedScene>("res://Levels/Template/Level.tscn").Instance();
            if (level == null) return null;
            level.LevelData = levelData;
            return level;
        }

        public LevelData NewBlankLevelData(int width = defaultWidth, 
                                           int height = defaultHeight, 
                                           int tileSize = defaultTileSize)
        {
            LevelData levelData = ResourceLoader.Load<LevelData>("res://Levels/Template/LevelData.tres");
            if (levelData == null) return null; //TODO: maybe also check if all types are valid here

            levelData.Width = width;
            levelData.Height = height;
            levelData.TileSize = tileSize;
            levelData.StartingHops = defaultStartingHops;
            levelData.MaximumHops = defaultMaximumHops;
            levelData.ScoreRequired = defaultScoreRequired;
            levelData.PlayerStartPosition = defaultPlayerStartPosition;
            levelData.Init(width * height);
            for (int i = 0; i < levelData.TileType.Length - 1; i++)
            {
                levelData.TileType[i] = Type.Lily;
                levelData.TilePointValue[i] = 0;
            }
            return levelData;
        }

/*         public Level Load(string path)
        {
            LevelData levelData = ResourceLoader.Load<LevelData>(path);
            if (levelData == null) return null;
            return GetLevelScene(levelData);
        } */

        public Level Load(string name, bool buildPath = false) //the 'name' parameter can hold either just the name or the full path
        {
            string path;
            if (buildPath == true)
            {
                path = defaultLevelSaveFolder + name + defaultLevelSaveSuffix; 
            }
            else
            {
                path = name;
            }
            LevelData levelData = ResourceLoader.Load<LevelData>(path);
            if (levelData == null) return null;
            return GetLevelScene(levelData);
        }

        public Error Save(Level level)
        {
            if (level.LevelName == null) level.LevelName = "DefaultLevelName";
            level.UpdateLevelData();
            return ResourceSaver.Save($"res://Levels/{level.LevelName}_Data.tres", level.LevelData);
        }
        
        public Level Generate(int width = defaultWidth, 
                              int height = defaultHeight, 
                              int tileSize = defaultTileSize, 
                              int startingHops = defaultStartingHops,
                              int maximumHops = defaultMaximumHops,
                              int scoreRequired = defaultScoreRequired, 
                              int scoreTileCount = defaultScoreTileCount, 
                              int goalCount = defaultGoalCount,
                              int playerPositionX = defaultX,
                              int playerPositionY = defaultY)
        {
            LevelData levelData = NewBlankLevelData(width, height, tileSize);
            levelData.StartingHops = startingHops;
            levelData.MaximumHops = maximumHops;
            levelData.ScoreRequired = scoreRequired;
            levelData.PlayerStartPosition = new Vector2(playerPositionX, playerPositionY);

            Tile goalTile = CalculateGoalTilePosition(rand, 
                                                      levelData.PlayerStartPosition,
                                                      width,
                                                      height,
                                                      startingHops, 
                                                      2);
            levelData.UpdateTile(goalTile);
            goalTile.QueueFree();           

            Tile jumpTile = CalculateJumpTilePositions(rand, 
                                                       levelData.PlayerStartPosition,
                                                       width,
                                                       height,
                                                       startingHops,
                                                       1,
                                                       goalTile);
            levelData.UpdateTile(jumpTile);
            jumpTile.QueueFree();

            Tile[] scoreTiles = CalculateScoreTilePositions(rand, 
                                                            levelData.PlayerStartPosition,
                                                            width,
                                                            height,
                                                            maximumHops,
                                                            goalTile,
                                                            jumpTile,
                                                            4);
            foreach (Tile t in scoreTiles)
            {
                levelData.UpdateTile(t);
                t.QueueFree();
            }

            return GetLevelScene(levelData);
        }

        private Tile CalculateGoalTilePosition(RandomNumberGenerator rand, 
                                               Vector2 playerPosition, 
                                               int width,
                                               int height,
                                               int maxStepsFromPlayer, 
                                               int minStepsFromPlayer = 1)
        {
            int TopMax = (int)Math.Max(playerPosition.x, playerPosition.y);
            int BottomMax = (int)Math.Max(width - 1 - playerPosition.x, height - 1 - playerPosition.y);
            int OverallMax = Math.Max(TopMax, BottomMax);
            int limitedMaxStepsFromPlayer = maxStepsFromPlayer;
            if (OverallMax < limitedMaxStepsFromPlayer) limitedMaxStepsFromPlayer = OverallMax;

            int possibleSteps = rand.RandiRange(minStepsFromPlayer, limitedMaxStepsFromPlayer);
            int x = rand.RandiRange(0, possibleSteps);
            Vector2 relativeGridPosition = new Vector2(x, possibleSteps - x);
            Vector2 absoluteGridPosition;

            int limitCounter = 0;
            do
            {
                int limit = rand.RandiRange(0, 3);
                for (int i = 0; i <= limit; i++)
                {
                    float tempY = relativeGridPosition.y;
                    relativeGridPosition.y = relativeGridPosition.x * - 1;
                    relativeGridPosition.x = tempY;
                }
                absoluteGridPosition = relativeGridPosition + playerPosition;
                limitCounter++;

            } while ((absoluteGridPosition.x < 0 ||
                      absoluteGridPosition.x >= width ||
                      absoluteGridPosition.y < 0 || 
                      absoluteGridPosition.y >= height)
                      && (limitCounter < 100));

            //GD.Print($"Limit: {limitCounter}"); //TODO: ERROR CHECK HERE

            var tile = Resources.GoalOffScene.Instance() as Tile;
            tile.GridPosition = absoluteGridPosition;
            return tile;
            //return new Tile(Type.Goal, absoluteGridPosition);
        }

        private Tile CalculateJumpTilePositions(RandomNumberGenerator rand,
                                                Vector2 playerPosition, 
                                                int width,
                                                int height, 
                                                int maxStepsFromPlayer, 
                                                int minStepsFromPlayer,
                                                Tile goalTile)
        {
            if (rand.RandiRange(2, 2) == 2)
            {
                int TopMax = (int)Math.Max(playerPosition.x, playerPosition.y);
                int BottomMax = (int)Math.Max(width - 1 - playerPosition.x, height - 1 - playerPosition.y);
                int OverallMax = Math.Max(TopMax, BottomMax);
                int limitedMaxStepsFromPlayer = maxStepsFromPlayer;
                if (OverallMax < limitedMaxStepsFromPlayer) limitedMaxStepsFromPlayer = OverallMax;

                Vector2 absoluteGridPosition;

                int limitCounter = 0;
                do
                {
                    int possibleSteps = rand.RandiRange(minStepsFromPlayer, limitedMaxStepsFromPlayer);
                    int x = rand.RandiRange(0, possibleSteps);
                    Vector2 relativeGridPosition = new Vector2(x, possibleSteps - x);

                    int limit = rand.RandiRange(0, 3);
                    for (int i = 0; i <= limit; i++)
                    {
                        float tempY = relativeGridPosition.y;
                        relativeGridPosition.y = relativeGridPosition.x * - 1;
                        relativeGridPosition.x = tempY;
                    }
                    absoluteGridPosition = relativeGridPosition + playerPosition;
                    limitCounter++;

                } 
                while ((absoluteGridPosition.x < 0 || 
                        absoluteGridPosition.x >= width ||
                        absoluteGridPosition.y < 0 || 
                        absoluteGridPosition.y >= height ||
                        goalTile.GridPosition == absoluteGridPosition)
                        && (limitCounter < 100));

                GD.Print($"Limit: {limitCounter}"); //TODO: ERROR CHECK HERE

                Tile tile = Resources.JumpScene.Instance() as Tile;
                tile.GridPosition = absoluteGridPosition;
                tile.JumpLength = 2;
                return tile;
                //return new Tile(Type.Jump, absoluteGridPosition, 2);
            } 
            return null;
        }

        private Tile[] CalculateScoreTilePositions(RandomNumberGenerator rand,
                                                  Vector2 playerPosition,
                                                  int width,
                                                  int height,
                                                  int maximumHops,
                                                  Tile goalTile,
                                                  Tile jumpTile, 
                                                  int Count)
        {
            Tile[] tiles = new Tile[Count];
            Vector2 ScoreToGoal;
            Vector2 ScoreGridPosition;
            for (int i = 0; i < Count; i++)
            {
                float totalSteps;
                Vector2 PlayerToScore;
                Tile scoreTile = null;
                do
                {
                    if (scoreTile != null) scoreTile.QueueFree();
                    int possibleSteps = rand.RandiRange(-maximumHops, maximumHops);
                    int x = rand.RandiRange(-possibleSteps, possibleSteps);
                    PlayerToScore = new Vector2(x, possibleSteps - x);
                    ScoreGridPosition = PlayerToScore + playerPosition;
                    ScoreToGoal = goalTile.GridPosition - ScoreGridPosition;;
                    totalSteps = PlayerToScore.PathLength() + ScoreToGoal.PathLength();
                } 
                while (ScoreGridPosition == playerPosition ||
                       ScoreGridPosition == goalTile.GridPosition ||
                       ScoreGridPosition == jumpTile.GridPosition ||
                       totalSteps >= maximumHops || 
                       totalSteps <= 0 ||
                       ScoreGridPosition.x < 0 || 
                       ScoreGridPosition.x >= width ||
                       ScoreGridPosition.y < 0 || 
                       ScoreGridPosition.y >= height);
                
                foreach (Tile t in tiles)
                {
                    if (t != null)
                    {
                        if (t.GridPosition == ScoreGridPosition)
                        {
                            i--;
                            break;
                        }
                    }
                    else
                    {
                        scoreTile = Resources.ScoreScene.Instance() as Tile;
                        scoreTile.GridPosition = ScoreGridPosition;
                        scoreTile.PointValue = defaultScore * (int)totalSteps;
                        tiles[i] = scoreTile;
                        break;
                    }
                }
            }
            return tiles;
        }
    }
}