using Godot;
using System;

namespace Hopper 
{
    public class VictoryScreen : Control
    {
        public override void _Input(InputEvent @event)
        {
            if (@event.IsActionPressed("ui_accept") ||
                @event.IsActionPressed("ui_cancel") )
            {
                GetViewport().SetInputAsHandled();
                QueueFree();
                StartMenu StartMenu = GetNode<StartMenu>("/root/StartMenu");
                StartMenu.UpdateLoadButton();
                StartMenu.ShowMenu();
            }
        }
    }
}
