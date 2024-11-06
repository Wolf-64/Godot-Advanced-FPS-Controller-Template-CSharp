extends HBoxContainer

#HUD size variable
var HUDBaseSize : Vector2 = Vector2(1920, 1080)

func _ready():
	resizeHUD()
	
func resizeHUD():
	#this function handle the resize of the hud (only the text part), to ensure that the hud always match the window size
	var windowSize : Vector2 = get_viewport().get_size()
	scale.x = windowSize.x / HUDBaseSize.x
	scale.y = windowSize.y / HUDBaseSize.y 
	
func _notification(what: int):
	#this function call the resizeHUD one every time the window is resized
	if what == NOTIFICATION_RESIZED:
		resizeHUD()
	
