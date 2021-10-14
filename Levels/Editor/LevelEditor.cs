using Godot;
using System;

namespace Hopper
{
    public class LevelEditor : Node2D
    {
        private Level CurrentLevel { get; set; }
        private string LevelName { get; set; }

        public override void _Ready()
        {
        }

        private void NewBlankLevel()
        {
            if (CurrentLevel != null) CurrentLevel.QueueFree();
            CurrentLevel = NewLevel(7);
            CurrentLevel.Editable = true;
        }

        private Level NewLevel(int size, int tileSize = 32, string name = null)
        {
            if (size > 7) return null;
            Level level = (Level)GD.Load<PackedScene>("res://Levels/Template/Level.tscn").Instance();
            AddChild(level);
            LevelData levelData = LoadBlankLevelData(size);
            if (levelData == null) return null; //TODO: maybe also check if all types are valid here
            level.BuildGrid(size, tileSize, levelData);
            return level;
        }

        private void NewRandomLevel()
        {
            NewBlankLevel();
            
            //CurrentLevel.Grid.UpdateGrid();
        }

        private LevelData LoadBlankLevelData(int size = 7) //TODO: make this load any level
        {
            LevelData levelData = ResourceLoader.Load<LevelData>("res://Levels/Template/LevelData.tres");
            levelData.Init(size*size);
            for (int i = 0; i < levelData.TileType.Length - 1; i++)
            {
                levelData.TileType[i] = Type.Blank;
                levelData.Score[i] = 0;
            }
            return levelData;
        }

        private void ShowSaveDialog()
        {
            AcceptDialog SaveDialog = GetNode<AcceptDialog>("Dialogs/SaveDialog");
            SaveDialog.PopupCentered();
        }

        private void SaveCurrentLevel()
        {
            if (CurrentLevel != null)
            {
                Error error = CurrentLevel.SaveToFile(LevelName);
                GD.PrintErr(error);
            }
        }

        private void TextChanged(string newText)
        {
            LevelName = newText;
        }

        private void ShowLoadDialog()
        {
            FileDialog LoadDialog = GetNode<FileDialog>("Dialogs/LoadDialog");
            LoadDialog.PopupCentered();
        }

        private void OnFileSelected(string path)
        {
            if (CurrentLevel != null) CurrentLevel.QueueFree();
            CurrentLevel = LoadLevel(path);
        }

        private Level LoadLevel(string path)
        {
            Level level = (Level)GD.Load<PackedScene>("res://Levels/Template/Level.tscn").Instance();
            AddChild(level);
            LevelData levelData = ResourceLoader.Load<LevelData>(path);
            level.BuildGrid((int)Math.Sqrt(levelData.TileType.Length), 32, levelData);
            level.Editable = true;
            return level;
        }
    }
}