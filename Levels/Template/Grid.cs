using System;
using System.Collections.Generic;
using Godot;

namespace Hopper
{
    public class Grid : Control
    {
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

        public ResourceRepository Resources { get; set; }
        
        //private Vector2 ViewportSize;
        public Vector2 Offset;
        public Vector2 Size;
        public Vector2 TileSize;

        public Tile[,] Tiles;

        public Tile GoalTile
        {
            get
            {
                foreach (Tile t in Tiles)
                {
                    if (t.Type == Type.Goal) return t;
                }
                return null;
            }
        }
        //public Tile PlayerTile;

        [Signal]
        public delegate void NextLevel();

        public Grid()
        {
            Name = "Grid";
        }

        public Tile GetTile(Vector2 position)
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
                40 + ((7 - GridHeight) * TileSize.y)/2
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
                    if (levelData != null)
                    {
                        Tiles[x, y] = Resources.LoadByType(levelData.TileType[i]).Instance() as Tile;
                    }
                    else
                    {
                        Tiles[x, y] = Resources.LilyScene.Instance() as Tile;
                    }
                    Tiles[x, y].PointValue = levelData.TilePointValue[i];
                    Tiles[x, y].GridPosition = new Vector2(x, y);
                    Tiles[x, y].Name = $"Tile{x}-{y}";
                    //Tiles[x, y].Connect(nameof(Tile.TileUpdated), this, "UpdateTile");
                    //Tiles[x, y].Connect(nameof(Tile.PlayerStartUpdated), this, "UpdatePlayerStart");
                    AddChild(Tiles[x, y]);
                    Tiles[x, y].Owner = this;
                    i++;
                }
            }
        }

        internal Vector2 DetermineWaterExit(Vector2 movement)
        {
            throw new NotImplementedException();
        }

        public void ConnectTile(Tile tile)
        {
            tile.Connect(nameof(Tile.TileUpdated), this, nameof(UpdateTile));
        }

        public Vector2 LimitToBounds(Vector2 Position)
        {
            return new Vector2(
                Mathf.Clamp(Position.x, 0, GridWidth - 1),
                Mathf.Clamp(Position.y, 0, GridHeight - 1)
            );
        }

        public Vector2 DetermineWaterExit(Tile newTile, Vector2 movement)
        {
            Vector2 CheckPosition = newTile.GridPosition + movement;
            while (WithinGrid(CheckPosition))
            {
                if (GetTile(CheckPosition).Type == Type.Rock) break;
                if (GetTile(CheckPosition).Type == Type.Water)
                {
                    if (ViableLandingPoint(CheckPosition + movement)) return CheckPosition + movement;
                }
                CheckPosition += movement;
            }
            CheckPosition = newTile.GridPosition;
            if (ViableLandingPoint(CheckPosition + movement))
            {
                return newTile.GridPosition + movement;
            } 
            else
            {
                return newTile.GridPosition - movement;
            }
        }

        private bool ViableLandingPoint(Vector2 position)
        {
            if (!WithinGrid(position)) return false;
            if (GetTile(position).Type == Type.Water) return false;
            if (GetTile(position).Type == Type.Rock) return false;
            return true;
        }

        public bool WithinGrid(Vector2 position)
        {
            if (position.x < GridWidth && 
                position.y < GridHeight &&
                position.x >= 0 &&
                position.y >= 0)
            {
                return true;
            }
            return false;
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
                        //GD.Print(currentRowstring + " ]");
                        currentRowstring = "[ ";
                    }

                    currentRowstring += $"{Tiles[x, y].GridPosition} {Tiles[x, y].Type} {Tiles[x, y].PointValue}";
                }
            }
            //GD.Print(currentRowstring + " ]");
        }

        public void OnMouseExit()
        {
            if(Editable) Input.SetDefaultCursorShape(Input.CursorShape.Arrow);
        }

        //For updating a tile in the editor (called by signal in Tile)
        internal void UpdateTile(Vector2 gridPosition, Type type, int score)
        {
            Tile newTile = Resources.LoadByType(type).Instance() as Tile;
            if (newTile.Type == Type.Score) newTile.PointValue = score;
            newTile.Name = $"Tile{gridPosition.x}-{gridPosition.y}";
            //newTile.Connect(nameof(Tile.TileUpdated), this, "UpdateTile");
            ConnectTile(newTile);
            newTile.Editable = true;
            ReplaceTile(gridPosition, newTile);
        }

        internal void ReplaceTile(Vector2 gridPosition, Tile newTile)
        {
            newTile.GridPosition = gridPosition;
            newTile.Name = $"Tile{gridPosition.x}-{gridPosition.y}";
            Tiles[(int)gridPosition.x, (int)gridPosition.y].QueueFree();
            Tiles[(int)gridPosition.x, (int)gridPosition.y] = newTile;
            AddChild(newTile);
        }
    }
}