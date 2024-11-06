extends Node3D

#class name
class_name CameraObject 

#camera variables
@export_group("camera variables")
@export var XAxisSensibility : float
@export var YAxisSensibility : float
@export var maxUpAngleView : float
@export var maxDownAngleView : float

#movement changes variables
@export_group("movement changes variables")
@export var crouchCameraDepth : float 
@export var crouchCameraLerpSpeed : float
@export var slideCameraDepth : float
@export var slideCameraLerpSpeed : float 

#fov variables
@export_group("fov variables")
var targetFOV : float 
var lastFOV : float 
var addonFOV : float 
@export var baseFOV : float
@export var crouchFOV : float 
@export var runFOV : float
@export var slideFOV : float
@export var dashFOV : float 
@export var fovChangeSpeed : float 
@export var fovChangeSpeedWhenDash : float 

#bob variables
@export_group("bob variables")
@export var headBobValue : float
@export var bobFrequency : float
@export var bobAmplitude : float

#tilt variables
@export_group("tilt variables")
@export var camTiltRotationValue : float 
@export var camTiltRotationSpeed : float 

#input variables
@export_group("input variables")
var mouseInput : Vector2 
@export var mouseInputSpeed : float 
var playerCharInputDirection : Vector2 

#others variables
var mouseFree : bool 

#references variables
@onready var camera = $Camera3D
@onready var playerChar = get_tree().get_first_node_in_group("PlayerCharacter")

func _ready():
	Input.set_mouse_mode(Input.MOUSE_MODE_CAPTURED) #set mouse as captured
	mouseFree = false 
	lastFOV = baseFOV #get the base FOV at start
	
func _unhandled_input(event):
	#this function manage camera rotation (360 on x axis, blocked at <= -60 and >= 60 on y axis, to not having the character do a complete head turn, which will be kinda weird)
	if event is InputEventMouseMotion:
		rotate_y(-event.relative.x * XAxisSensibility)
		camera.rotate_x(-event.relative.y * YAxisSensibility)
		camera.rotation.x = clamp(camera.rotation.x, deg_to_rad(maxUpAngleView), deg_to_rad(maxDownAngleView))
		mouseInput = event.relative #get position of the mouse in a 2D sceen, so save it in a Vector2 
		
func _process(delta):
	applies(delta)
	
	cameraBob(delta)
	
	cameraTilt(delta)
	
	changeMouseMode()
	
	FOVChange(delta) 
	
	lastFOV = targetFOV #get the last FOV used
	
func applies(delta):
	#this function manage the differents camera modifications relative to a specific state, except for the FOV
	match playerChar.currentState:
		playerChar.states.IDLE:
			position.y = lerp(position.y, 0.715, crouchCameraLerpSpeed * delta)
			rotation.z = lerp(rotation.z, deg_to_rad(0.0), slideCameraLerpSpeed * delta)
		playerChar.states.WALK:
			position.y = lerp(position.y, 0.715, crouchCameraLerpSpeed * delta)
			rotation.z = lerp(rotation.z, deg_to_rad(0.0), slideCameraLerpSpeed * delta)
		playerChar.states.RUN:
			position.y = lerp(position.y, 0.715, crouchCameraLerpSpeed * delta)
			rotation.z = lerp(rotation.z, deg_to_rad(0.0), slideCameraLerpSpeed * delta)
		playerChar.states.CROUCH:
			#lean the camera
			position.y = lerp(position.y, 0.715 + crouchCameraDepth, crouchCameraLerpSpeed * delta)
			rotation.z = lerp(rotation.z, deg_to_rad(6.0), slideCameraLerpSpeed * delta)
		playerChar.states.SLIDE:
			#lean the camera a bit more
			position.y = lerp(position.y, 0.715 + slideCameraDepth, crouchCameraLerpSpeed * delta)
			rotation.z = lerp(rotation.z, deg_to_rad(10.0), slideCameraLerpSpeed * delta)
			
func cameraBob(delta):
	#this function manage the bobbing of the camera when the character is moving
	if playerChar.currentState != playerChar.states.SLIDE and playerChar.currentState != playerChar.states.DASH: #the bobbing doesn't apply when the character is sliding or is dashing
		headBobValue += delta * playerChar.velocity.length() * float(playerChar.is_on_floor())
		camera.transform.origin = headbob(headBobValue) #apply the bob effect obtained to the camera
		
func headbob(time): 
	#some trigonometry stuff here, basically it uses the cosinus and sinus functions (sinusoidal function) to get a nice and smooth bob effect
	var pos = Vector3.ZERO
	pos.y = sin(time * bobFrequency) * bobAmplitude
	pos.x = cos(time * bobFrequency / 2) * bobAmplitude
	return pos
	
func cameraTilt(delta): 
	#this function manage the camera tilting when the character is moving on the x axis (left and right)
	if playerChar.moveDirection != Vector3.ZERO and playerChar.currentState != playerChar.states.CROUCH and playerChar.currentState != playerChar.states.SLIDE: #the camera tilting doesn't apply when the character is not moving, or is crouching or walking  
		playerCharInputDirection = playerChar.inputDirection #get input direction to know where the character is heading to
		#apply smooth tilt movement
		if !playerChar.is_on_floor(): rotation.z = lerp(rotation.z, -playerCharInputDirection.x * camTiltRotationValue/1.6, camTiltRotationSpeed * delta)
		else: rotation.z = lerp(rotation.z, -playerCharInputDirection.x * camTiltRotationValue, camTiltRotationSpeed * delta)
		
func changeMouseMode():
	#this function manage the mouse state
	#when the mouse is captured, you can't see it, and she's disable (not for the movement detection, but for the on screen inputs)
	#when the mouse is visible, you can see it, and she's enable
	if Input.is_action_just_pressed("freeMouse"):
		mouseFree = !mouseFree
		
		if mouseFree: Input.set_mouse_mode(Input.MOUSE_MODE_VISIBLE)
		else: Input.set_mouse_mode(Input.MOUSE_MODE_CAPTURED)
	
func FOVChange(delta):
	#FOV addon used to keep a logic FOV (for example, FOV when the character jumps right after running should be a bit higher than when he jumps right after walking)
	if lastFOV == baseFOV: addonFOV = 0
	if lastFOV == runFOV: addonFOV = 10
	if lastFOV == slideFOV: addonFOV = 30
	
	#get the corresponding FOV to the current state the character is
	match playerChar.currentState:
		playerChar.states.IDLE:
			targetFOV = baseFOV
		playerChar.states.CROUCH:
			targetFOV = crouchFOV
		playerChar.states.WALK:
			targetFOV = baseFOV
		playerChar.states.RUN:
			targetFOV = runFOV
		playerChar.states.SLIDE:
			targetFOV = slideFOV
		playerChar.states.DASH: 
			targetFOV = dashFOV
		playerChar.states.JUMP:
			targetFOV = baseFOV + addonFOV
		playerChar.states.INAIR:
			targetFOV = baseFOV + addonFOV
			
	#smoothly apply the FOV
	if playerChar.currentState == playerChar.states.DASH: camera.fov = lerp(camera.fov, targetFOV, fovChangeSpeedWhenDash * delta) #the dash state has it's own get-to-FOV speed, because the action is very quick and so the FOV change won't be seen with the regular get-to-FOV speed
	else: camera.fov = lerp(camera.fov, targetFOV, fovChangeSpeed * delta)
		
