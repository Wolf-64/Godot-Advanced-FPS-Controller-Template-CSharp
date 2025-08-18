using Godot;
using System;

public partial class Jumppad : CsgCylinder3D
{
    [ExportGroup("value variables")]
    [Export]
    public float JumpBoostValue { get; set; }
    // Called when the node enters the scene tree for the first time.
    public void OnArea3DAreaEntered(Area3D area)
    {
        if (area.GetParent() is PlayerCharacter player)
        {
            player.Jump(JumpBoostValue, true);
        }
    }
}
