using System;
using System.Collections.Generic;
using Godot;

namespace Hopper
{
    public class Grid : Control
    {
        public Player Player { get; set; }

        //TODO: add editable property
        private bool editable = false;
        public bool Editable
        { 
            get => editable; 
            set
            {
                editable = value;
                foreach (Tile t in Tiles)
                {
                    t.Editable = value;
                }
            } 
        }

        public int GridWidth;
        public int GridHeight; 
        public int GoalCount; //TODO: should live in level
        
        //private Vector2 ViewportSize;
        public Vector2 Offset;
        public Vector2 Size;
        public Vector2 TileSize;

        public Tile[,] Tiles;
        public RandomNumberGenerator rand = new RandomNumberGenerator();

        public Tile GoalTile;
        public Tile PlayerTile;

/*         private Level currentLevel;
        public Level CurrentLevel 
        {
            get { return currentLevel; }
            set
            {
                currentLevel = value;
                int previousGridWidth = GridWidth;
                DefineGrid(32, GridWidth, GridHeight); //FIXME: check this isn't an error, used to take from CurrentLevel
                if (Player != null)
                {
                    SetupGrid();
                    if (GridWidth != previousGridWidth) Player.GridPosition = new Vector2(0, 0);
                }
            }
        } */

        [Signal]
        public delegate void NextLevel();

        public Grid()
        {
            Name = "Grid";
        }

/*         public Grid(Level level) : this()
        {
            CurrentLevel = level;
        } */

        public Tile Tile(Vector2 position)
        {
            return Tiles[(int)position.x, (int)position.y];
        }

        public override void _Ready()
        {
            Connect("mouse_exited", this, "OnMouseExit");
            //rand.Randomize();
        }

        public virtual void DefineGrid(int tileSize, int gridWidth, int gridHeight)
        {
            GridWidth = gridWidth;
            GridHeight = gridHeight;

            TileSize = new Vector2(tileSize, tileSize);
            RectSize = new Vector2(tileSize * GridWidth, tileSize * GridHeight);
            SetPosition(new Vector2(
                128 + ((7 - GridWidth) * TileSize.x)/2, 
                23 + ((7 - GridHeight) * TileSize.y)/2
            ));

            Tiles = new Tile[GridWidth, GridHeight];
        }

        public void ClearExistingChildren()  //TODO: could be an extension method
        {
            Godot.Collections.Array children = GetChildren();
            foreach (Node2D child in children)
            {
                RemoveChild(child);
                child.QueueFree();
            }
        }

        internal void PopulateGrid(LevelData levelData = null)
        {
            int i = 0;
            for (int y = 0; y < GridHeight; y++)
            {
                for (int x = 0; x < GridWidth; x++)
                {
                Tiles[x, y] = new Tile($"Tile{x}-{y}");
                AddChild(Tiles[x, y]);
                if (levelData != null)
                {
                    Tiles[x, y].BuildTile(levelData.TileType[i], TileSize, new Vector2(x, y), levelData.TilePointValue[i]);
                }
                else
                {
                    Tiles[x, y].BuildTile(Type.Lily, TileSize, new Vector2(x, y));
                }
                Tiles[x, y].Owner = this;
                i++;
                }
            }
        }






