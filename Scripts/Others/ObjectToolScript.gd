extends Node3D

@export_group("Knockback variables")
@export var knockbackAmount : float
@export var waitTimeBefCanUseKnobaAgain : float
var waitTimeBefCanUseKnobaAgainRef : float

@onready var knockbackToolAttackPoint : Node3D = $KnockbackTool/KnockbackToolAttackPoint
@onready var animationPlayer : AnimationPlayer = $AnimationPlayer
@onready var hud : Control = $"../../../HUD"

signal sendKnockback

func _ready():
	waitTimeBefCanUseKnobaAgainRef = waitTimeBefCanUseKnobaAgain
	
func _process(delta):
	use(delta)
	
	timeManagement(delta)
	
	sendProperties()
	
func use(_delta : float):
	if Input.is_action_just_pressed("useKnockbackTool"):
		#send a knockback action to the character
		if waitTimeBefCanUseKnobaAgain <= 0.0:
			waitTimeBefCanUseKnobaAgain = waitTimeBefCanUseKnobaAgainRef
			
			emit_signal("sendKnockback", knockbackAmount, -global_transform.basis.z.normalized())
			animationPlayer.play("useKnockbackTool")
			
func timeManagement(delta : float):
	if waitTimeBefCanUseKnobaAgain > 0.0: waitTimeBefCanUseKnobaAgain -= delta
	
func sendProperties():
	#display knockback tool properties
	hud.displayKnockbackToolWaitTime(waitTimeBefCanUseKnobaAgain)
