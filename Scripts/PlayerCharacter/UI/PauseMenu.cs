using Godot;
using System;

public partial class PauseMenu : CanvasLayer
{
	public bool pauseMenuEnabled { get; set; } = false;
	bool mouseFree = false;

	[Export]
	public OptionsMenu optionsMenu { get; set; }

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		setPauseMenu(false, false);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		// this function manage the mouse state
		// when the mouse is captured, you can't see it, and she's disable (not for the movement detection, but for the on screen inputs)
		// when the mouse is visible, you can see it, and she's enable
		if (Input.IsActionJustPressed("pauseMenu"))
		{
			if (!optionsMenu.optionsMenuEnabled)
			{
				if (pauseMenuEnabled)
					setPauseMenu(false, false);
				else
					setPauseMenu(true, true);

				// handle mouse mode
				if (mouseFree)
					Input.MouseMode = Input.MouseModeEnum.Visible;
				else
					Input.MouseMode = Input.MouseModeEnum.Captured;
			}
		}
	}

	public void setPauseMenu(bool value, bool enable)
	{
		// set the pause penu behaviour (visibility, mouse control, ...)
		Visible = value;
		mouseFree = value;
		pauseMenuEnabled = enable;

		// stop game process when the pause menu is enabled
		if (pauseMenuEnabled)
			Engine.TimeScale = 0.0;
		else
			Engine.TimeScale = 1.0;
	}
	public void OnResumeButtonPressed()
	{
		// close pause menu

		// there is a bug here, i don't know why, but the mouse keep being free when the pause menu is closed via the resume button
		// you can set the mouse to not free again by closing the menu directly with the key input
		// if you know how to resolve that issue, don't hesitate to make a post about it on the discussions tab of the project's Github repository

		setPauseMenu(false, false);
	}

	public void OnOptionsButtonPressed()
	{
		// close pause menu, but keep it enabled, to block possible reopen while being on the options menu
		if (optionsMenu != null)
		{
			setPauseMenu(false, true);
			optionsMenu.setOptionsMenu(true); // open options menu
		}
	}

	public void OnQuitButtonPRessed()
	{
		// close the window, and so close the game
		GetTree().Quit();
	}
}
