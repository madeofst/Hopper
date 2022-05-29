using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Hopper
{
    public class Map : Node2D
    {
        public List<Location> Locations;
        public Pointer Pointer;
        private PauseMenu PauseMenu;
        
        public override void _Ready()
        {
            PauseMenu = GetNode<PauseMenu>("../PauseMenu");
            
            PauseMenu.QuitButton.Connect("pressed", this, nameof(QuitToMenu));
            PauseMenu.Connect(nameof(PauseMenu.Quit), this, nameof(QuitToMenu));
            PauseMenu.Connect(nameof(PauseMenu.Unpause), this, nameof(Unpause));

            Locations = GetChildren().OfType<Location>().ToList<Location>();
            Pointer = GetNode<Pointer>("Pointer");
            Pointer.SetLocations(Locations);
        }

        private void QuitToMenu()
        {
            QueueFree();
        }

        private void Unpause()
        {
            Pointer.SetProcessInput(false);
        }

        public void UnlockWorld(string[] worldsToUnlock)
        {
            foreach (var s in worldsToUnlock)
            {
                Location l = GetNode<Location>((string)s);
                if (l.Active == false)
                {
                    l.Active = true;
                    l.NewlyActivated = true;
                }
            }
            Pointer.Target.NewlyActivated = false;
            Pointer.Target.Complete = true;
        }

        public override void _Process(float delta)
        {
            foreach (Location l in Locations)
            {
                if (Pointer.Position == l.Position)
                {
                    l.Scale = new Vector2(1.2f, 1.2f);
                }
                else
                {
                    l.Scale = new Vector2(1f, 1f);
                }
            }
        }

        public override void _Input(InputEvent @event)
        {
            if (@event.IsActionPressed("ui_cancel"))
            {
                Pointer.SetProcessInput(false);
                PauseMenu.SetPosition(Position);
                PauseMenu.Visible = true;
                PauseMenu.SetMode(PauseMenu.PauseMenuMode.Map);
                PauseMenu.AnimateShow();
            }
        }

    }
}