using Godot;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

public partial class PlayerCharacter : CharacterBody3D
{
	public enum State
	{
		[Display(Name = "Idle")]
		IDLE,
		[Display(Name = "Walking")]
		WALK,
		[Display(Name = "Running")]
		RUN,
		[Display(Name = "Crouching")]
		CROUCH,
		[Display(Name = "Sliding")]
		SLIDE,
		[Display(Name = "Jumping")]
		JUMP,
		[Display(Name = "In air")]
		INAIR,
		[Display(Name = "On wall")]
		ONWALL,
		[Display(Name = "Dashing")]
		DASH,
		[Display(Name = "Grappling")]
		GRAPPLE
	}
	public State currentState { get; set; }

	[ExportGroup("move variables")]
	float moveSpeed;
	float desiredMoveSpeed;
	[Export]
	public Curve DesiredMoveSpeedCurve { get; set; }
	[Export]
	public float MaxSpeed { get; set; } = 80f;
	[Export]
	public float WalkSpeed { get; set; } = 11f;
	[Export]
	public float RunSpeed { get; set; } = 20f;
	[Export]
	public float CrouchSpeed { get; set; } = 7f;
	float slideSpeed;
	[Export]
	public float SlideSpeedAddon { get; set; } = 8f;
	[Export]
	public float DashSpeed { get; set; } = 130f;
	float moveAcceleration;
	[Export]
	public float WalkAcceleration { get; set; } = 10f;
	[Export]
	public float RunAcceleration { get; set; } = 8f;
	[Export]
	public float crouchAcceleration { get; set; } = 6f;
	float moveDecceleration;
	[Export]
	public float WalkDecceleration { get; set; } = 10f;
	[Export]
	public float RunDecceleration { get; set; } = 8f;
	[Export]
	public float CrouchDecceleration { get; set; } = 6f;
	[Export]
	public Curve InAirMoveSpeedCurve;

	[ExportGroup("movement variables")]
	public Vector2 inputDirection { get; set; }
	public Vector3 moveDirection { get; set; }
	/// <summary>
	/// Amount of time the character keeps its accumulated speed before losing it (while being on ground)
	/// </summary>
	[Export]
	public double HitGroundCooldown { get; set; } = 0.2;
	double hitGroundCooldownRef;
	Vector3 lastFramePosition;
	Vector3 floorAngle; // angle of the floor the character is on 
	float slopeAngle; // angle of the slope the character is on
	bool canInput;
	float collisionInfo;
	bool wasOnFloor;

	[ExportGroup("jump variables")]
	[Export]
	public float JumpHeight { get; set; } = 4f;
	[Export]
	public float JumpTimeToPeak { get; set; } = 0.4f;
	[Export]
	public float JumpTimeToFall { get; set; } = 0.35f;
	// @onready
	float jumpVelocity;
	[Export]
	public double JumpCooldown { get; set; } = 0.25;
	double jumpCooldownRef;
	[Export]
	public int JumpsInAirAllowed { get; set; } = 1;
	int nbJumpsInAirAllowedRef;
	bool canCoyoteJump { get; set; } = true;
	[Export]
	public double CoyoteJumpCooldown { get; set; } = 0.3;
	double coyoteJumpCooldownRef;
	bool coyoteJumpOn = false;
	bool jumpBuffOn = false;

	[ExportGroup("slide variables")]
	[Export]
	public double SlideTime { get; set; } = 1.0;
	[Export]
	public double SlideTimeRef { get; set; } = 0.0;
	Vector2 slideVector = Vector2.Zero; // slide direction
	bool startSlideInAir;
	[Export]
	public double TimeBeforeCanSlideAgain { get; set; } = 0.5;
	double timeBeforeCanSlideAgainRef;
	/// <summary>
	/// Max angle value where the slide time duration is applied
	/// </summary>
	[Export]
	public float MaxSlopeAngle { get; set; } = 10.0f;

	[ExportGroup("wall run variables")]
	[Export]
	public float WallJumpVelocity { get; set; } = 3f;
	bool canWallRun;

	[ExportGroup("dash variables")]
	[Export]
	public double DashTime { get; set; } = 0.11;
	double dashTimeRef;
	[Export]
	public int DashesAllowed { get; set; } = 3;
	int nbDashAllowedRef;
	[Export]
	public double TimeBeforeCanDashAgain { get; set; } = 0.2f;
	double timeBeforeCanDashAgainRef { get; set; } = 0.2f;
	[Export]
	public double TimeBefReloadDash { get; set; } = 0.6;
	double timeBefReloadDashRef;
	Vector3 velocityPreDash;

