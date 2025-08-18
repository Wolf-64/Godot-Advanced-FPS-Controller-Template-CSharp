using Godot;
using System;
using System.Collections.Generic;

public partial class OptionsMenu : CanvasLayer
{
    [ExportGroup("Input variables")]
    // @onready 
    private PackedScene _inputKeybindBox;
    // @onready 
    private VBoxContainer _inputList;
    public bool IsRemapping { get; set; } = false;
    public string ActionToRemap { get; set; } = null;
    public Button RemappingButton { get; set; } = null;

    [ExportGroup("Video variables")]
    private bool _fullscreeenOn = false;
    // @onready 
    private OptionButton _resolOptionsButton;
    private Dictionary<int, (int, int)> _resList = new Dictionary<int, (int, int)>();

    // list of inputs actions to display (key is input name used by Godot, value is what is displayed to the user)
    private Dictionary<string, string> _inputActions = new Dictionary<string, string>() {
        { "moveLeft", "Move left" },
        { "moveRight", "Move right" },
        { "moveBackward", "Move backward" },
        { "moveForward", "Move forward" },
        { "jump", "Jump" },
        { "run", "Run" },
        { "crouch | slide", "Crouch | Slide" },
        { "dash", "Dash" },
        { "grappleHook", "Grapple hook" },
        { "useKnockbackTool", "Knockack tool" },
        { "pauseMenu", "Pause menu" }
    };

    [ExportGroup("Audio variables")]
    private int _masterBusIndex = AudioServer.GetBusIndex("Master");
    private bool _volumeIsMute = false;

    [ExportGroup("Parent variables")]
    [Export]
    public PauseMenu pauseMenu { get; set; }

    public bool OptionsMenuEnabled { get; set; } = false;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        _inputKeybindBox = ResourceLoader.Load<PackedScene>
        (
            "res://Scenes/InputKeybindOptionScene.tscn"
        );
        _inputList = GetNode<VBoxContainer>
        (
            "TabContainer/Controls/Control/ScrollContainer/InputList"
        );
        _resolOptionsButton = GetNode<OptionButton>
        (
            "TabContainer/Video/CenterContainer/VBoxContainer/ResolutionOption/OptionButton"
        );

        SetOptionsMenu(false);
        CreateInputsList();
        CreateResolutionsSelection();
    }

    public void SetOptionsMenu(bool value)
    {
        // set the options penu behaviour
        Visible = value;
        OptionsMenuEnabled = value;
    }

    // -------------------------------- Input part ----------------------------------
    // this function handle the inputs list creation
    public void CreateInputsList()
    {
        InputMap.LoadFromProjectSettings(); // load the inputs set in the project settings

        // clear inputs (to avoid duplicates and remove unwanted inputs boxes)
        foreach (var inputBoxIndex in _inputList.GetChildren())
        {
            inputBoxIndex.QueueFree();
        }

        //  for each action/input
        foreach (string action in _inputActions.Keys)
        {
            // create an instance of the inputBox scene
            Node inputBox = _inputKeybindBox.Instantiate();

            //  get the child nodes
            var actionLabel = inputBox.FindChild("ActionLabel") as Label;
            var inputButton = inputBox.FindChild("InputButton") as Button;
            actionLabel.Text = _inputActions[action];

            //  set action name
            Godot.Collections.Array<InputEvent> events = InputMap.ActionGetEvents(action);
            if (events.Count > 0)
            {
                inputButton.Text = events[0].AsText().TrimSuffix("(Physical)");
            }
            else
            {
                inputButton.Text = "";
            }

            _inputList.AddChild(inputBox);
            // connect button pressed signal to "OnInputButtonPressed" function
            inputButton.Pressed += () => OnInputButtonPressed(inputButton, action); 

            // create and initialize a separator to add between each instance of inputBox
            HSeparator horSepar = new();
            Theme horSeparTheme = new();
            horSepar.Theme = horSeparTheme;
            horSepar.Modulate = new Color(255, 255, 255, 0);
            _inputList.AddChild(horSepar);
        }
    }

    public void OnInputButtonPressed(Button inputButton, string action)
    {
        // select properties to modify, and so call the keybinding function
        // (which is in the inputBox script)
        if (!IsRemapping)
        {
            IsRemapping = true;
            ActionToRemap = action;
            RemappingButton = inputButton;
            inputButton.Text = "...";
        }
    }

    public void OnResetButtonPressed()
    {
        // recall the function to cruch all modifications (in others words, reset the inputs list)
        CreateInputsList();
    }

    //  -------------------------------- Video part ----------------------------------
    // this function handles the screen resolutions fill for the options button
    private void CreateResolutionsSelection()
    {
        List<(int, int)> resToAdd = new List<(int width, int height)>
        {
            (1920, 1080),
            (1280, 720),
            (1152, 648),
            (768, 432)
        }; // list of resolutions to add

        // for each resolution, get the width and height, add them to the resolution List (which will be useful for the resize option)
        // and add them to the options button
        for (int res = 0; res < resToAdd.Count; res++)
        {
            var (widthVal, heightVal) = resToAdd[res];
            _resList[res] = (widthVal, heightVal);
            _resolOptionsButton.AddItem($"{widthVal}x{heightVal}", res);
        }

        _resolOptionsButton.Select(2);
    }

    // this function handle the fullscreen option, by changing the window display mode
    public void OnFullScreenCheckBoxPressed()
    {
        _fullscreeenOn = !_fullscreeenOn;

        if (_fullscreeenOn)
        {
            DisplayServer.WindowSetMode(DisplayServer.WindowMode.Fullscreen);
        }
        else
        {
            DisplayServer.WindowSetMode(DisplayServer.WindowMode.Windowed);
        }
    }
    public void OnOptionButtonItemSelected(int ind)
    {
        // this function handle the resize window option, by getting the corresponding values 
        // from resList, and applying them to the window
        // ind+1 because the createResolutionsSelection loop has begun at 1
        int resWidth = _resList[ind].Item1;
        int resHeight = _resList[ind].Item2;
        DisplayServer.WindowSetSize(new Vector2I(resWidth, resHeight));
    }

    //  -------------------------------- Audio part ----------------------------------
    public void OnCheckBoxSelected()
    {
        // this function handle the mute option
        AudioServer.SetBusMute(_masterBusIndex, !_volumeIsMute);
        _volumeIsMute = !_volumeIsMute;
    }

    public void OnBackButtonPressed()
    {
        // close the options menu, and re open the pause menu
        if (pauseMenu != null)
        {
            SetOptionsMenu(false);
            pauseMenu.SetPauseMenu(true, true);
        }
    }
}
