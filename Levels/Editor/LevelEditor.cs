using Godot;
using System;

namespace Hopper
{
    public class LevelEditor : Node2D
    {
        private StartMenu Menu { get; set; }

        private ResourceRepository Resources { get; set; }
        private LevelFactory levelFactory { get; set; }
        private Level CurrentLevel { get; set; }
        private Stage TestStage { get; set; }

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
            Menu = GetNode<StartMenu>("/root/StartMenu");
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
            CurrentLevel.Build(Resources);
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
            string Error = CheckValidityOfLevel();
            if (Error != null)
            {
                GD.Print(Error);
            }
            else
            {
                SaveCurrentLevel();
                Stage TestStage = (Stage)GD.Load<PackedScene>("res://Stage/Stage.tscn").Instance();
                GetViewport().AddChild(TestStage);
                TestStage.Init(new StageData(1, "", 0), new string[]{}, Position + new Vector2(240, 135), true, CurrentLevel.LevelName);
            }
        }

        private void SaveCurrentLevel()
        {
            WriteParameterValues();
            levelFactory.Save(CurrentLevel);
        }

        private string CheckValidityOfLevel()
        {
            if (CurrentLevel == null) return "No level.";
            if (!CurrentLevel.Grid.HasOneGoal()) return "Level must have exactly one goal tile.";
            return null;
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
            CurrentLevel.Build(Resources);
            PopulateParameterValues();
            CurrentLevel.Connect(nameof(Level.LevelParametersUpdated), this, nameof(PopulateParameterValues));
            CurrentLevel.Editable = true;
        }

        private void PopulateParameterValues()
        {
            PlayerStartX.Text = (CurrentLevel.PlayerStartPosition.x - 1).ToString();
            PlayerStartY.Text = (CurrentLevel.PlayerStartPosition.y - 1).ToString();
            StartingHops.Text = CurrentLevel.StartingHops.ToString();  //TODO: remove starting hops
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
/*         private void NewRandomLevel()
        {
            if (CurrentLevel != null) CurrentLevel.QueueFree();
            CurrentLevel = levelFactory.Generate();
            AddChild(CurrentLevel);
            CurrentLevel.Build(Resources);
            PopulateParameterValues();
            CurrentLevel.Connect(nameof(Level.LevelParametersUpdated), this, nameof(PopulateParameterValues));
            CurrentLevel.Editable = true;
        } */

        public void GoHome()
        {
            Menu.ShowMenu();
            GetNode<HUD>("/root/HUD").QueueFree();
            QueueFree();
        }
    }
}