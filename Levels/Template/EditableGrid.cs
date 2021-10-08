using Godot;
using System;
using System.Collections.Generic;
using GodotExtension; 

public class EditableGrid : Grid
{
    public EditableGrid()
    {
/*         Name = "EditableGrid";
        MouseFilter = MouseFilterEnum.Ignore;
        DefineGridParameters();
        SetupGrid(); */
    }

    public override void DefineGridParameters() 
    {
        GridWidth = 7;
        GridHeight = 7;
        //MaxHops = 10; //Make hops a property or attach a level. Or define it all in a level?
/*         GoalsToNextLevel = goalsToNextLevel;
        if (hopsToAdd == 0)
        {
            HopsToAdd = (int)Math.Floor((decimal)MaxHops/2);
        }
        else
        {
            HopsToAdd = hopsToAdd;
        }
        ScoreTileCount = scoreTileCount;
        base.DefineGridParameters(); */
    }

}
