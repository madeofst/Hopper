using Godot;
using System;

namespace Hopper
{
    public class Counter : RichTextLabel
    {
        //public World World { get; private set; }
        //public Player Player { get; set; }

        public Counter(){}

        public Counter(Vector2 size)
        {
            RectSize = size;

/*             DynamicFont dynamicFont = new DynamicFont();
            dynamicFont.FontData = GD.Load<DynamicFontData>("res://Fonts/rainyhearts.ttf");
            dynamicFont.OutlineColor = Colors.Firebrick;
            dynamicFont.OutlineSize = 1;
            dynamicFont.Size = 16;
            AddFontOverride("font",dynamicFont); */
            //var fontdata = Get("custom_fonts/normal_font/font_data") ;
            //
        }

        public override void _Ready()
        {
            //CallDeferred("ConnectNodesAndSignals");
        }

        public void ConnectNodesAndSignals()
        {
            //World = GetNode<World>("/root/World");
            //Player = GetNode<Player>("/root/World/Player");
            //MakeConnections();
        }

        public void UpdateText(string text)
        {
            Text = text;
        }

        public void UpdateText(int number)
        {
            Text = number.ToString();
        }

        public void UpdateText(float number)
        {
            Text = number.ToString();
        }

        public virtual void MakeConnections()
        {

        }

    }
}