        /* public void InitializeGrid()
        {
            //TODO: Can I avoid calling player here?  The Goal stuff will be in the Level anyway I think
            Player = GetNode<Player>("../Player");
            if (Player != null) Player.Connect(nameof(Player.GoalReached), this, "IncrementGoalCount");
            SetupGrid();
        }

        public void SetupGrid()
        {
            ClearExistingChildren();
            PopulateGrid();
            UpdateGrid();
        }




        public void UpdateGrid()
        {
            //AssignTileTypes();
        }

        private void AssignTileTypes()  //TODO: make a level function rather than a grid function
        {
            ClearTypes();
            AssignGoalTile(rand, CurrentLevel.HopsToAdd, 2);
            AssignScoreTiles(rand, CurrentLevel.ScoreTileCount);
            AssignJumpTile(rand, CurrentLevel.HopsToAdd, 2);
        }

        private void ClearTypes()
        {
            foreach (Tile t in Tiles)
            {
                t.Type = Type.Lily;
            }
        }

        private void AssignGoalTile(RandomNumberGenerator rand, int maxStepsFromPlayer, int minStepsFromPlayer = 1)  //TODO: Maybe a separate GridGenerator class for this?
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

            } while ((absoluteGridPosition.x < 0 || absoluteGridPosition.x >= GridWidth ||
                    absoluteGridPosition.y < 0 || absoluteGridPosition.y >= GridHeight)
                    &&
                    (limitCounter < 100));

            GD.Print($"Limit: {limitCounter}"); //TODO: ERROR CHECK HERE

            GoalTile = Tiles[(int)absoluteGridPosition.x, (int)absoluteGridPosition.y];
            GoalTile.Type = Type.Goal;
        }

        private void AssignJumpTile(RandomNumberGenerator rand, int maxStepsFromPlayer, int minStepsFromPlayer = 1)
        {
            if (rand.RandiRange(1, 3) == 3)
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
                    Tile(absoluteGridPosition).Type != Type.Lily)
                    && (limitCounter < 100)
                    );
            if (limitCounter < 100) Tile(absoluteGridPosition).Type = Type.Jump;
            }
            //GD.Print($"Limit: {limitCounter}"); //TODO: ERROR CHECK HERE
        }

        private void AssignScoreTiles(RandomNumberGenerator rand, int Count)
        {
            Vector2 ScoreToGoal;
            Vector2 ScoreGridPosition;
            for (int i = 1; i <= Count; i++)
            {
                float totalSteps;
                Vector2 PlayerToScore;
                do
                {
                    int possibleSteps = rand.RandiRange(-CurrentLevel.MaxHops, CurrentLevel.MaxHops);
                    int x = rand.RandiRange(-possibleSteps, possibleSteps);
                    PlayerToScore = new Vector2(x, possibleSteps - x);
                    ScoreGridPosition = PlayerToScore + Player.GridPosition;
                    ScoreToGoal = GoalTile.GridPosition - ScoreGridPosition;;
                    totalSteps = PlayerToScore.PathLength() + ScoreToGoal.PathLength();
                } 
                while (ScoreGridPosition == Player.GridPosition ||
                    ScoreGridPosition == GoalTile.GridPosition ||
                    totalSteps >= CurrentLevel.MaxHops || 
                    totalSteps <= 0 ||
                    ScoreGridPosition.x < 0 || 
                    ScoreGridPosition.x >= GridWidth ||
                    ScoreGridPosition.y < 0 || 
                    ScoreGridPosition.y >= GridHeight ||
                    Tile(ScoreGridPosition).Type == Type.Score);

                Tile(ScoreGridPosition).Type = Type.Score;
                Tile(ScoreGridPosition).PointValue *= (int)totalSteps;
            }
        } */

        public void IncrementGoalCount()
        {
            EmitSignal(nameof(NextLevel));
/*             GoalCount += 1;
            if (GoalCount % 1 == 0)
            {
                GoalCount = 0;
                EmitSignal(nameof(NextLevel));
            } */
        }

        public void PrintGrid()  //For debugging tile types etc.
        {
            float row = 0;
            string currentRowstring = "[ ";
            for (int y = 0; y < GridWidth; y++)
            {
                for (int x = 0; x < GridHeight; x++)
                {
                    if (Tiles[x, y].GridPosition.y > row)
                    {
                        row += 1;
                        GD.Print(currentRowstring + " ]");
                        currentRowstring = "[ ";
                    }

                    currentRowstring += $"{Tiles[x, y].GridPosition} {Tiles[x, y].Type} {Tiles[x, y].PointValue}";
                }
            }
            GD.Print(currentRowstring + " ]");
        }

        public void OnMouseExit()
        {
            if(Editable) Input.SetDefaultCursorShape(Input.CursorShape.Arrow);
        }
    }
}