using System;
using System.Collections.Generic;
using Godot;

namespace Hopper
{
    public class Grid : Control
    {
        public ulong Time;
        public void PrintTime(string desc = "")
        {
            GD.Print($"{desc} - {Time}");
        }

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
        [Signal]
        public delegate void ConnectTileRightClick();

        public Grid()
        {
            Name = "Grid";
        }

        public Tile GetTile(Vector2 position)
        {
            if (WithinGrid(position))
                return Tiles[(int)position.x, (int)position.y];
            else
                throw new NotImplementedException();
        }

        public override void _Ready()
        {
            Connect("mouse_exited", this, "OnMouseExit");
        }

        public virtual void DefineGrid(int tileSize, int gridWidth, int gridHeight)
        {
            GridWidth = gridWidth + 2;
            GridHeight = gridHeight + 2;

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
            for (int y = 1; y < GridHeight - 1; y++)
            {
                for (int x = 1; x < GridWidth - 1; x++)
                {
                    
                    //Time = OS.GetTicksMsec();

                    Tile tempTile;
                    if (levelData != null)
                    {
                        tempTile = Resources.LoadByType(levelData.TileType[i]).Instance() as Tile;
                    }
                    else
                    {
                        tempTile = Resources.LilyScene.Instance() as Tile;
                    }

/*                     Time = OS.GetTicksMsec() - Time;
                    PrintTime($"Time to instance tile type {tempTile.Type}");
                    Time = OS.GetTicksMsec(); */

                    Tiles[x, y] = tempTile;
                    Tiles[x, y].PointValue = levelData.TilePointValue[i];
                    if (levelData.TileBounceDirection == null)
                    {
                        //TODO:build bouncedirection array
                        levelData.BuildBounceDirectionArray();
                    }
                    Tiles[x, y].BounceDirection = levelData.TileBounceDirection[i];
                    Tiles[x, y].GridPosition = new Vector2(x, y);
                    Tiles[x, y].Name = $"Tile{x}-{y}";
                    AddChild(Tiles[x, y]);
                    Tiles[x, y].Owner = this;
                    i++;

/*                     Time = OS.GetTicksMsec() - Time;
                    PrintTime($"Time to add to tree tile type {tempTile.Type}"); */
                }
            }
            FillEdges();
        }

        private void FillEdges()
        {
            for (int y = 0; y < GridHeight; y++)
            {
                for (int x = 0; x < GridWidth; x++)
                {
                    if (Tiles[x, y] == null)
                    {
                        Tile tempTile = Resources.RockScene.Instance() as Tile;
                        Tiles[x, y] = tempTile;
                        Tiles[x, y].GridPosition = new Vector2(x, y);
                        Tiles[x, y].Name = $"Tile{x}-{y}";
                        Tiles[x, y].Visible = false;
                        AddChild(Tiles[x, y]);
                        Tiles[x, y].Owner = this;
                    } 
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
            EmitSignal(nameof(ConnectTileRightClick), tile);
        }

        public Vector2 LimitToBounds(Vector2 Position)
        {
            return new Vector2(
                Mathf.Clamp(Position.x, 0, GridWidth - 1),
                Mathf.Clamp(Position.y, 0, GridHeight - 1)
            );
        }

        public bool ViableLandingPoint(Vector2 position)
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
        internal void UpdateTile(Vector2 gridPosition, Type type, int score, Vector2 BounceDirection)
        {
            Tile newTile = Resources.LoadByType(type).Instance() as Tile;
            if (newTile.Type == Type.Score) newTile.PointValue = score;
            if (newTile.Type == Type.Direct) newTile.BounceDirection = BounceDirection;
            newTile.Name = $"Tile{gridPosition.x}-{gridPosition.y}";
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

        public bool HasOneGoal()
        {
            int goalCount = 0;
            foreach (Tile tile in Tiles)
            {
                if (tile.Type == Type.Goal) goalCount += 1;
            }
            if (goalCount == 1) return true;
            return false;
        }
    }
}