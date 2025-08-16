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
	public float MaxUpAngleView { get; set; } = -90f;
	[Export]
	public float MaxDownAngleView { get; set; } = 90f;

	//  movement changes variables
	[ExportGroup("movement changes variables")]
	[Export]
	public float CrouchCameraDepth { get; set; } = -0.2f;
	[Export]
	public float CrouchCameraLerpSpeed { get; set; } = 8f;
	[Export]
	public float SlideCameraDepth { get; set; } = -0.5f;
	[Export]
	public float SlideCameraLerpSpeed { get; set; } = 8f;

	//  fov variables
	[ExportGroup("fov variables")]
	float targetFOV;
	float lastFOV;
	float addonFOV;
	[Export]
	public float BaseFOV { get; set; } = 90f;
	[Export]
	public float CrouchFOV { get; set; } = 75f;
	[Export]
	public float RunFOV { get; set; } = 100f;
	[Export]
	public float SlideFOV { get; set; } = 120f;
	[Export]
	public float DashFOV { get; set; } = 150f;
	[Export]
	public float FovChangeSpeed { get; set; } = 4f;
	[Export]
	public float FovChangeSpeedWhenDash { get; set; } = 3f;

	//  bob variables
	[ExportGroup("bob variables")]
	[Export]
	public float HeadBobValue { get; set; }
	[Export]
	public float BobFrequency { get; set; } = 0.8f;
	[Export]
	public float BobAmplitude { get; set; } = 0.06f;

	//  tilt variables
	[ExportGroup("tilt variables")]
	[Export]
	public float CamTiltRotationValue { get; set; } = 0.35f;
	[Export]
	public float CamTiltRotationSpeed { get; set; } = 2.2f;

	//  shake variables
	[ExportGroup("camera shake variables")]
	float shakeForce;
	[Export]
	public float ShakeDuration { get; set; } = 0.35f;
	float shakeDurationRef;
	[Export]
	public float ShakeFade { get; set; } = 6f;
	RandomNumberGenerator rng = new RandomNumberGenerator();
	bool canCameraShake = false;

	// input variables
	[ExportGroup("input variables")]
	Vector2 mouseInput;
	[Export]
	public float MouseInputSpeed { get; set; } = 2f;
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

		lastFOV = BaseFOV;
		shakeDurationRef = ShakeDuration;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		Applies((float)delta);
		CameraBob((float)delta);
		CameraTilt((float)delta);
		FOVChange((float)delta);
		lastFOV = targetFOV; // get the last FOV used
	}


	public void Applies(float delta)
	{
		// this function manage the differents camera modifications relative to a specific state, except for the FOV
		float newPosY = 0.0f;
		float newRotZ = 0.0f;
		switch (playerChar.currentState)
		{
			case PlayerCharacter.State.IDLE:
				newPosY = Mathf.Lerp(Position.Y, 0.715f, CrouchCameraLerpSpeed * delta);
				newRotZ = Mathf.Lerp(Rotation.Z, Mathf.DegToRad(0.0f), SlideCameraLerpSpeed * delta);
				break;
			case PlayerCharacter.State.WALK:
				newPosY = Mathf.Lerp(Position.Y, 0.715f, CrouchCameraLerpSpeed * delta);
				newRotZ = Mathf.Lerp(Rotation.Z, Mathf.DegToRad(0.0f), SlideCameraLerpSpeed * delta);
				break;
			case PlayerCharacter.State.RUN:
				newPosY = Mathf.Lerp(Position.Y, 0.715f, CrouchCameraLerpSpeed * delta);
				newRotZ = Mathf.Lerp(Rotation.Z, Mathf.DegToRad(0.0f), SlideCameraLerpSpeed * delta);
				break;
			case PlayerCharacter.State.CROUCH:
				// lean the camera
				newPosY = Mathf.Lerp(Position.Y, 0.715f + CrouchCameraDepth, CrouchCameraLerpSpeed * delta);
				newRotZ = Mathf.Lerp(Rotation.Z, Mathf.DegToRad(6.0f) * (!Mathf.IsEqualApprox(playCharInputDir.X, 0.0f) ? playCharInputDir.X : Mathf.DegToRad(6.0f)), SlideCameraLerpSpeed * delta);
				break;
			case PlayerCharacter.State.SLIDE:
				// lean the camera a bit more
				newPosY = Mathf.Lerp(Position.Y, 0.715f + SlideCameraDepth, CrouchCameraLerpSpeed * delta);
				newRotZ = Mathf.Lerp(Rotation.Z, Mathf.DegToRad(10.0f) * (!Mathf.IsEqualApprox(playCharInputDir.X, 0.0f) ? playCharInputDir.X : Mathf.DegToRad(10.0f)), SlideCameraLerpSpeed * delta);
				break;
		}

		Position = new Vector3(Position.X, newPosY, Position.Z);
		Rotation = new Vector3(Rotation.X, Rotation.Y, newRotZ);
	}

	public void CameraBob(float delta)
	{
		// this function manage the bobbing of the camera when the character is moving
		if (playerChar.currentState != PlayerCharacter.State.SLIDE && playerChar.currentState != PlayerCharacter.State.DASH)
		{
			// the bobbing doesn't apply when the character is sliding or is dashing
			HeadBobValue += delta * playerChar.Velocity.Length() * (playerChar.IsOnFloor() ? 1.0f : 0.0f);

			Transform3D transform = camera.Transform;
			transform.Origin = Headbob(HeadBobValue);
			camera.Transform = transform; // apply the bob effect obtained to the camera
		}
	}

	public Vector3 Headbob(float time)
	{
		// some trigonometry stuff here, basically it uses the cosinus and sinus functions (sinusoidal function) to get a nice and smooth bob effect
		float y = Mathf.Sin(time * BobFrequency) * BobAmplitude;
		float x = Mathf.Cos(time * BobFrequency / 2) * BobAmplitude;
		return new Vector3(x, y, 0.0f);
	}

	public void CameraTilt(float delta)
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
				Rotation = new Vector3(Rotation.X, Rotation.Y, Mathf.Lerp(Rotation.Z, -playCharInputDir.X * CamTiltRotationValue / 1.6f, CamTiltRotationSpeed * delta));
			else
				Rotation = new Vector3(Rotation.X, Rotation.Y, Mathf.Lerp(Rotation.Z, -playCharInputDir.X * CamTiltRotationValue, CamTiltRotationSpeed * delta));
		}
	}
	public void FOVChange(float delta)
	{
		// FOV addon used to keep a logic FOV (for example, FOV when the character jumps right after running should be a bit higher than when he jumps right after walking)
		if (Mathf.IsEqualApprox(lastFOV, BaseFOV))
			addonFOV = 0f;
		if (Mathf.IsEqualApprox(lastFOV, RunFOV))
			addonFOV = 10f;
		if (Mathf.IsEqualApprox(lastFOV, SlideFOV))
			addonFOV = 30f;

		// get the corresponding FOV to the current state the character is
		switch (playerChar.currentState)
		{
			case PlayerCharacter.State.IDLE:
				targetFOV = BaseFOV;
				break;
			case PlayerCharacter.State.CROUCH:
				targetFOV = CrouchFOV;
				break;
			case PlayerCharacter.State.WALK:
				targetFOV = BaseFOV;
				break;
			case PlayerCharacter.State.RUN:
				targetFOV = RunFOV;
				break;
			case PlayerCharacter.State.SLIDE:
				targetFOV = SlideFOV;
				break;
			case PlayerCharacter.State.DASH:
				targetFOV = DashFOV;
				break;
			case PlayerCharacter.State.JUMP:
				targetFOV = BaseFOV + addonFOV;
				break;
			case PlayerCharacter.State.INAIR:
				targetFOV = BaseFOV + addonFOV;
				break;
		}
		
		// smoothly apply the FOV
		if (playerChar.currentState == PlayerCharacter.State.DASH)
			camera.Fov = Mathf.Lerp(camera.Fov, targetFOV, FovChangeSpeedWhenDash * delta); // the dash state has it's own get-to-FOV speed, because the action is very quick and so the FOV change won't be seen with the regular get-to-FOV speed
		else
			camera.Fov = Mathf.Lerp(camera.Fov, targetFOV, FovChangeSpeed * delta);
	}
}
