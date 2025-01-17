extends CanvasLayer

@export_group("Input variables")
@onready var inputKeybindBox : PackedScene = preload("res://Scenes/InputKeybindOptionScene.tscn")
@onready var inputList : VBoxContainer = $TabContainer/Controls/Control/ScrollContainer/InputList
var isRemapping : bool = false
var actionToRemap = null
var remappingButton = null

@export_group("Video variables")
var fullscreeenOn : bool = false
@onready var resolOptionsButton : OptionButton = $TabContainer/Video/CenterContainer/VBoxContainer/ResolutionOption/OptionButton
var resList : Dictionary = {}

#list of inputs actions to display (key is input name used by Godot, value is what is displayed to the user)
var inputActions : Dictionary = {
	"moveLeft" : "Move left",
	"moveRight" : "Move right",
	"moveBackward" : "Move backward",
	"moveForward" : "Move forward",
	"jump" : "Jump",
	"run" : "Run",
	"crouch | slide" : "Crouch | Slide",
	"dash" : "Dash",
	"grappleHook" : "Grapple hook",
	"useKnockbackTool" : "Knockack tool",
	"pauseMenu" : "Pause menu"
}

@export_group("Audio variables")
var masterBusIndex : int = AudioServer.get_bus_index("Master")
var volumeIsMute : bool = false

@export_group("Parent variables")
@export var pauseMenu : CanvasLayer

var optionsMenuEnabled : bool = false

func _ready():
	setOptionsMenu(false)
	
	createInputsList()
	
	createResolutionsSelection()
	
func setOptionsMenu(value : float):
	#set the options penu behaviour
	visible = value
	optionsMenuEnabled = value
	
# -------------------------------- Input part ----------------------------------
func createInputsList():
	#this function handle the inputs list creation
	
	InputMap.load_from_project_settings() #load the inputs set in the project settings
	
	#clear inputs (to avoid duplicates and remove unwanted inputs boxes)
	for inputBoxIndex in inputList.get_children(): inputBoxIndex.queue_free()
	
	#for each action/input
	for action in inputActions:
		#create an instance of the inputBox scene
		var inputBox = inputKeybindBox.instantiate()
		
		#get the child nodes
		var actionLabel = inputBox.find_child("ActionLabel")
		var inputButton = inputBox.find_child("InputButton")
		actionLabel.text = inputActions[action]
		
		#set action name
		var events = InputMap.action_get_events(action)
		if events.size() > 0: inputButton.text = events[0].as_text().trim_suffix("(Physical)")
		else: inputButton.text = ""
		
		inputList.add_child(inputBox)
		inputButton.pressed.connect(_on_input_button_pressed.bind(inputButton, action)) #connect button pressed signal to "on_input_button_pressed" function
		
		#create and initialize a separator to add between each instance of inputBox
		var horSepar : HSeparator = HSeparator.new()
		var horSeparTheme : Theme = Theme.new()
		horSepar.theme = horSeparTheme
		horSepar.modulate = Color(255, 255, 255, 0)
		inputList.add_child(horSepar)
		
func _on_input_button_pressed(inputButton, action):
	#select properties to modify, and so call the keybinding function (which is in the inputBox script)
	if !isRemapping:
		isRemapping = true
		actionToRemap = action
		remappingButton = inputButton
		inputButton.text = "..."
		
func _on_reset_button_pressed():
	#recall the function to cruch all modifications (in others words, reset the inputs list)
	createInputsList()
	
# -------------------------------- Video part ----------------------------------
func createResolutionsSelection():
	#this function handle the screen resolutions fill for the options button
	
	var resToAdd : Array = [[1920, 1080], [1280, 720], [1152, 648], [768, 432]] #list of resolutions to add
	
	#for each resolution, get the width and height, add them to the resolution List (which will be useful for the resize option)
	#and add them to the options button
	for res in range(0, resToAdd.size()):
		var widthVal = resToAdd[res][0]
		var heightVal = resToAdd[res][1]
		resList[res] = [widthVal, heightVal]
		resolOptionsButton.add_item(str(widthVal,"x",heightVal), res)
	resolOptionsButton.select(2)
		
func _on_fullscreen_check_box_pressed():
	#this function handle the fullscreen option, by changing the window display mode
	fullscreeenOn = !fullscreeenOn
	
	if fullscreeenOn: DisplayServer.window_set_mode(DisplayServer.WINDOW_MODE_FULLSCREEN)
	else: DisplayServer.window_set_mode(DisplayServer.WINDOW_MODE_WINDOWED)
	
func _on_option_button_item_selected(ind: int):
	#this function handle the resize window option, by getting the corresponding values from resList, and applying them to the window
	
	#ind+1 because the createResolutionsSelection loop has begun at 1
	var resWidth : int = resList[ind][0]
	var resHeight : int = resList[ind][1]
	DisplayServer.window_set_size(Vector2i(resWidth, resHeight))
	
	
	
# -------------------------------- Audio part ----------------------------------
func _on_check_box_pressed():
	#this function handle the mute option
	AudioServer.set_bus_mute(masterBusIndex, false if volumeIsMute else true)
	volumeIsMute = !volumeIsMute
	
func _on_back_button_pressed():
	#close the options menu, and re open the pause menu
	if pauseMenu != null:
		setOptionsMenu(false)
		pauseMenu.setPauseMenu(true, true)