	[ExportGroup("grapple hook variables")]
	List<string> grapHookType = new List<string>{ "Pull", "Swing" };
	[Export]
	public float GrapHookMaxDist { get; set; } = 800f;
	[Export]
	public float grapHookSpeed { get; set; } = 80f;
	[Export]
	public float GrapHookAccel { get; set; } = 6f;
	Vector3 anchorPoint;
	[Export]
	public float DistToStopGrappleOnFloor { get; set; } = 10f;
	[Export]
	public float DistToStopGrappleIAir { get; set; } = 5f;
	[Export]
	public double TimeBeforeCanGrappleAgain { get; set; } = 0.5;
	double timeBeforeCanGrappleAgainRef;
	[Export]
	public float GrappleLaunchJumpVelocity { get; set; } = 8f;
	/// <summary>
	/// enable if the character can jump while grappling downhill
	/// </summary>
	[Export]
	public bool DownDirJump { get; set; } = true;

	// knockback variables
	[ExportGroup("Knockback variables")]
	[Export]
	public float OnFloorKnockbackDivider { get; set; } = 3.5f;

	// gravity variables
	[ExportGroup("gravity variables")]
	// @onready
	float jumpGravity;
	// @onready
	float fallGravity;
	[Export]
	public float WallGravityMultiplier { get; set; } = 0.7f;

	// references variables
	// @onready
	CameraObject cameraHolder;
	// @onready
	CollisionShape3D standHitbox;
	// @onready 
	CollisionShape3D crouchHitbox;
	// @onready
	RayCast3D ceilingCheck;
	// @onready
	RayCast3D floorCheck;
	// @onready
	RayCast3D grappleHookCheck;
	// @onready
	Node3D grapHookRope;
	//@onready
	MeshInstance3D mesh;
	// @onready
	HUD hud;
	// @onready
	PauseMenu pauseMenu;

	public override void _Ready()
	{
		// onready vars
		jumpVelocity = 2.0f * JumpHeight / JumpTimeToPeak;
		jumpGravity = -2.0f * JumpHeight / (JumpTimeToPeak * JumpTimeToPeak);
		fallGravity = -2.0f * JumpHeight / (JumpTimeToFall * JumpTimeToFall);

		cameraHolder = GetNode<CameraObject>("CameraHolder");
		standHitbox = GetNode<CollisionShape3D>("standingHitbox");
		crouchHitbox = GetNode<CollisionShape3D>("crouchingHitbox");

		ceilingCheck = GetNode<RayCast3D>("Raycasts/CeilingCheck");
		floorCheck = GetNode<RayCast3D>("Raycasts/FloorCheck");
		grappleHookCheck = GetNode<RayCast3D>("CameraHolder/Camera3D/GrappleHookCheck");

		grapHookRope = GetNode<Node3D>("CameraHolder/Camera3D/GrappleHookRope");
		mesh = GetNode<MeshInstance3D>("MeshInstance3D");
		hud = GetNode<HUD>("HUD");
		pauseMenu = GetNode<PauseMenu>("PauseMenu");

		// set the start move speed
		moveSpeed = WalkSpeed;
		moveAcceleration = WalkAcceleration;
		moveDecceleration = WalkDecceleration;

		// set the values refenrencials for the needed variables
		desiredMoveSpeed = moveSpeed;
		jumpCooldownRef = JumpCooldown;
		nbJumpsInAirAllowedRef = JumpsInAirAllowed;
		hitGroundCooldownRef = HitGroundCooldown;
		coyoteJumpCooldownRef = CoyoteJumpCooldown;
		SlideTimeRef = SlideTime;
		dashTimeRef = DashTime;
		nbDashAllowedRef = DashesAllowed;
		timeBeforeCanSlideAgainRef = TimeBeforeCanSlideAgain;
		timeBeforeCanDashAgainRef = TimeBeforeCanDashAgain;
		timeBefReloadDashRef = TimeBefReloadDash;
		timeBeforeCanGrappleAgainRef = TimeBeforeCanGrappleAgain;
		canWallRun = false;
		canInput = true;

		// disable the crouch hitbox, enable is standing one
		if (!crouchHitbox.Disabled)
			crouchHitbox.Disabled = true;
		if (standHitbox.Disabled)
			standHitbox.Disabled = false;

		// set the raycasts
		if (!ceilingCheck.Enabled)
			ceilingCheck.Enabled = true;
		if (!floorCheck.Enabled)
			floorCheck.Enabled = true;
		if (!grappleHookCheck.Enabled)
			grappleHookCheck.Enabled = true;

		grappleHookCheck.TargetPosition = new Vector3(-GrapHookMaxDist, 0.0f, 0.0f); // -grapHookMaxDist pour être bien dans la direction du joueur
		if (grapHookRope.Visible)
			grapHookRope.Visible = false;

		// set the mesh scale of the character
		mesh.Scale = new Vector3(1.0f, 1.0f, 1.0f);
	}

