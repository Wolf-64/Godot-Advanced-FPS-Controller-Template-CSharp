extends Control

@onready var optionsMenu : CanvasLayer = $".."

func _input(event):
	#this function handle the input of the inputBox, but more specifically in this case the keybinding mechanic
	if optionsMenu.isRemapping:
		if (event is InputEventKey or (event is InputEventMouseButton and event.pressed)):
			if event is InputEventMouseButton and event.double_click: event.double_click = false #to avoid double clicks changes
			
			#remap the action, by setting a new input event, and change the name displayed
			InputMap.action_erase_events(optionsMenu.actionToRemap)
			InputMap.action_add_event(optionsMenu.actionToRemap, event)
			optionsMenu.remappingButton.text = event.as_text().trim_suffix("(Physical)")
			
			#reset the properties to default
			optionsMenu.isRemapping = false
			optionsMenu.actionToRemap = null
			optionsMenu.remappingButton = null
			
			accept_event() #prevents the current input from being directly modified again, to re modify it, it must be clicked again
