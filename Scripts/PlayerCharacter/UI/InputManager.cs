using Godot;
using System;

public partial class InputManager : Control
{
    // @onready
    private OptionsMenu _optionsMenu;

    public override void _Ready()
    {
        _optionsMenu = GetNode<OptionsMenu>("..");
    }

    public override void _Input(InputEvent @event)
    {
        // this function handle the input of the inputBox, but more specifically in this case the keybinding mechanic
        if (_optionsMenu.IsRemapping)
        {
            if (@event is InputEventKey || (@event is InputEventMouseButton && @event.IsPressed()))
            {
                if (@event is InputEventMouseButton mouseButton && mouseButton.DoubleClick) 
                {
                    mouseButton.DoubleClick = false; // to avoid double clicks changes

                    // remap the action, by setting a new input event, and change the name displayed
                    InputMap.ActionEraseEvents(_optionsMenu.ActionToRemap);
                    InputMap.ActionAddEvent(_optionsMenu.ActionToRemap, @event);
                    _optionsMenu.RemappingButton.Text = @event.AsText().TrimSuffix("(Physical)");

                    // reset the properties to default
                    _optionsMenu.IsRemapping = false;
                    _optionsMenu.ActionToRemap = null;
                    _optionsMenu.RemappingButton = null;

                    AcceptEvent(); // prevents the current input from being directly modified again, to re modify it, it must be clicked again
                }
            }
        }
    }
}
