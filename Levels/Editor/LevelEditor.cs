using Godot;
using System;

namespace Hopper
{
    public class LevelEditor : Node2D
    {
        private LevelFactory levelFactory { get; set; } = new LevelFactory();
        private Level CurrentLevel { get; set; }

        //New
        private void NewBlankLevel()
        {
            if (CurrentLevel != null) CurrentLevel.QueueFree();
            CurrentLevel = levelFactory.New();
            AddChild(CurrentLevel);
            CurrentLevel.Build(); //TODO: may be able to come out and run automatically
            CurrentLevel.Editable = true;
        }

        //Save
        private void ShowSaveDialog()
        {
            AcceptDialog SaveDialog = GetNode<AcceptDialog>("Dialogs/SaveDialog");
            SaveDialog.PopupCentered();
        }
        
        private void TextChanged(string newText)
        {
            CurrentLevel.Name = newText;
        }

        private void SaveCurrentLevel()
        {
            if (CurrentLevel != null)
            {
                Error error = levelFactory.Save(CurrentLevel);
                GD.PrintErr(error);
            }
        }

        //Load
        private void ShowLoadDialog()
        {
            FileDialog LoadDialog = GetNode<FileDialog>("Dialogs/LoadDialog");
            LoadDialog.PopupCentered();
        }

        private void LoadSelectedFile(string path)
        {
            if (CurrentLevel != null) CurrentLevel.QueueFree();
            CurrentLevel = levelFactory.Load(path);
            AddChild(CurrentLevel);
            CurrentLevel.Build(); //TODO: may be able to come out and run automatically
            CurrentLevel.Editable = true;
        }

        //Randomize
        private void NewRandomLevel()
        {
            NewBlankLevel();
        }




    }
}