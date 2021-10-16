using System;
using System.Collections.Generic;
using Godot;

namespace Hopper
{
    public class Grid : Control
    {
        //public Player Player { get; set; }

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

        public void IncrementGoalCount()
        {
            EmitSignal(nameof(NextLevel));
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