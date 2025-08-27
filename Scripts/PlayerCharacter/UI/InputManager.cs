using Godot;
using System;

/// <summary>
/// Manager used in options menu to allow key rebinding.
/// </summary>
public partial class InputManager : Control
{
    // @onready
    private OptionsMenu _optionsMenu;

    public override void _Ready()
    {
        _optionsMenu = GetNode<OptionsMenu>("..");
    }

    /// <summary>
    /// This function handles the keybinding mechanic.
    /// </summary>
    /// <param name="event"></param>
    public override void _Input(InputEvent @event)
    {
        if (_optionsMenu.IsRemapping)
        {
            if (@event is InputEventKey || (@event is InputEventMouseButton && @event.IsPressed()))
            {
                if (@event is InputEventMouseButton mouseButton && mouseButton.DoubleClick)
                {
                    // to avoid double clicks changes
                    mouseButton.DoubleClick = false;

                    // remap the action by setting a new input event, and change the name displayed
                    InputMap.ActionEraseEvents(_optionsMenu.ActionToRemap);
                    InputMap.ActionAddEvent(_optionsMenu.ActionToRemap, @event);
                    _optionsMenu.RemappingButton.Text = @event.AsText().TrimSuffix("(Physical)");

                    // reset the properties to default
                    _optionsMenu.IsRemapping = false;
                    _optionsMenu.ActionToRemap = null;
                    _optionsMenu.RemappingButton = null;

                    // prevents the current input from being directly modified again, to re modify it, it must be clicked again
                    AcceptEvent();
                }
            }
        }
    }
}
