using Godot;
using System;
using System.Linq;

namespace Hopper
{
    public class LevelFactory
    {
        const int defaultWidth = 7;
        const int defaultHeight = 7;
        const int defaultTileSize = 32;
        const int defaultStartingHops = 3;
        const int defaultMaximumHops = 10;
        const int defaultScoreRequired = 0;
        const int defaultScore = 100;
        const int defaultX = 0;
        const int defaultY = 0;
        Vector2 defaultPlayerStartPosition;
        
        const int defaultScoreTileCount = 3;
        const int defaultGoalCount = 1;
        private RandomNumberGenerator rand = new RandomNumberGenerator();

        public LevelFactory()
        {
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
                levelData.TileType[i] = Type.Blank;
                levelData.TilePointValue[i] = 0;
            }
            return levelData;
        }

        public Level Load(string path)
        {
            LevelData levelData = ResourceLoader.Load<LevelData>(path);
            if (levelData == null) return null;
            return GetLevelScene(levelData);
        }

        public Error Save(Level level)
        {
            if (level.Name == "") level.Name = "DefaultLevelName";
            LevelData levelData = ResourceLoader.Load<LevelData>("res://Levels/Template/LevelData.tres");
            level.UpdateLevelData();
            levelData = level.LevelData;
            return ResourceSaver.Save($"res://Levels/{level.Name}_Data.tres", levelData);
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
            levelData.MaximumHops = defaultMaximumHops;
            levelData.ScoreRequired = scoreRequired;

            Tile goalTile = CalculateGoalTilePosition(rand, 
                                                      new Vector2(playerPositionX, playerPositionY),
                                                      width,
                                                      height,
                                                      startingHops, 
                                                      2);
            levelData.UpdateTile(goalTile);

            Tile[] tiles = CalculateScoreTilePosition(rand, 
                                                      new Vector2(playerPositionX, playerPositionY),
                                                      width,
                                                      height,
                                                      maximumHops,
                                                      goalTile,
                                                      4);

            foreach (Tile t in tiles)
            {
                levelData.UpdateTile(t);
            }
             //Vector2[] scoreTilePositions 
            //AssignScoreTiles(rand, scoreTileCount);
            //AssignJumpTile(rand, startingHops, 2);
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

            } while ((absoluteGridPosition.x < 0 || absoluteGridPosition.x >= width ||
                    absoluteGridPosition.y < 0 || absoluteGridPosition.y >= height)
                    &&
                    (limitCounter < 100));

            //GD.Print($"Limit: {limitCounter}"); //TODO: ERROR CHECK HERE

            return new Tile(Type.Goal, absoluteGridPosition);
        }

        private Tile[] CalculateScoreTilePosition(RandomNumberGenerator rand,
                                                  Vector2 playerPosition,
                                                  int width,
                                                  int height,
                                                  int maximumHops,
                                                  Tile goalTile, 
                                                  int Count)
        {
            Tile[] tiles = new Tile[Count];
            Vector2 ScoreToGoal;
            Vector2 ScoreGridPosition;
            for (int i = 0; i < Count; i++)
            {
                float totalSteps;
                Vector2 PlayerToScore;
                Tile scoreTile;
                do
                {
                    int possibleSteps = rand.RandiRange(-maximumHops, maximumHops);
                    int x = rand.RandiRange(-possibleSteps, possibleSteps);
                    PlayerToScore = new Vector2(x, possibleSteps - x);
                    ScoreGridPosition = PlayerToScore + playerPosition;
                    ScoreToGoal = goalTile.GridPosition - ScoreGridPosition;;
                    totalSteps = PlayerToScore.PathLength() + ScoreToGoal.PathLength();
                    scoreTile = new Tile(Type.Score, ScoreGridPosition,(defaultScore * (int)totalSteps));
                } 
                while (ScoreGridPosition == playerPosition ||
                       ScoreGridPosition == goalTile.GridPosition ||
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
                        if (t.GridPosition == scoreTile.GridPosition)
                        {
                            i--;
                            break;
                        }
                    }
                    else
                    {
                        tiles[i] = scoreTile;
                        break;
                    }
                } 
            }
            return tiles;
        }

        private void AssignJumpTile(RandomNumberGenerator rand, int maxStepsFromPlayer, int minStepsFromPlayer = 1)
        {
   /*          if (rand.RandiRange(1, 3) == 3)
            {
            int TopMax = (int)Math.Max(Player.GridPosition.x, Player.GridPosition.y);
            int BottomMax = (int)Math.Max(GridWidth - 1 - Player.GridPosition.x, GridHeight - 1 - Player.GridPosition.y);
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
                absoluteGridPosition = relativeGridPosition + Player.GridPosition;
                limitCounter++;

            } while ((absoluteGridPosition.x < 0 || 
                    absoluteGridPosition.x >= GridWidth ||
                    absoluteGridPosition.y < 0 || 
                    absoluteGridPosition.y >= GridHeight ||
                    Tile(absoluteGridPosition).Type != Type.Blank)
                    && (limitCounter < 100)
                    );
            if (limitCounter < 100) Tile(absoluteGridPosition).Type = Type.Jump;
            } */
            //GD.Print($"Limit: {limitCounter}"); //TODO: ERROR CHECK HERE
        }




    }
}