using Godot;
using System;

public partial class PauseMenu : CanvasLayer
{
    public bool PauseMenuEnabled { get; set; } = false;
    private bool _mouseFree = false;

    [Export]
    public OptionsMenu OptionsMenu { get; set; }

    public override void _Ready()
    {
        SetPauseMenu(false, false);
        OptionsMenu = GetNode<OptionsMenu>("OptionsMenu");
    }

    public override void _Process(double delta)
    {
        // this function manages the mouse state
        // when the mouse is captured, it's disabled for the on-screen inputs (but not movement)
        if (Input.IsActionJustPressed("pauseMenu"))
        {
            if (!OptionsMenu.OptionsMenuEnabled)
            {
                SetPauseMenu(!PauseMenuEnabled, !PauseMenuEnabled);
                Input.MouseMode = _mouseFree ? Input.MouseModeEnum.Visible : Input.MouseModeEnum.Captured;
            }
        }
    }

    /// <summary>
    /// Sets the pause menu behaviour (visibility, mouse control, ...)
    /// </summary>
    /// <param name="value"></param>
    /// <param name="enable"></param>
    public void SetPauseMenu(bool value, bool enable)
    {
        Visible = value;
        _mouseFree = value;
        PauseMenuEnabled = enable;

        // stop game process when the pause menu is enabled
        Engine.TimeScale = PauseMenuEnabled ? 0.0 : 1.0;
    }

    /// <summary>
    /// Close pause menu.
    /// </summary>
    public void OnResumeButtonPressed()
    {
        SetPauseMenu(false, false);
        Input.MouseMode = Input.MouseModeEnum.Captured;
    }

    /// <summary>
    /// Close pause menu, but keep it enabled, to block possible reopen while being on the options menu.
    /// </summary>
    public void OnOptionsButtonPressed()
    {
        if (OptionsMenu != null)
        {
            SetPauseMenu(false, true);
            // Open options menu
            OptionsMenu.SetOptionsMenu(true);
        }
    }

    // Close the window, and so close the game.
    public void OnQuitButtonPressed()
    {
        GetTree().Quit();
    }
}
