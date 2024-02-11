using Godot;
using System;
using System.Collections.Generic;

namespace Hopper
{
    public class UndoManager : Node2D
    {
        private Stack<UndoItem> UndoStack;

        private Player Player;
        private Stage Stage;
        private Grid Grid;

        public void Init(Player player, Level currentLevel)
        {
            UndoStack = new Stack<UndoItem>();
            
            Player = player;

            if (!Player.IsConnected(nameof(Player.SaveUndoState), this, nameof(AddUndoItem))) 
                Player.Connect(nameof(Player.SaveUndoState), this, nameof(AddUndoItem));

            if (!Player.IsConnected(nameof(Player.InitUndo), this, nameof(Undo))) 
                Player.Connect(nameof(Player.InitUndo), this, nameof(Undo));

            Stage = GetNode<Stage>("..");
            Grid = currentLevel.Grid;
        }


        // Save undo state

        public void AddUndoItem()
        {
            UndoItem undoItem = new UndoItem();
            undoItem.PlayerState = ExtractPlayerState();
            undoItem.GridState = ExtractGridState();
            UndoStack.Push(undoItem);
        }

        private PlayerState ExtractPlayerState()
        {
            PlayerState playerState = new PlayerState(Player.GridPosition, Player.FacingDirection, Player.LevelScore); 
            return playerState;
        }

        private GridState ExtractGridState()
        {
            return new GridState(Grid.GridWidth, Grid.GridHeight, Grid.Tiles);
        }

        
        // Perform undo action

        private void Undo()
        {
            if (Player.Active)
            {
                Player.Deactivate();

                if (UndoStack.Count > 0)
                {  
                    Player.HaltAnimation();
                    UndoItem undoItem = UndoStack.Pop();
                    UpdatePlayer(undoItem.PlayerState);
                    UpdateGrid(undoItem.GridState);
                }

                if (UndoStack.Count == 0) Player.FacingDirection = Vector2.Down;
                Player.Activate();
            }
        }

        private void UpdatePlayer(PlayerState playerState)
        {
            Player.UpdateHopsRemaining(1);
            Player.GridPosition = playerState.GridPosition;
            Player.ResetAnimation(playerState.FacingDirection);
            Player.LevelScore = playerState.Score;
            Player.FacingDirection = playerState.FacingDirection;
        }

        private void UpdateGrid(GridState gridState)
        {
            for (int y = 1; y < Grid.GridHeight - 1; y++)
            {
                for (int x = 1; x < Grid.GridWidth - 1; x++)
                {
                    Grid.UpdateTile(gridState.TileChangeInstructions[x, y], false);
                }
            }
            Stage.UpdateBossIndicators(Stage.CurrentLevel);
            Stage.MovePlayerToTop();
        }
    }
}