extends Control

#class name
class_name HUD

#references variables
@onready var currentStateLabelText = $HBoxContainer/VBoxContainer2/CurrentStateLabelText
@onready var moveSpeedLabelText = $HBoxContainer/VBoxContainer2/MoveSpeedLabelText
@onready var desiredMoveSpeedLabelText = $HBoxContainer/VBoxContainer2/DesiredMoveSpeedLabelText
@onready var velocityLabelText = $HBoxContainer/VBoxContainer2/VelocityLabelText
@onready var framesPerSecondLabelText = $HBoxContainer/VBoxContainer2/FramesPerSecondLabelText
@onready var nbJumpsAllowedInAirLabelText = $HBoxContainer/VBoxContainer2/NbJumpsInAirLabelText
@onready var speedLinesContainer = $SpeedLinesContrainer

func _ready():
	speedLinesContainer.visible = false #the speed lines will only be displayed when the character will dashing

func displayCurrentState(currentState):
	#this function manage the current state displayment
	
	var stateSignification : String 
	
	#set the state name to display according to the parameter value
	if currentState == 0:
		stateSignification = "Idle"
	if currentState == 1:
		stateSignification = "Walking"
	if currentState == 2:
		stateSignification = "Running"
	if currentState == 3:
		stateSignification = "Crouching"
	if currentState == 4:
		stateSignification = "Sliding"
	if currentState == 5:
		stateSignification = "Jumping"
	if currentState == 6:
		stateSignification = "In air"
	if currentState == 7:
		stateSignification = "On wall"
	if currentState == 8:
		stateSignification = "Dashing"
		
	currentStateLabelText.set_text(str(stateSignification))
	
func displayMoveSpeed(moveSpeed):
	#this function manage the move speed displayment
	moveSpeedLabelText.set_text(str(moveSpeed))
	
func displayDesiredMoveSpeed(desiredMoveSpeed):
	#this function manage the desired move speed displayment
	desiredMoveSpeedLabelText.set_text(str(desiredMoveSpeed))
	
func displayVelocity(velocity):
	#this function manage the current velocity displayment
	velocityLabelText.set_text(str(velocity))
	
func displayNbJumpsAllowedInAir(nbJumpsAllowedInAir):
	#this function manage the nb jumps allowed left displayment
	nbJumpsAllowedInAirLabelText.set_text(str(nbJumpsAllowedInAir))
	
func displaySpeedLines(dashTime):
	#this function manage the speed lignes displayment (only when the character is dashing)
	speedLinesContainer.visible = true 
	#when the dash is finished, hide the speed lines
	await get_tree().create_timer(dashTime).timeout
	speedLinesContainer.visible = false 
	
func _process(_delta):
	#this function manage the frames per second displayment
	framesPerSecondLabelText.set_text(str(Engine.get_frames_per_second()))
