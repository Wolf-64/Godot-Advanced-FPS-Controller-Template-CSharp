extends Control

#class name
class_name HUD

#references variables
@onready var currentStateLabelText = $HBoxContainer/VBoxContainer2/CurrentStateLabelText
@onready var moveSpeedLabelText = $HBoxContainer/VBoxContainer2/MoveSpeedLabelText
@onready var desiredMoveSpeedLabelText = $HBoxContainer/VBoxContainer2/DesiredMoveSpeedLabelText
@onready var velocityLabelText = $HBoxContainer/VBoxContainer2/VelocityLabelText
@onready var nbJumpsAllowedInAirLabelText = $HBoxContainer/VBoxContainer2/NbJumpsInAirLabelText
@onready var nbDashsAllowedLabelText = $HBoxContainer/VBoxContainer2/NbDashsAllowedLabelText
@onready var slideWaitTimeLabelText = $HBoxContainer/VBoxContainer2/SlideWaitTimeLabelText
@onready var dashWaitTimeLabelText = $HBoxContainer/VBoxContainer2/DashWaitTimeLabelText
@onready var knockbackToolWaitTimeLabelText = $HBoxContainer/VBoxContainer2/KnockbackToolWaitTimeLabelText
@onready var grappleToolWaitTimeLabelText = $HBoxContainer/VBoxContainer2/GrappleToolWaitTimeLabelText
@onready var framesPerSecondLabelText = $HBoxContainer/VBoxContainer2/FramesPerSecondLabelText
@onready var speedLinesContainer = $SpeedLinesContrainer

func _ready():
	speedLinesContainer.visible = false #the speed lines will only be displayed when the character will dashing

func displayCurrentState(currentState : int):
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
	if currentState == 9:
		stateSignification = "Grappling"
	if currentState == 10:
		stateSignification = "Ground pounding"
		
	currentStateLabelText.set_text(str(stateSignification))
	
func displayMoveSpeed(moveSpeed : float):
	#this function manage the move speed displayment
	moveSpeedLabelText.set_text(str(moveSpeed))
	
func displayDesiredMoveSpeed(desiredMoveSpeed : float):
	#this function manage the desired move speed displayment
	desiredMoveSpeedLabelText.set_text(str(desiredMoveSpeed))
	
func displayVelocity(velocity : float):
	#this function manage the current velocity displayment
	velocityLabelText.set_text(str(velocity))
	
func displayNbJumpsAllowedInAir(nbJumpsAllowedInAir : int):
	#this function manage the nb jumps allowed left displayment
	nbJumpsAllowedInAirLabelText.set_text(str(nbJumpsAllowedInAir))
	
func displayNbDashsAllowed(nbDashsAllowed : int):
	#this function manage the nb dashs allowed left displayment
	nbDashsAllowedLabelText.set_text(str(nbDashsAllowed))
	
func displaySlideWaitTime(slideWaitTime : float):
	slideWaitTimeLabelText.set_text(str(slideWaitTime))
	
func displayDashWaitTime(dashWaitTime : float):
	dashWaitTimeLabelText.set_text(str(dashWaitTime))
	
func displayKnockbackToolWaitTime(timeBefCanUseAgain : float):
	#this function manage the knockback tool time left displayment
	knockbackToolWaitTimeLabelText.set_text(str(timeBefCanUseAgain))
	
func displayGrappleHookToolWaitTime(timeBefCanUseAgain : float):
	#this function manage the grapple hook time left displayment
	grappleToolWaitTimeLabelText.set_text(str(timeBefCanUseAgain))
	
func displaySpeedLines(dashTime):
	#this function manage the speed lignes displayment (only when the character is dashing)
	speedLinesContainer.visible = true 
	#when the dash is finished, hide the speed lines
	await get_tree().create_timer(dashTime).timeout
	speedLinesContainer.visible = false 
	
func _process(_delta):
	#this function manage the frames per second displayment
	framesPerSecondLabelText.set_text(str(Engine.get_frames_per_second()))
