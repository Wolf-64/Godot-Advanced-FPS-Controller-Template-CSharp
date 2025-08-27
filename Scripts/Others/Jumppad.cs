using Godot;
using System;

public partial class Jumppad : CsgCylinder3D
{
    [ExportGroup("Value variables")]
    [Export]
    public float JumpBoostValue { get; set; }

    /// <summary>
    /// Hook up this event to an Area3D node attached to your jumppad
    /// </summary>
    /// <param name="area"></param>
    public void OnArea3DAreaEntered(Area3D area)
    {
        if (area.GetParent() is PlayerCharacter player)
        {
            player.Jump(JumpBoostValue, true);
        }
    }
}