	public override void _Process(double delta)
	{
		// the behaviours that is preferable to check every "visual" frame
		if (!pauseMenu.PauseMenuEnabled)
		{
			InputManagement();
			DisplayStats();
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		// the behaviours that is preferable to check every "physics" frame
		Applies(delta);
		Move(delta);
		GrappleHookManagement(delta);
		CollisionHandling();
		MoveAndSlide();
	}

	public void InputManagement()
	{
		// for each state, check the possibles actions available
		// This allow to have a good control of the controller behaviour, because you can easely check the actions possibls, 
		// add or remove some, and it prevent certain actions from being played when they shouldn't be

		if (canInput)
		{
			switch (currentState)
			{
				case State.IDLE:
					if (Input.IsActionJustPressed("jump"))
					{
						Jump(0.0f, false);
						JumpBuffering();
					}
					if (Input.IsActionJustPressed("crouch | slide"))
						CrouchStateChanges();
					if (Input.IsActionJustPressed("grappleHook"))
						GrappleStateChanges();
					break;
				case State.WALK:
					if (Input.IsActionJustPressed("run"))
						RunStateChanges();
					if (Input.IsActionJustPressed("jump"))
					{
						Jump(0.0f, false);
						JumpBuffering();
					}
					if (Input.IsActionJustPressed("crouch | slide"))
						CrouchStateChanges();
					if (Input.IsActionJustPressed("dash"))
						DashStateChanges();
					if (Input.IsActionJustPressed("grappleHook"))
						GrappleStateChanges();
					break;
				case State.RUN:
					if (Input.IsActionJustPressed("run"))
						WalkStateChanges();
					if (Input.IsActionJustPressed("jump"))
					{
						Jump(0.0f, false);
						JumpBuffering();
					}
					if (Input.IsActionJustPressed("crouch | slide"))
						SlideStateChanges();
					if (Input.IsActionJustPressed("dash"))
						DashStateChanges();
					if (Input.IsActionJustPressed("grappleHook"))
						GrappleStateChanges();
					break;
				case State.CROUCH:
					if (Input.IsActionJustPressed("run") && !ceilingCheck.IsColliding())
						WalkStateChanges();
					if (Input.IsActionJustPressed("crouch | slide") && !ceilingCheck.IsColliding())
						WalkStateChanges();
					break;
				case State.SLIDE:
					if (Input.IsActionJustPressed("run"))
						SlideStateChanges();
					if (Input.IsActionJustPressed("jump"))
					{
						Jump(0.0f, false);
						JumpBuffering();
					}
					if (Input.IsActionJustPressed("crouch | slide"))
						SlideStateChanges();
					break;
				case State.JUMP:
					if (Input.IsActionJustPressed("crouch | slide"))
						SlideStateChanges();
					if (Input.IsActionJustPressed("dash"))
						DashStateChanges();
					if (Input.IsActionJustPressed("jump"))
					{
						Jump(0.0f, false);
						JumpBuffering();
					}
					if (Input.IsActionJustPressed("grappleHook"))
						GrappleStateChanges();
					break;
				case State.INAIR:
					if (Input.IsActionJustPressed("crouch | slide"))
						SlideStateChanges();
					if (Input.IsActionJustPressed("dash"))
						DashStateChanges();
					if (Input.IsActionJustPressed("jump"))
					{
						Jump(0.0f, false);
						JumpBuffering();
					}
					if (Input.IsActionJustPressed("grappleHook"))
						GrappleStateChanges();
					break;
				case State.ONWALL:
					if (Input.IsActionJustPressed("jump"))
						Jump(0.0f, false);
					break;
				case State.DASH:
					break;
				case State.GRAPPLE:
					if (Input.IsActionJustPressed("jump"))
						Jump(grapHookSpeed / 3, true);
					if (Input.IsActionJustPressed("grappleHook"))
						GrappleStateChanges();
					break;
			}
		}
	}
	void DisplayStats()
	{
		// call the functions in charge of displaying the controller properties
		hud.DisplayCurrentState(currentState);
		hud.DisplayMoveSpeed(moveSpeed);
		hud.DisplayDesiredMoveSpeed(desiredMoveSpeed);
		hud.DisplayVelocity(Velocity.Length());
		hud.DisplayNbDashsAllowed(DashesAllowed);
		hud.DisplaySlideWaitTime(TimeBeforeCanSlideAgain);
		hud.DisplayDashWaitTime(TimeBeforeCanDashAgain);
		hud.DisplayNbJumpsAllowedInAir(JumpsInAirAllowed);
		hud.DisplayGrappleHookToolWaitTime(TimeBeforeCanGrappleAgain);

		// not a property, but a visual
		if (currentState == State.DASH)
			hud.DisplaySpeedLinesAsync(DashTime);
	}

	void Applies(double delta)
	{
		// general appliements
		floorAngle = GetFloorNormal(); // get the angle of the floor

		if (!IsOnFloor())
		{
			// modify the type of gravity to apply to the character, depending of his velocity (when jumping jump gravity, otherwise fall gravity)
			if (Velocity.Y >= 0.0)
			{
				if (currentState != State.GRAPPLE)
					Velocity = new Vector3 (Velocity.X, (float)(Velocity.Y + jumpGravity * delta), Velocity.Z);
				if (currentState != State.SLIDE && currentState != State.DASH && currentState != State.GRAPPLE)
					currentState = State.JUMP;
			}
			else
			{
				if (currentState != State.GRAPPLE)
					Velocity = new Vector3 (Velocity.X, (float)(Velocity.Y + fallGravity * delta), Velocity.Z);
				if (currentState != State.SLIDE && currentState != State.DASH && currentState != State.GRAPPLE)
					currentState = State.INAIR;
			}

			if (currentState == State.SLIDE)
				if (!startSlideInAir)
					SlideTime = -1; // if the character start slide on the grund, and the jump, the slide is canceled


			if (currentState == State.DASH)
				Velocity = new Vector3(Velocity.X, 0.0f, Velocity.Z); // set the y axis velocity to 0, to allow the character to not be affected by gravity while dashing
			if (HitGroundCooldown != hitGroundCooldownRef)
				HitGroundCooldown = hitGroundCooldownRef; // reset the before bunny hopping value
			if (CoyoteJumpCooldown > 0.0)
				CoyoteJumpCooldown -= delta;
		}
		if (IsOnFloor())
		{
			slopeAngle = Mathf.RadToDeg(Mathf.Acos(floorAngle.Dot(Vector3.Up))); // get the angle of the slope 

			if (currentState == State.SLIDE && startSlideInAir)
				startSlideInAir = false;

			if (jumpBuffOn)
			{
				jumpBuffOn = false;
				Jump(0.0f, false);
			}

			if (HitGroundCooldown >= 0)
				HitGroundCooldown -= delta; // disincremente the value each frame, when it's <= 0, the player lose the speed he accumulated while being in the air 


			if (JumpsInAirAllowed != nbJumpsInAirAllowedRef)
				JumpsInAirAllowed = nbJumpsInAirAllowedRef; // set the number of jumps possible


			if (CoyoteJumpCooldown != coyoteJumpCooldownRef)
				CoyoteJumpCooldown = coyoteJumpCooldownRef;

			// set the move state depending on the move speed, only when the character is moving

			// not the best piece of code i made, but i didn't really saw a more efficient way
			if (inputDirection != Vector2.Zero && moveDirection != Vector3.Zero)
			{
				if (Mathf.IsEqualApprox(moveSpeed, CrouchSpeed))
					currentState = State.CROUCH;
				else if (Mathf.IsEqualApprox(moveSpeed, WalkSpeed))
					currentState = State.WALK;
				else if (Mathf.IsEqualApprox(moveSpeed, RunSpeed))
					currentState = State.RUN;
				else if (Mathf.IsEqualApprox(moveSpeed, slideSpeed))
					currentState = State.SLIDE;
				else if (Mathf.IsEqualApprox(moveSpeed, DashSpeed))
					currentState = State.DASH;
				else if (Mathf.IsEqualApprox(moveSpeed, grapHookSpeed))
					moveSpeed = RunSpeed;
			}
			else
			{
				// set the state to idle
				if (currentState == State.JUMP
						|| currentState == State.INAIR
						|| currentState == State.WALK
						|| currentState == State.RUN)
				{
					if (Velocity.Length() < 1.0)
					{
						currentState = State.IDLE;
					}
				}
			}
		}

		// if the character is on a wall
		// set the state on onwall
		if (IsOnWall())
			WallrunStateChanges();


		if (IsOnFloor() || !IsOnFloor())
		{
			// manage the slide behaviour
			if (currentState == State.SLIDE)
			{
				// if character slide on an uphill, cancel slide

				// there is a bug here related to the uphill/downhill slide 
				// (simply said, i have to adjust manually the lastFramePosition value in order to make the character slide indefinitely downhill bot not uphill)
				// if you know how to resolve that issue, don't hesitate to make a post about it on the discussions tab of the project's Github repository

				if (!startSlideInAir && lastFramePosition.Y + 0.1 < Position.Y) // don't know why i need to add a +0.1 to lastFramePosition.y, otherwise it breaks the mechanic some times
					SlideTime = -1;


				if (!startSlideInAir && slopeAngle < MaxSlopeAngle)
					if (SlideTime > 0)
						SlideTime -= delta;


				if (SlideTime <= 0)
				{
					TimeBeforeCanSlideAgain = timeBeforeCanSlideAgainRef;
					// go to crouch state if the ceiling is too low, otherwise go to run state 
					if (ceilingCheck.IsColliding())
						CrouchStateChanges();
					else
						RunStateChanges();
				}
			}
			// manage the dash behaviour
			if (currentState == State.DASH)
			{
				if (canInput)
					canInput = false; // the character cannot change direction while dashing 

				if (DashTime > 0)
					DashTime -= delta;

				// the character cannot dash anymore, change to corresponding variables, and go back to run state
				if (DashTime <= 0)
				{
					Velocity = velocityPreDash; // go back to pre dash velocity
					canInput = true;
					TimeBeforeCanDashAgain = timeBeforeCanDashAgainRef;
					RunStateChanges();
				}
			}

			if (TimeBeforeCanSlideAgain > 0.0)
				TimeBeforeCanSlideAgain -= delta;


			if (TimeBeforeCanDashAgain > 0.0)
				TimeBeforeCanDashAgain -= delta;

			// manage the dash reloading
			if (TimeBefReloadDash > 0.0 && DashesAllowed != nbDashAllowedRef)
				TimeBefReloadDash -= delta;

			if (TimeBefReloadDash <= 0.0 && DashesAllowed != nbDashAllowedRef)
			{
				TimeBefReloadDash = timeBefReloadDashRef;
				DashesAllowed += 1;
			}

			if (TimeBeforeCanGrappleAgain > 0.0)
				TimeBeforeCanGrappleAgain -= delta;


			if (currentState == State.JUMP)
				FloorSnapLength = 0.0f; // the character cannot stick to structures while jumping


			if (currentState == State.INAIR)
				FloorSnapLength = 2.5f; // but he can if he stopped jumping, but he's still in the air


			if (JumpCooldown > 0.0)
				JumpCooldown -= delta;
		}
	}

	void Move(double delta)
	{
		// direction input
		inputDirection = Input.GetVector("moveLeft", "moveRight", "moveForward", "moveBackward");

		// get direction input when sliding
		if (currentState == State.SLIDE)
		{
			if (moveDirection == Vector3.Zero) // if the character is moving
				moveDirection = (cameraHolder.Basis * new Vector3(slideVector.X, 0.0f, slideVector.Y)).Normalized(); // get move direction at the start of the slide, and stick to it
		}
		// get direction input when wall running
		else if (currentState == State.ONWALL)
		{
			moveDirection = Velocity.Normalized(); // get character current velocity and apply it as the current move direction, and stick to it
		}
		// dash
		else if (currentState == State.DASH)
		{
			if (moveDirection == Vector3.Zero) // if the character is moving
				moveDirection = (cameraHolder.Basis * new Vector3(inputDirection.X, 0.0f, inputDirection.Y)).Normalized(); // get move direction at the start of the dash, and stick to it
		}
		// all others 
		else
		{
			// get the move direction depending on the input
			moveDirection = (cameraHolder.Basis * new Vector3(inputDirection.X, 0.0f, inputDirection.Y)).Normalized();
		}

		// move applies when the character is on the floor
		if (IsOnFloor())
		{
			// if the character is moving
			if (Vector3.Zero != moveDirection)
			{
				// apply slide move
				if (currentState == State.SLIDE)
				{
					if (slopeAngle > MaxSlopeAngle)
						desiredMoveSpeed += (float)(3.0 * delta); // increase more significantly desired move speed if the slope is steep enough
					else
						desiredMoveSpeed += (float)(2.0 * delta);

					Velocity = new Vector3(moveDirection.X * desiredMoveSpeed, Velocity.Y, moveDirection.Z * desiredMoveSpeed);
				}
				// apply dash move
				else if (currentState == State.DASH)
				{
					Velocity = new Vector3(moveDirection.X * DashSpeed, Velocity.Y, moveDirection.Z * DashSpeed);
				}
				// apply grapple hook desired move speed incrementation
				else if (currentState == State.GRAPPLE)
				{
					if (desiredMoveSpeed < MaxSpeed)
						desiredMoveSpeed += (float)(grapHookSpeed * delta);
				}
				// apply smooth move when walking, crouching, running
				else
				{
					Velocity = new Vector3(
						(float)Mathf.Lerp(Velocity.X, moveDirection.X * moveSpeed, moveAcceleration * delta),
						Velocity.Y,
						(float)Mathf.Lerp(Velocity.Z, moveDirection.Z * moveSpeed, moveAcceleration * delta)
					);

					// cancel desired move speed accumulation if the timer is out
					if (HitGroundCooldown <= 0)
						desiredMoveSpeed = Velocity.Length();
				}
			}
			// if the character is not moving
			else
			{
				// apply smooth stop 
				Velocity = new Vector3(
					(float)Mathf.Lerp(Velocity.X, 0.0, moveAcceleration * delta),
					Velocity.Y,
					(float)Mathf.Lerp(Velocity.Z, 0.0, moveAcceleration * delta)
				);

				// cancel desired move speed accumulation
				desiredMoveSpeed = Velocity.Length();
			}
		}
		// move applies when the character is not on the floor (so if he's in the air)
		if (!IsOnFloor())
		{
			if (moveDirection != Vector3.Zero)
			{
				// apply dash move
				if (currentState == State.DASH)
				{
					Velocity = new Vector3(moveDirection.X * DashSpeed, Velocity.Y, moveDirection.Z * DashSpeed);
				}
				// apply slide move
				else if (currentState == State.SLIDE)
				{
					desiredMoveSpeed += (float)(2.5 * delta);
					Velocity = new Vector3(moveDirection.X * desiredMoveSpeed, Velocity.Y, moveDirection.Z * desiredMoveSpeed);
				}
				// apply grapple hook desired move speed incrementation
				else if (currentState == State.GRAPPLE)
				{
					if (desiredMoveSpeed < MaxSpeed)
						desiredMoveSpeed += (float)(grapHookSpeed * delta);
				}
				// apply smooth move when in the air (air control)
				else
				{
					if (desiredMoveSpeed < MaxSpeed)
						desiredMoveSpeed += (float)(1.5 * delta);

					// here, set the air control amount depending on a custom curve, to select it with precision, depending on the desired move speed
					float contrdDesMoveSpeed = DesiredMoveSpeedCurve.Sample(desiredMoveSpeed / 100);
					float contrdInAirMoveSpeed = InAirMoveSpeedCurve.Sample(desiredMoveSpeed);

					Velocity = new Vector3(
						(float)Mathf.Lerp(Velocity.X, moveDirection.X * contrdDesMoveSpeed, contrdInAirMoveSpeed * delta),
						Velocity.Y,
						(float)Mathf.Lerp(Velocity.Z, moveDirection.Z * contrdDesMoveSpeed, contrdInAirMoveSpeed * delta)
					);
				}
			}
			else
			{
				// accumulate desired speed for bunny hopping
				desiredMoveSpeed = Velocity.Length();
			}
		}
		// move applies when the character is on the wall
		if (IsOnWall())
		{
			// apply on wall move
			if (currentState == State.ONWALL)
			{
				if (moveDirection != Vector3.Zero)
					desiredMoveSpeed += (float)(1.0 * delta);

				Velocity = new Vector3(moveDirection.X * desiredMoveSpeed, Velocity.Y, moveDirection.Z * desiredMoveSpeed);
			}
		}
		if (desiredMoveSpeed >= MaxSpeed)
			desiredMoveSpeed = MaxSpeed; // set to ensure the character don't exceed the max speed authorized

		lastFramePosition = Position;
		wasOnFloor = !IsOnFloor();
	}

	public void Jump(float jumpBoostValue, bool isJumpBoost)
	{
		// this function manage the jump behaviour, depending of the different variables and states the character is

		bool canJump = false; // jump condition

		// the jump can only be applied if the player character is pulled up
		if (currentState == State.GRAPPLE && lastFramePosition.Y > Position.Y && !DownDirJump)
		{
			if (!Mathf.IsEqualApprox(jumpBoostValue, 0.0f))
				jumpBoostValue = 0.0f;
			if (isJumpBoost)
				isJumpBoost = false;
			GrappleStateChanges();
			return;
		}
		// on wall jump 
		if (IsOnWall() && canWallRun)
		{
			currentState = State.JUMP;
			Velocity = GetWallNormal() * WallJumpVelocity; // add some knockback in the opposite direction of the wall
			Velocity = new Vector3(Velocity.X, jumpVelocity, Velocity.Z);
			JumpCooldown = jumpCooldownRef;
		}
		else
		{
			// in air jump
			if (!IsOnFloor())
			{
				if (JumpCooldown <= 0)
				{
					// determine if the character are in the conditions for enable coyote jump
					if (wasOnFloor && CoyoteJumpCooldown > 0.0 && lastFramePosition.Y > Position.Y)
						coyoteJumpOn = true;

					// if the character jump from a jumppad, the jump isn't taken into account in the max numbers of jumps allowed, allowing the character to continusly jump as long as it lands on a jumppad
					if (JumpsInAirAllowed > 0 || (JumpsInAirAllowed <= 0 && isJumpBoost) || coyoteJumpOn)
					{  // also, take into account if the character is coyote jumping
						if (!isJumpBoost && !coyoteJumpOn)
							JumpsInAirAllowed -= 1;

						JumpCooldown = jumpCooldownRef;
					}
					coyoteJumpOn = false;
					canJump = true;
				}
			}
			// on floor jump
			else
			{
				JumpCooldown = jumpCooldownRef;
				canJump = true;
			}
		}
		// apply jump
		if (canJump)
		{
			if (isJumpBoost)
			 	JumpsInAirAllowed = nbJumpsInAirAllowedRef;
			currentState = State.JUMP;
			Velocity = new Vector3(Velocity.X, jumpVelocity + jumpBoostValue, Velocity.Z); // apply directly jump velocity to y axis velocity, to give the character instant vertical forcez
			canJump = false;
		}
	}

	void JumpBuffering()
	{
		// if the character is falling, and the floor check raycast is colliding and the jump properties are good, enable jump buffering
		if (floorCheck.IsColliding()
				&& lastFramePosition.Y > Position.Y
				&& JumpsInAirAllowed <= 0
				&& JumpCooldown <= 0.0)
			jumpBuffOn = true;
	}

	void GrappleHookManagement(double delta)
	{
		float distToAnchorPoint = 0.0f; // distance entre le personnae et le point d'ancrage du grappin
		GrappleHookMove(delta, distToAnchorPoint);
		GrappleHookRopeManagement(distToAnchorPoint);
	}

	void GrappleHookMove(double delta, float distToAnchorPoint)
	{
		if (currentState == State.GRAPPLE)
		{
			moveDirection = GlobalPosition.DirectionTo(anchorPoint); // direction to move on
			distToAnchorPoint = GlobalPosition.DistanceTo(anchorPoint); // distance from anchor point to character
			if (moveDirection != Vector3.Zero)
			{
				// apply grapple hook move
				if (IsOnFloor())
				{
					if (distToAnchorPoint > DistToStopGrappleIAir)
						Velocity = Velocity.Lerp(moveDirection * grapHookSpeed, (float)(GrapHookAccel * delta));
					else
						GrappleStateChanges();
				}
				if (!IsOnFloor())
				{
					if (distToAnchorPoint > DistToStopGrappleOnFloor)
						Velocity = Velocity.Lerp(moveDirection * grapHookSpeed, (float)(GrapHookAccel * delta));
					else
						GrappleStateChanges();
				}
			}
		
		}
	}

	void GrappleHookRopeManagement(float distToAnchorPoint)
	{
		// hide the rope
		if (currentState != State.GRAPPLE)
		{
			if (grapHookRope.Visible)
				grapHookRope.Visible = false;
			return;
		}
		else
		{
			// show the rope at the corresponding point and direction
			if (!grapHookRope.Visible)
				grapHookRope.Visible = true;

			grapHookRope.LookAt(anchorPoint);
			distToAnchorPoint = GlobalPosition.DistanceTo(anchorPoint);
			grapHookRope.Scale = new Vector3(0.07f, 0.18f, distToAnchorPoint); // change the scale to make the rope take all the direction width
		}
	}
			
	// theses functions manages the differents changes and appliments the character will go trought when changing his current state
	void CrouchStateChanges()
	{
		currentState = State.CROUCH;
		moveSpeed = CrouchSpeed;
		moveAcceleration = crouchAcceleration;
		moveDecceleration = CrouchDecceleration;

		standHitbox.Disabled = true;
		crouchHitbox.Disabled = false;

		if (!Mathf.IsEqualApprox(mesh.Scale.Y, 0.7))
		{
			mesh.Scale = new Vector3(mesh.Scale.X, 0.7f, mesh.Scale.Z);
			mesh.Position = new Vector3(mesh.Position.X, -0.5f, mesh.Position.Z);
		}
	}

	void WalkStateChanges()
	{
		currentState = State.WALK;
		moveSpeed = WalkSpeed;
		moveAcceleration = WalkAcceleration;
		moveDecceleration = WalkDecceleration;

		standHitbox.Disabled = false;
		crouchHitbox.Disabled = true;

		if (!Mathf.IsEqualApprox(mesh.Scale.Y, 1.0))
		{
			mesh.Scale = new Vector3(mesh.Scale.X, 1.0f, mesh.Scale.Z);
			mesh.Position = new Vector3(mesh.Position.X, 1.0f, mesh.Position.Z);
		}
	}

	void RunStateChanges()
	{
		currentState = State.RUN;
		moveSpeed = RunSpeed;
		moveAcceleration = RunAcceleration;
		moveDecceleration = RunDecceleration;

		standHitbox.Disabled = false;
		crouchHitbox.Disabled = true;

		if (!Mathf.IsEqualApprox(mesh.Scale.Y, 1.0))
		{
			mesh.Scale = new Vector3(mesh.Scale.X, 1.0f, mesh.Scale.Z);
			mesh.Position = new Vector3(mesh.Position.X, 1.0f, mesh.Position.Z);
		}
	}

	void SlideStateChanges()
	{
		// condition here, the state is changed only if the character is moving (so has an input direction)
		if (TimeBeforeCanSlideAgain <= 0 && currentState != State.SLIDE)
		{
			currentState = State.SLIDE;

			// change the start slide in air variable depending zon where the slide begun
			if (!IsOnFloor() && SlideTime <= 0)
			{
				startSlideInAir = true;
			}
			else if (IsOnFloor() && lastFramePosition.Y >= Position.Y)
			{
				// character can slide only on flat or downhill surfaces: 
				desiredMoveSpeed += SlideSpeedAddon; // slide speed boost when on ground (for balance purpose)
				startSlideInAir = false;
			}

			SlideTime = SlideTimeRef;
			moveSpeed = slideSpeed;
			if (inputDirection != Vector2.Zero)
				slideVector = inputDirection;
			else
				slideVector = new Vector2(0, -1);

			standHitbox.Disabled = true;
			crouchHitbox.Disabled = false;

			if (!Mathf.IsEqualApprox(mesh.Scale.Y, 0.7))
			{
				mesh.Scale = new Vector3(mesh.Scale.X, 0.7f, mesh.Scale.Z);
				mesh.Position = new Vector3(mesh.Position.X, -0.5f, mesh.Position.Z);
			}
		}
		else if (currentState == State.SLIDE)
		{
			SlideTime = -1.0;
			TimeBeforeCanSlideAgain = timeBeforeCanSlideAgainRef;
			if (ceilingCheck.IsColliding())
				CrouchStateChanges();
			else
				RunStateChanges();
		}
	}

	void DashStateChanges()
	{
		// condition here, the state is changed only if the character is moving (so has an input direction)
		if (inputDirection != Vector2.Zero && TimeBeforeCanDashAgain <= 0.0 && DashesAllowed > 0)
		{
			currentState = State.DASH;
			DashesAllowed -= 1;
			moveSpeed = DashSpeed;
			DashTime = dashTimeRef;
			velocityPreDash = Velocity; // save the pre dash velocity, to apply it when the dash is finished (to get back to a normal velocity)

			if (!Mathf.IsEqualApprox(mesh.Scale.Y, 1.0))
			{
				mesh.Scale = new Vector3(mesh.Scale.X, 1.0f, mesh.Scale.Z);
				mesh.Position = new Vector3(mesh.Position.X, 1.0f, mesh.Position.Z);
			}
		}
	}

	void WallrunStateChanges()
	{
		// condition here, the state is changed only if the character speed is greater than the walk speed
		if (Velocity.Length() > WalkSpeed && currentState != State.DASH && currentState != State.CROUCH && canWallRun)
		{
			currentState = State.ONWALL;
			Velocity = new Vector3(Velocity.X, Velocity.Y * WallGravityMultiplier, Velocity.Z); // gravity value became onwall one


			if (JumpsInAirAllowed != nbJumpsInAirAllowedRef)
				JumpsInAirAllowed = nbJumpsInAirAllowedRef;

			standHitbox.Disabled = false;
			crouchHitbox.Disabled = true;

			if (!Mathf.IsEqualApprox(mesh.Scale.Y, 1.0))
			{
				mesh.Scale = new Vector3(mesh.Scale.X, 1.0f, mesh.Scale.Z);
				mesh.Position = new Vector3(mesh.Position.X, 1.0f, mesh.Position.Z);
			}
		}
	}

	void GrappleStateChanges()
	{
		// condition here, the state is changed only if the character isn't already grappling, and the grapple check is colliding
		if (grappleHookCheck.IsColliding() && TimeBeforeCanGrappleAgain <= 0.0 && currentState != State.GRAPPLE)
		{
			currentState = State.GRAPPLE;

			if (IsOnFloor())
				Velocity = new Vector3(Velocity.X, GrappleLaunchJumpVelocity, Velocity.Z);

			TimeBeforeCanGrappleAgain = timeBeforeCanGrappleAgainRef;
			if (JumpsInAirAllowed < nbJumpsInAirAllowedRef)
				JumpsInAirAllowed = nbJumpsInAirAllowedRef;

			moveSpeed = grapHookSpeed;

			// get the collision point of the grapple hook raycast check
			anchorPoint = grappleHookCheck.GetCollisionPoint();

			standHitbox.Disabled = false;
			crouchHitbox.Disabled = true;

			if (!Mathf.IsEqualApprox(mesh.Scale.Y, 1.0))
			{
				mesh.Scale = new Vector3(mesh.Scale.X, 1.0f, mesh.Scale.Z);
				mesh.Position = new Vector3(mesh.Position.X, 1.0f, mesh.Position.Z);
			}
		}
		// the character is already grappling, so cut grapple state, and change to the one corresponding to his velocity
		else if (currentState == State.GRAPPLE)
		{
			if (!IsOnFloor())
			{
				if (Velocity.Y >= 0.0)
					currentState = State.JUMP;
				else
					currentState = State.INAIR;
			}
		}
	}

	void CollisionHandling()
	{
		// this function handle the collisions, but in this case, only the collision with a wall, to detect if the character can wallrun
		if (IsOnWall())
		{
			KinematicCollision3D lastCollision = GetSlideCollision(0);

			if (lastCollision != null)
			{
				CollisionObject3D collidedBody = lastCollision.GetCollider() as CollisionObject3D;
				if (collidedBody != null)
				{
					uint layer = collidedBody.CollisionLayer;

					// here, we check the layer of the collider, then we check if the layer 3 (walkableWall) is enabled, with 1 << 3-1. If theses two points are valid, the character can wallrun
					if ((layer & (1 << 3 - 1)) != 0)
						canWallRun = true;
					else
						canWallRun = false;
				}
				else
				{
					CsgShape3D csgShape = lastCollision.GetCollider() as CsgShape3D;
					if (csgShape != null)
					{
						uint layer = csgShape.CollisionLayer;

						if ((layer & (1 << 3 - 1)) != 0)
							canWallRun = true;
						else
							canWallRun = false;
					}
				}
			}
		}
	}

	// this function handle the knockback mechanic
	void OnObjectToolSendKnockback(float knockbackAmount, Vector3 knockbackOrientation)
	{
		Vector3 knockbackForce = -knockbackOrientation * knockbackAmount; // opposite of the knockback tool orientation, times knockback amount
		Velocity += !IsOnFloor() ? knockbackForce : knockbackForce / OnFloorKnockbackDivider;
	}
}
