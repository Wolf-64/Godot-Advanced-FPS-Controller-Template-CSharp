using Godot;
using System;

public partial class InputManager : Control
{
	// @onready
	OptionsMenu optionsMenu;

	public override void _Ready()
	{
		optionsMenu = GetNode<OptionsMenu>("..");
	}

	public override void _Input(InputEvent inputEvent)
	{
		// this function handle the input of the inputBox, but more specifically in this case the keybinding mechanic
		if (optionsMenu.IsRemapping)
		{
			if (inputEvent is InputEventKey || (inputEvent is InputEventMouseButton && inputEvent.IsPressed()))
			{
				if (inputEvent is InputEventMouseButton mouseButton && mouseButton.DoubleClick) 
				{
					mouseButton.DoubleClick = false; // to avoid double clicks changes

					// remap the action, by setting a new input event, and change the name displayed
					InputMap.ActionEraseEvents(optionsMenu.ActionToRemap);
					InputMap.ActionAddEvent(optionsMenu.ActionToRemap, inputEvent);
					optionsMenu.RemappingButton.Text = inputEvent.AsText().TrimSuffix("(Physical)");

					// reset the properties to default
					optionsMenu.IsRemapping = false;
					optionsMenu.ActionToRemap = null;
					optionsMenu.RemappingButton = null;

					AcceptEvent(); // prevents the current input from being directly modified again, to re modify it, it must be clicked again
				}
			}
		}
	}
}
