using Godot;
using System;

public partial class CameraObject : Node3D
{
	//  camera variables
	[ExportGroup("camera variables")]
	[Export]
	public float XAxisSensibility { get; set; } = 0.008f;
	[Export]
	public float YAxisSensibility { get; set; } = 0.008f;
	[Export]
	public float maxUpAngleView { get; set; } = -90f;
	[Export]
	public float maxDownAngleView { get; set; } = 90f;

	//  movement changes variables
	[ExportGroup("movement changes variables")]
	[Export]
	public float crouchCameraDepth { get; set; } = -0.2f;
	[Export]
	public float crouchCameraLerpSpeed { get; set; } = 8f;
	[Export]
	public float slideCameraDepth { get; set; } = -0.5f;
	[Export]
	public float slideCameraLerpSpeed { get; set; } = 8f;

	//  fov variables
	[ExportGroup("fov variables")]
	float targetFOV;
	float lastFOV;
	float addonFOV;
	[Export]
	public float baseFOV { get; set; } = 90f;
	[Export]
	public float crouchFOV { get; set; } = 75f;
	[Export]
	public float runFOV { get; set; } = 100f;
	[Export]
	public float slideFOV { get; set; } = 120f;
	[Export]
	public float dashFOV { get; set; } = 150f;
	[Export]
	public float fovChangeSpeed { get; set; } = 4f;
	[Export]
	public float fovChangeSpeedWhenDash { get; set; } = 3f;

	//  bob variables
	[ExportGroup("bob variables")]
	[Export]
	public float headBobValue { get; set; }
	[Export]
	public float bobFrequency { get; set; } = 0.8f;
	[Export]
	public float bobAmplitude { get; set; } = 0.06f;

	//  tilt variables
	[ExportGroup("tilt variables")]
	[Export]
	public float camTiltRotationValue { get; set; } = 0.35f;
	[Export]
	public float camTiltRotationSpeed { get; set; } = 2.2f;

	//  shake variables
	[ExportGroup("camera shake variables")]
	float shakeForce;
	[Export]
	public float shakeDuration { get; set; } = 0.35f;
	float shakeDurationRef;
	[Export]
	public float shakeFade { get; set; } = 6f;
	RandomNumberGenerator rng = new RandomNumberGenerator();
	bool canCameraShake = false;

	// input variables
	[ExportGroup("input variables")]
	Vector2 mouseInput;
	[Export]
	public float mouseInputSpeed { get; set; } = 2f;
	Vector2 playCharInputDir;

	//  references variables
	// @onready
	Camera3D camera;
	// @onready
	PlayerCharacter playerChar;
	// @onready
	CanvasLayer pauseMenu;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		camera = GetNode<Camera3D>("Camera3D");
		playerChar = GetNode<PlayerCharacter>("..");
		pauseMenu = GetNode<CanvasLayer>("../PauseMenu");

		Input.MouseMode = Input.MouseModeEnum.Captured;

