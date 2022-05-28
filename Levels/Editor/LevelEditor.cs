using Godot;
using System;

namespace Hopper
{
    public class LevelEditor : Node2D
    {
        private Menu Menu { get; set; }

        private ResourceRepository Resources { get; set; }
        private LevelFactory levelFactory { get; set; }
        private Level CurrentLevel { get; set; }
        private World TestWorld { get; set; }

        //Parameter edit boxes
        private LineEdit PlayerStartX { get; set; }
        private LineEdit PlayerStartY { get; set; }
        private LineEdit StartingHops { get; set; }
        private LineEdit MaximumHops { get; set; }
        private LineEdit ScoreRequired { get; set; }

        public override void _Ready()
        {
            Resources = GetNode<ResourceRepository>("/root/ResourceRepository");
            levelFactory = new LevelFactory(Resources);
            Menu = GetNode<Menu>("/root/Menu");
            CallDeferred("Init");
        }

        private void Init()
        {
            PlayerStartX = GetNode<LineEdit>("Controls/VBoxContainer/HBoxContainer/PlayerStartX");
            PlayerStartY = GetNode<LineEdit>("Controls/VBoxContainer/HBoxContainer/PlayerStartY");
            StartingHops = GetNode<LineEdit>("Controls/VBoxContainer/HBoxContainer2/StartingHops");
            MaximumHops = GetNode<LineEdit>("Controls/VBoxContainer/HBoxContainer2/MaximumHops");
            ScoreRequired = GetNode<LineEdit>("Controls/VBoxContainer/VBoxContainer/ScoreRequired");
        }

        //New
        private void NewBlankLevel(int FillWith = 0)
        {   
            if (CurrentLevel != null) CurrentLevel.QueueFree();
            CurrentLevel = levelFactory.New((Type)FillWith);
            AddChild(CurrentLevel);
            CurrentLevel.Build(Resources); //TODO: may be able to come out and run automatically
            PopulateParameterValues();
            CurrentLevel.Connect(nameof(Level.LevelParametersUpdated), this, nameof(PopulateParameterValues));
            CurrentLevel.Editable = true;
        }

        //Save
        private void ShowSaveDialog()
        {
            AcceptDialog SaveDialog = GetNode<AcceptDialog>("Dialogs/SaveDialog");
            LineEdit SaveName = SaveDialog.GetNode<LineEdit>("VBoxContainer/LineEdit");
            SaveName.Text = CurrentLevel.LevelName;
            SaveDialog.PopupCentered();
        }
        
        private void TextChanged(string newText)
        {
            CurrentLevel.LevelName = newText;
        }

        private void SaveAndPlayLevel()
        {
            if (CurrentLevel != null)
            {
                SaveCurrentLevel();
                World TestWorld = (World)GD.Load<PackedScene>("res://World/World.tscn").Instance();
                GetTree().Root.AddChild(TestWorld);
                TestWorld.Init(new string[]{}, Position, true, CurrentLevel.LevelName);
            }
        }

        private void SaveCurrentLevel()
        {
            if (CurrentLevel != null)
            {
                WriteParameterValues();
                Error error = levelFactory.Save(CurrentLevel);
                //GD.PrintErr(error);
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
            CurrentLevel.Build(Resources); //TODO: may be able to come out and run automatically
            PopulateParameterValues();
            CurrentLevel.Connect(nameof(Level.LevelParametersUpdated), this, nameof(PopulateParameterValues));
            CurrentLevel.Editable = true;
        }

        private void PopulateParameterValues()
        {
            PlayerStartX.Text = (CurrentLevel.PlayerStartPosition.x - 1).ToString();
            PlayerStartY.Text = (CurrentLevel.PlayerStartPosition.y - 1).ToString();
            StartingHops.Text = CurrentLevel.StartingHops.ToString();
            MaximumHops.Text = CurrentLevel.MaximumHops.ToString();
            ScoreRequired.Text = CurrentLevel.ScoreRequired.ToString();
        }

        private void WriteParameterValues()
        {
            CurrentLevel.PlayerStartPosition = new Vector2(
                Int16.Parse(PlayerStartX.Text), 
                Int16.Parse(PlayerStartY.Text));
            CurrentLevel.StartingHops = Int16.Parse(StartingHops.Text);
            CurrentLevel.MaximumHops = Int16.Parse(MaximumHops.Text);
            CurrentLevel.ScoreRequired = Int16.Parse(ScoreRequired.Text);
        }

        //Randomize
        private void NewRandomLevel()
        {
            if (CurrentLevel != null) CurrentLevel.QueueFree();
            CurrentLevel = levelFactory.Generate();
            AddChild(CurrentLevel);
            CurrentLevel.Build(Resources); //TODO: may be able to come out and run automatically
            PopulateParameterValues();
            CurrentLevel.Connect(nameof(Level.LevelParametersUpdated), this, nameof(PopulateParameterValues));
            CurrentLevel.Editable = true;
        }

        public void GoHome()
        {
            Menu.ShowMenu();
            QueueFree();
        }
    }
}