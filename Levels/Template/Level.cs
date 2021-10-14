using System;
using Godot;

namespace Hopper
{
    public class Level : Node2D
    {
        public LevelData LevelData { get; set; }
        public Grid Grid { get; set; }

        private bool editable = false;
        public bool Editable 
        { 
            get => editable; 
            set
            {
                editable = value;
                Grid.Editable = value;
            } 
        }

        //New params which relate to save file
        public int StartingHops { get; set; }
        public int MaximumHops { get; set; }
        public int ScoreRequired { get; set; }
        public Vector2 PlayerStartPosition { get; set; }
        public string LevelName { get; set; }

        public Level(){}

        public override void _Ready()
        {
            Name = "Level";
            Grid = GetNode<Grid>("Grid");
        }
        
        //Builds grid using levelData.TileType info (if provided) and using plain lily tiles if not
/*         public void BuildGrid(int size, int tileSize, LevelData levelData = null)
        {
            Grid.DefineGrid(tileSize, levelData.Width, levelData.Height);
            Grid.ClearExistingChildren();
            Grid.PopulateGrid(levelData);
        } */

        public void Build()
        {
            StartingHops = LevelData.StartingHops;
            MaximumHops = LevelData.MaximumHops;
            ScoreRequired = LevelData.ScoreRequired;
            PlayerStartPosition = LevelData.PlayerStartPosition;
            Grid.DefineGrid(LevelData.TileSize, LevelData.Width, LevelData.Height);
            Grid.ClearExistingChildren();
            Grid.PopulateGrid(LevelData);
        }


        public void UpdateLevelData()
        {
            //levelData.Init(Grid.GridWidth*Grid.GridHeight);  //FIXME: levelData should alreacy by initialised
            int i = 0;
            for (int y = 0; y < Grid.GridHeight; y++)
            {
                for (int x = 0; x < Grid.GridWidth; x++)
                {
                    LevelData.TileType[i] = Grid.Tiles[x, y].Type;
                    LevelData.TilePointValue[i] = Grid.Tiles[x, y].PointValue;
                    i++;
                }
            }
            LevelData.StartingHops = StartingHops;
            LevelData.MaximumHops = MaximumHops;
            LevelData.ScoreRequired = ScoreRequired;
            LevelData.PlayerStartPosition = PlayerStartPosition;
        }

/*         public Error SaveToFile(string levelName)
        {
            LevelData levelData = ResourceLoader.Load<LevelData>("res://Levels/Template/LevelData.tres");
            levelData.Init(Grid.GridWidth*Grid.GridHeight);   
            int i = 0;
            for (int y = 0; y < Grid.GridHeight; y++)
            {
                for (int x = 0; x < Grid.GridWidth; x++)
                {
                    levelData.TileType[i] = Grid.Tiles[x, y].Type;
                    levelData.TilePointValue[i] = Grid.Tiles[x, y].PointValue;
                    i++;
                }
            }
            levelData.MaximumHops = MaximumHops;
            levelData.ScoreRequired = ScoreRequired;

            return ResourceSaver.Save($"res://Levels/{levelName}_Data.tres", levelData);
        } */

        //Old and auto params
        public int ID;
        public int GridSize;
        public int GoalsToNextLevel;
        public int ScoreTileCount;

        public int MaxHops;
        public int HopsToAdd;
    
        public Level(int id, int gridSize, int maxHops, int scoreTileCount, int goalsToNextLevel = 10, int hopsToAdd = 0) //TODO: turn this into a Generate() method?
        {
            ID = id;
            GridSize = gridSize;
            MaxHops = maxHops;
            GoalsToNextLevel = goalsToNextLevel;
            if (hopsToAdd == 0)
            {
                HopsToAdd = (int)Math.Floor((decimal)MaxHops/2);
            }
            else
            {
                HopsToAdd = hopsToAdd;
            }
            ScoreTileCount = scoreTileCount;
        }
    }
}