		lastFOV = baseFOV;
		shakeDurationRef = shakeDuration;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		applies((float)delta);
		cameraBob((float)delta);
		cameraTilt((float)delta);
		FOVChange((float)delta);
		lastFOV = targetFOV; // get the last FOV used
	}


	public void applies(float delta)
	{
		// this function manage the differents camera modifications relative to a specific state, except for the FOV
		float newPosY = 0.0f;
		float newRotZ = 0.0f;
		switch (playerChar.currentState)
		{
			case PlayerCharacter.State.IDLE:
				newPosY = Mathf.Lerp(Position.Y, 0.715f, crouchCameraLerpSpeed * delta);
				newRotZ = Mathf.Lerp(Rotation.Z, Mathf.DegToRad(0.0f), slideCameraLerpSpeed * delta);
				break;
			case PlayerCharacter.State.WALK:
				newPosY = Mathf.Lerp(Position.Y, 0.715f, crouchCameraLerpSpeed * delta);
				newRotZ = Mathf.Lerp(Rotation.Z, Mathf.DegToRad(0.0f), slideCameraLerpSpeed * delta);
				break;
			case PlayerCharacter.State.RUN:
				newPosY = Mathf.Lerp(Position.Y, 0.715f, crouchCameraLerpSpeed * delta);
				newRotZ = Mathf.Lerp(Rotation.Z, Mathf.DegToRad(0.0f), slideCameraLerpSpeed * delta);
				break;
			case PlayerCharacter.State.CROUCH:
				// lean the camera
				newPosY = Mathf.Lerp(Position.Y, 0.715f + crouchCameraDepth, crouchCameraLerpSpeed * delta);
				newRotZ = Mathf.Lerp(Rotation.Z, Mathf.DegToRad(6.0f) * (!Mathf.IsEqualApprox(playCharInputDir.X, 0.0f) ? playCharInputDir.X : Mathf.DegToRad(6.0f)), slideCameraLerpSpeed * delta);
				break;
			case PlayerCharacter.State.SLIDE:
				// lean the camera a bit more
				newPosY = Mathf.Lerp(Position.Y, 0.715f + slideCameraDepth, crouchCameraLerpSpeed * delta);
				newRotZ = Mathf.Lerp(Rotation.Z, Mathf.DegToRad(10.0f) * (!Mathf.IsEqualApprox(playCharInputDir.X, 0.0f) ? playCharInputDir.X : Mathf.DegToRad(10.0f)), slideCameraLerpSpeed * delta);
				break;
		}

		Position = new Vector3(Position.X, newPosY, Position.Z);
		Rotation = new Vector3(Rotation.X, Rotation.Y, newRotZ);
	}

	public void cameraBob(float delta)
	{
		// this function manage the bobbing of the camera when the character is moving
		if (playerChar.currentState != PlayerCharacter.State.SLIDE && playerChar.currentState != PlayerCharacter.State.DASH)
		{
			// the bobbing doesn't apply when the character is sliding or is dashing
			headBobValue += delta * playerChar.Velocity.Length() * (playerChar.IsOnFloor() ? 1.0f : 0.0f);
	
			Transform3D transform = new Transform3D();
			transform.Origin = headbob(headBobValue);
			camera.Transform = transform; // apply the bob effect obtained to the camera
		}
	}

	public Vector3 headbob(float time)
	{
		// some trigonometry stuff here, basically it uses the cosinus and sinus functions (sinusoidal function) to get a nice and smooth bob effect
		return new Vector3(Mathf.Cos(time * bobFrequency / 2) * bobAmplitude, Mathf.Sin(time * bobFrequency) * bobAmplitude, 0.0f);;
	}

	public void cameraTilt(float delta)
	{
		// this function manage the camera tilting when the character is moving on the x axis (left and right)
		if (playerChar.moveDirection != Vector3.Zero
				&& playerChar.currentState != PlayerCharacter.State.CROUCH
				&& playerChar.currentState != PlayerCharacter.State.SLIDE)
		{
			// the camera tilting doesn't apply when the character is not moving, or is crouching or walking  
			playCharInputDir = playerChar.inputDirection; // get input direction to know where the character is heading to
			// apply smooth tilt movement
			if (!playerChar.IsOnFloor())
				Rotation = new Vector3(Rotation.X, Rotation.Y, Mathf.Lerp(Rotation.Z, -playCharInputDir.X * camTiltRotationValue / 1.6f, camTiltRotationSpeed * delta));
			else
				Rotation = new Vector3(Rotation.X, Rotation.Y, Mathf.Lerp(Rotation.Z, -playCharInputDir.X * camTiltRotationValue, camTiltRotationSpeed * delta));
		}
	}
	public void FOVChange(float delta)
	{
		// FOV addon used to keep a logic FOV (for example, FOV when the character jumps right after running should be a bit higher than when he jumps right after walking)
		if (Mathf.IsEqualApprox(lastFOV, baseFOV))
			addonFOV = 0f;
		if (Mathf.IsEqualApprox(lastFOV, runFOV))
			addonFOV = 10f;
		if (Mathf.IsEqualApprox(lastFOV, slideFOV))
			addonFOV = 30f;

		// get the corresponding FOV to the current state the character is
		switch (playerChar.currentState)
		{
			case PlayerCharacter.State.IDLE:
				targetFOV = baseFOV;
				break;
			case PlayerCharacter.State.CROUCH:
				targetFOV = crouchFOV;
				break;
			case PlayerCharacter.State.WALK:
				targetFOV = baseFOV;
				break;
			case PlayerCharacter.State.RUN:
				targetFOV = runFOV;
				break;
			case PlayerCharacter.State.SLIDE:
				targetFOV = slideFOV;
				break;
			case PlayerCharacter.State.DASH:
				targetFOV = dashFOV;
				break;
			case PlayerCharacter.State.JUMP:
				targetFOV = baseFOV + addonFOV;
				break;
			case PlayerCharacter.State.INAIR:
				targetFOV = baseFOV + addonFOV;
				break;
		}
		
		// smoothly apply the FOV
		if (playerChar.currentState == PlayerCharacter.State.DASH)
			camera.Fov = Mathf.Lerp(camera.Fov, targetFOV, fovChangeSpeedWhenDash * delta); // the dash state has it's own get-to-FOV speed, because the action is very quick and so the FOV change won't be seen with the regular get-to-FOV speed
		else
			camera.Fov = Mathf.Lerp(camera.Fov, targetFOV, fovChangeSpeed * delta);
	}
}
