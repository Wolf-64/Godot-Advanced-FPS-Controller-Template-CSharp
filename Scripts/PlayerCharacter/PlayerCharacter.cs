using Godot;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Reflection;


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
    private float _moveSpeed;
    private float _desiredMoveSpeed;
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
    private float _slideSpeed;
    [Export]
    public float SlideSpeedAddon { get; set; } = 8f;
    [Export]
    public float DashSpeed { get; set; } = 130f;
    private float _moveAcceleration;
    [Export]
    public float WalkAcceleration { get; set; } = 10f;
    [Export]
    public float RunAcceleration { get; set; } = 8f;
    [Export]
    public float crouchAcceleration { get; set; } = 6f;
    private float _moveDecceleration;
    [Export]
    public float WalkDecceleration { get; set; } = 10f;
    [Export]
    public float RunDecceleration { get; set; } = 8f;
    [Export]
    public float CrouchDecceleration { get; set; } = 6f;
    [Export]
    public Curve InAirMoveSpeedCurve { get; set; }

    [ExportGroup("movement variables")]
    public Vector2 InputDirection { get; set; }
    public Vector3 MoveDirection { get; set; }
    /// <summary>
    /// Amount of time the character keeps its accumulated speed before losing it (while being on ground)
    /// </summary>
    [Export]
    public double HitGroundCooldown { get; set; } = 0.2;
    private double _hitGroundCooldownRef;
    private Vector3 _lastFramePosition;
    private Vector3 _floorAngle; // angle of the floor the character is on 
    private float _slopeAngle; // angle of the slope the character is on
    private bool _canInput;
    private float _collisionInfo;
    private bool _wasOnFloor;

    [ExportGroup("jump variables")]
    [Export]
    public float JumpHeight { get; set; } = 4f;
    [Export]
    public float JumpTimeToPeak { get; set; } = 0.4f;
    [Export]
    public float JumpTimeToFall { get; set; } = 0.35f;
    // @onready
    private float _jumpVelocity;
    [Export]
    public double JumpCooldown { get; set; } = 0.25;
    private double _jumpCooldownRef;
    [Export]
    public int JumpsInAirAllowed { get; set; } = 1;
    private int _nbJumpsInAirAllowedRef;
    bool CanCoyoteJump { get; set; } = true;
    [Export]
    public double CoyoteJumpCooldown { get; set; } = 0.3;
    private double _coyoteJumpCooldownRef;
    private bool _coyoteJumpOn = false;
    private bool _jumpBuffOn = false;

    [ExportGroup("slide variables")]
    [Export]
    public double SlideTime { get; set; } = 1.0;
    [Export]
    public double SlideTimeRef { get; set; } = 0.0;
    private Vector2 _slideVector = Vector2.Zero; // slide direction
    private bool _startSlideInAir;
    [Export]
    public double TimeBeforeCanSlideAgain { get; set; } = 0.5;
    private double _timeBeforeCanSlideAgainRef;
    /// <summary>
    /// Max angle value where the slide time duration is applied
    /// </summary>
    [Export]
    public float MaxSlopeAngle { get; set; } = 10.0f;

    [ExportGroup("wall run variables")]
    [Export]
    public float WallJumpVelocity { get; set; } = 3f;
    private bool _canWallRun;

    [ExportGroup("dash variables")]
    [Export]
    public double DashTime { get; set; } = 0.11;
    private double _dashTimeRef;
    [Export]
    public int DashesAllowed { get; set; } = 3;
    private int _nbDashAllowedRef;
    [Export]
    public double TimeBeforeCanDashAgain { get; set; } = 0.2f;
    public double TimeBeforeCanDashAgainRef { get; set; } = 0.2f;
    [Export]
    public double TimeBefReloadDash { get; set; } = 0.6;
    private double _timeBefReloadDashRef;
    private Vector3 _velocityPreDash;

    [ExportGroup("grapple hook variables")]
    List<string> grapHookType = new List<string> { "Pull", "Swing" };
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
    private float _jumpGravity;
    // @onready
    private float _fallGravity;
    [Export]
    public float WallGravityMultiplier { get; set; } = 0.7f;

    // references variables
    // @onready
    private CameraObject _cameraHolder;
    // @onready
    private CollisionShape3D _standHitbox;
    // @onready 
    private CollisionShape3D _crouchHitbox;
    // @onready
    private RayCast3D _ceilingCheck;
    // @onready
    private RayCast3D _floorCheck;
    // @onready
    private RayCast3D _grappleHookCheck;
    // @onready
    private Node3D _grapHookRope;
    //@onready
    private MeshInstance3D _mesh;
    // @onready
    private HUD _hud;
    // @onready
    private PauseMenu _pauseMenu;

    public override void _Ready()
    {
        // onready vars
        _jumpVelocity = 2.0f * JumpHeight / JumpTimeToPeak;
        _jumpGravity = -2.0f * JumpHeight / (JumpTimeToPeak * JumpTimeToPeak);
        _fallGravity = -2.0f * JumpHeight / (JumpTimeToFall * JumpTimeToFall);

        _cameraHolder = GetNode<CameraObject>("CameraHolder");
        _standHitbox = GetNode<CollisionShape3D>("standingHitbox");
        _crouchHitbox = GetNode<CollisionShape3D>("crouchingHitbox");

        _ceilingCheck = GetNode<RayCast3D>("Raycasts/CeilingCheck");
        _floorCheck = GetNode<RayCast3D>("Raycasts/FloorCheck");
        _grappleHookCheck = GetNode<RayCast3D>("CameraHolder/Camera3D/GrappleHookCheck");

        _grapHookRope = GetNode<Node3D>("CameraHolder/Camera3D/GrappleHookRope");
        _mesh = GetNode<MeshInstance3D>("MeshInstance3D");
        _hud = GetNode<HUD>("HUD");
        _pauseMenu = GetNode<PauseMenu>("PauseMenu");

        // set the start move speed
        _moveSpeed = WalkSpeed;
        _moveAcceleration = WalkAcceleration;
        _moveDecceleration = WalkDecceleration;

        // set the values refenrencials for the needed variables
        _desiredMoveSpeed = _moveSpeed;
        _jumpCooldownRef = JumpCooldown;
        _nbJumpsInAirAllowedRef = JumpsInAirAllowed;
        _hitGroundCooldownRef = HitGroundCooldown;
        _coyoteJumpCooldownRef = CoyoteJumpCooldown;
        SlideTimeRef = SlideTime;
        _dashTimeRef = DashTime;
        _nbDashAllowedRef = DashesAllowed;
        _timeBeforeCanSlideAgainRef = TimeBeforeCanSlideAgain;
        TimeBeforeCanDashAgainRef = TimeBeforeCanDashAgain;
        _timeBefReloadDashRef = TimeBefReloadDash;
        timeBeforeCanGrappleAgainRef = TimeBeforeCanGrappleAgain;
        _canWallRun = false;
        _canInput = true;

        // disable the crouch hitbox, enable is standing one
        if (!_crouchHitbox.Disabled)
        {
            _crouchHitbox.Disabled = true;
        }
        if (_standHitbox.Disabled)
        {
            _standHitbox.Disabled = false;
        }

        // set the raycasts
        if (!_ceilingCheck.Enabled)
        {
            _ceilingCheck.Enabled = true;
        }
        if (!_floorCheck.Enabled)
        {
            _floorCheck.Enabled = true;
        }
        if (!_grappleHookCheck.Enabled)
        {
            _grappleHookCheck.Enabled = true;
        }

        // -grapHookMaxDist to be in the player's direction
            _grappleHookCheck.TargetPosition = new Vector3(-GrapHookMaxDist, 0.0f, 0.0f);
        if (_grapHookRope.Visible)
        {
            _grapHookRope.Visible = false;
        }
        
        // set the mesh scale of the character
        _mesh.Scale = new Vector3(1.0f, 1.0f, 1.0f);
    }

    public override void _Process(double delta)
    {
        // the behaviours that are preferable to check every "visual" frame
        if (!_pauseMenu.PauseMenuEnabled)
        {
            InputManagement();
            DisplayStats();
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        // the behaviours that are preferable to check every "physics" frame
        Applies(delta);
        Move(delta);
        GrappleHookManagement(delta);
        CollisionHandling();
        MoveAndSlide();
    }

    public void InputManagement()
    {
        // for each state, check the possibles actions available
        // This allow to have a good control of the controller behaviour, because you can easily check the actions possible, 
        // add or remove some, and it prevent certain actions from being played when they shouldn't be

        if (_canInput)
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
                    {
                        CrouchStateChanges();
                    }
                    if (Input.IsActionJustPressed("grappleHook"))
                    {
                        GrappleStateChanges();
                    }
                    break;
                case State.WALK:
                    if (Input.IsActionJustPressed("run"))
                    {
                        RunStateChanges();
                    }
                    if (Input.IsActionJustPressed("jump"))
                    {
                        Jump(0.0f, false);
                        JumpBuffering();
                    }
                    if (Input.IsActionJustPressed("crouch | slide"))
                    {
                        CrouchStateChanges();
                    }
                    if (Input.IsActionJustPressed("dash"))
                    {
                        DashStateChanges();
                    }
                    if (Input.IsActionJustPressed("grappleHook"))
                    {
                        GrappleStateChanges();
                    }
                    break;
                case State.RUN:
                    if (Input.IsActionJustPressed("run"))
                    {
                        WalkStateChanges();
                    }
                    if (Input.IsActionJustPressed("jump"))
                    {
                        Jump(0.0f, false);
                        JumpBuffering();
                    }
                    if (Input.IsActionJustPressed("crouch | slide"))
                    {
                        SlideStateChanges();
                    }
                    if (Input.IsActionJustPressed("dash"))
                    {
                            DashStateChanges();
                    }
                    if (Input.IsActionJustPressed("grappleHook"))
                    {
                        GrappleStateChanges();
                    }
                    break;
                case State.CROUCH:
                    if (Input.IsActionJustPressed("run") && !_ceilingCheck.IsColliding())
                    {
                        WalkStateChanges();
                    }
                    if (Input.IsActionJustPressed("crouch | slide") && !_ceilingCheck.IsColliding())
                    {
                        WalkStateChanges();
                    }
                    break;
                case State.SLIDE:
                    if (Input.IsActionJustPressed("run"))
                    {
                        SlideStateChanges();
                    }
                    if (Input.IsActionJustPressed("jump"))
                    {
                        Jump(0.0f, false);
                        JumpBuffering();
                    }
                    if (Input.IsActionJustPressed("crouch | slide"))
                    {
                        SlideStateChanges();
                    }
                    break;
                case State.JUMP:
                    if (Input.IsActionJustPressed("crouch | slide"))
                    {
                        SlideStateChanges();
                    }
                    if (Input.IsActionJustPressed("dash"))
                    {
                        DashStateChanges();
                    }
                    if (Input.IsActionJustPressed("jump"))
                    {
                        Jump(0.0f, false);
                        JumpBuffering();
                    }
                    if (Input.IsActionJustPressed("grappleHook"))
                    {
                        GrappleStateChanges();
                    }
                    break;
                case State.INAIR:
                    if (Input.IsActionJustPressed("crouch | slide"))
                    {
                        SlideStateChanges();
                    }
                    if (Input.IsActionJustPressed("dash"))
                    {
                        DashStateChanges();
                    }
                    if (Input.IsActionJustPressed("jump"))
                    {
                        Jump(0.0f, false);
                        JumpBuffering();
                    }
                    if (Input.IsActionJustPressed("grappleHook"))
                    {
                        GrappleStateChanges();
                    }
                    break;
                case State.ONWALL:
                    if (Input.IsActionJustPressed("jump"))
                    {
                        Jump(0.0f, false);
                    }
                    break;
                case State.DASH:
                    break;
                case State.GRAPPLE:
                    if (Input.IsActionJustPressed("jump"))
                    {
                        Jump(grapHookSpeed / 3, true);
                    }
                    if (Input.IsActionJustPressed("grappleHook"))
                    {
                        GrappleStateChanges();
                    }
                    break;                    
            }
        }
    }
    void DisplayStats()
    {
        // call the functions in charge of displaying the controller properties
        _hud.DisplayCurrentState(currentState);
        _hud.DisplayMoveSpeed(_moveSpeed);
        _hud.DisplayDesiredMoveSpeed(_desiredMoveSpeed);
        _hud.DisplayVelocity(Velocity.Length());
        _hud.DisplayNbDashsAllowed(DashesAllowed);
        _hud.DisplaySlideWaitTime(TimeBeforeCanSlideAgain);
        _hud.DisplayDashWaitTime(TimeBeforeCanDashAgain);
        _hud.DisplayNbJumpsAllowedInAir(JumpsInAirAllowed);
        _hud.DisplayGrappleHookToolWaitTime(TimeBeforeCanGrappleAgain);

        // not a property, but a visual
        if (currentState == State.DASH)
        {
            _hud.DisplaySpeedLinesAsync(DashTime);
        }
    }

    void Applies(double delta)
    {
        // general appliements
        _floorAngle = GetFloorNormal(); // get the angle of the floor

        if (!IsOnFloor())
        {
            // modify the type of gravity to apply to the character, depending of his velocity (when jumping jump gravity, otherwise fall gravity)
            if (Velocity.Y >= 0.0)
            {
                if (currentState != State.GRAPPLE)
                {
                    Velocity = new Vector3(
                        Velocity.X,
                        (float)(Velocity.Y + _jumpGravity * delta),
                        Velocity.Z);
                }

                if (currentState != State.SLIDE
                        && currentState != State.DASH
                        && currentState != State.GRAPPLE)
                {
                    currentState = State.JUMP;
                }
            }
            else
            {
                if (currentState != State.GRAPPLE)
                {
                    Velocity = new Vector3(
                        Velocity.X,
                        (float)(Velocity.Y + _fallGravity * delta),
                        Velocity.Z);
                }

                if (currentState != State.SLIDE
                        && currentState != State.DASH
                        && currentState != State.GRAPPLE)
                {
                    currentState = State.INAIR;
                }
            }

            if (currentState == State.SLIDE)
            {
                // if the character start slide on the ground, and then jumps, the slide is canceled
                if (!_startSlideInAir)
                {
                    SlideTime = -1;
                }
            }

            if (currentState == State.DASH)
            {
                // set the y axis velocity to 0, to allow the character to not be affected by gravity while dashing
                Velocity = new Vector3(Velocity.X, 0.0f, Velocity.Z);
            }

            if (HitGroundCooldown != _hitGroundCooldownRef)
            {
                // reset the before bunny hopping value
                HitGroundCooldown = _hitGroundCooldownRef;
            }

            if (CoyoteJumpCooldown > 0.0)
            {
                CoyoteJumpCooldown -= delta;
            }
        }
        if (IsOnFloor())
        {
            _slopeAngle = Mathf.RadToDeg(Mathf.Acos(_floorAngle.Dot(Vector3.Up))); // get the angle of the slope 

            if (currentState == State.SLIDE && _startSlideInAir)
            {
                _startSlideInAir = false;
            }

            if (_jumpBuffOn)
            {
                _jumpBuffOn = false;
                Jump(0.0f, false);
            }

            if (HitGroundCooldown >= 0)
            {
                HitGroundCooldown -= delta; // disincremente the value each frame, when it's <= 0, the player lose the speed he accumulated while being in the air 
            }

            if (JumpsInAirAllowed != _nbJumpsInAirAllowedRef)
            {
                JumpsInAirAllowed = _nbJumpsInAirAllowedRef; // set the number of jumps possible
            }

            if (CoyoteJumpCooldown != _coyoteJumpCooldownRef)
            {
                CoyoteJumpCooldown = _coyoteJumpCooldownRef;
            }
            // set the move state depending on the move speed, only when the character is moving

            // not the best piece of code i made, but i didn't really saw a more efficient way
            if (InputDirection != Vector2.Zero && MoveDirection != Vector3.Zero)
            {
                if (Mathf.IsEqualApprox(_moveSpeed, CrouchSpeed))
                    currentState = State.CROUCH;
                else if (Mathf.IsEqualApprox(_moveSpeed, WalkSpeed))
                    currentState = State.WALK;
                else if (Mathf.IsEqualApprox(_moveSpeed, RunSpeed))
                    currentState = State.RUN;
                else if (Mathf.IsEqualApprox(_moveSpeed, _slideSpeed))
                    currentState = State.SLIDE;
                else if (Mathf.IsEqualApprox(_moveSpeed, DashSpeed))
                    currentState = State.DASH;
                else if (Mathf.IsEqualApprox(_moveSpeed, grapHookSpeed))
                    _moveSpeed = RunSpeed;
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
        {
            WallrunStateChanges();
        }

        if (IsOnFloor() || !IsOnFloor())
        {
            // manage the slide behaviour
            if (currentState == State.SLIDE)
            {
                // if character slide on an uphill, cancel slide

                // there is a bug here related to the uphill/downhill slide 
                // (simply said, i have to adjust manually the lastFramePosition value in order to 
                // make the character slide indefinitely downhill bot not uphill)
                // if you know how to resolve that issue, don't hesitate to make a post about it on 
                // the discussions tab of the project's Github repository

                // don't know why i need to add a +0.1 to lastFramePosition.y, otherwise it breaks the mechanic some times
                if (!_startSlideInAir && _lastFramePosition.Y + 0.1 < Position.Y)
                {
                    SlideTime = -1;
                }

                if (!_startSlideInAir && _slopeAngle < MaxSlopeAngle)
                {
                    if (SlideTime > 0)
                    {
                        SlideTime -= delta;
                    }
                }

                if (SlideTime <= 0)
                {
                    TimeBeforeCanSlideAgain = _timeBeforeCanSlideAgainRef;
                    // go to crouch state if the ceiling is too low, otherwise go to run state 
                    if (_ceilingCheck.IsColliding())
                    {
                        CrouchStateChanges();
                    }
                    else
                    {
                        RunStateChanges();
                    }
                }
            }
            // manage the dash behaviour
            if (currentState == State.DASH)
            {
                if (_canInput)
                {
                    _canInput = false; // the character cannot change direction while dashing 
                }

                if (DashTime > 0)
                {
                    DashTime -= delta;
                }

                // the character cannot dash anymore, change to corresponding variables, and go back to run state
                if (DashTime <= 0)
                {
                    Velocity = _velocityPreDash; // go back to pre dash velocity
                    _canInput = true;
                    TimeBeforeCanDashAgain = TimeBeforeCanDashAgainRef;
                    RunStateChanges();
                }
            }

            if (TimeBeforeCanSlideAgain > 0.0)
            {
                TimeBeforeCanSlideAgain -= delta;
            }

            if (TimeBeforeCanDashAgain > 0.0)
            {
                TimeBeforeCanDashAgain -= delta;
            }

            // manage the dash reloading
            if (TimeBefReloadDash > 0.0 && DashesAllowed != _nbDashAllowedRef)
            {
                TimeBefReloadDash -= delta;
            }

            if (TimeBefReloadDash <= 0.0 && DashesAllowed != _nbDashAllowedRef)
            {
                TimeBefReloadDash = _timeBefReloadDashRef;
                DashesAllowed += 1;
            }

            if (TimeBeforeCanGrappleAgain > 0.0)
            {
                TimeBeforeCanGrappleAgain -= delta;
            }

            if (currentState == State.JUMP)
            {
                // the character cannot stick to structures while jumping
                FloorSnapLength = 0.0f;
            }

            if (currentState == State.INAIR)
            {
                // but he can if he stopped jumping, but he's still in the air
                FloorSnapLength = 2.5f; 
            }

            if (JumpCooldown > 0.0)
            {
                JumpCooldown -= delta;
            }
        }
    }

    void Move(double delta)
    {
        // direction input
        InputDirection = Input.GetVector("moveLeft", "moveRight", "moveForward", "moveBackward");

        // get direction input when sliding
        if (currentState == State.SLIDE)
        {
            // if the character is moving
            if (MoveDirection == Vector3.Zero)
            {
                // get move direction at the start of the slide, and stick to it
                MoveDirection = (_cameraHolder.Basis * new Vector3(
                        _slideVector.X,
                        0.0f,
                        _slideVector.Y))
                    .Normalized();
            }
        }
        // get direction input when wall running
        else if (currentState == State.ONWALL)
        {
            // get character current velocity and apply it as the current move direction, and stick to it
            MoveDirection = Velocity.Normalized();
        }
        // dash
        else if (currentState == State.DASH)
        {
            // if the character is moving, get move direction at the start of the dash, and stick to it
            if (MoveDirection == Vector3.Zero)
            {
                MoveDirection = (_cameraHolder.Basis * new Vector3(
                        InputDirection.X,
                        0.0f,
                        InputDirection.Y))
                    .Normalized();
            }
        }
        // all others 
        else
        {
            // get the move direction depending on the input
            MoveDirection = (_cameraHolder.Basis * new Vector3(
                    InputDirection.X,
                    0.0f,
                    InputDirection.Y))
                .Normalized();
        }

        // move applies when the character is on the floor
        if (IsOnFloor())
        {
            // if the character is moving
            if (Vector3.Zero != MoveDirection)
            {
                // apply slide move
                if (currentState == State.SLIDE)
                {
                    if (_slopeAngle > MaxSlopeAngle)
                    {
                        // increase more significantly desired move speed if the slope is steep enough
                        _desiredMoveSpeed += (float)(3.0 * delta);
                    }
                    else
                    {
                        _desiredMoveSpeed += (float)(2.0 * delta);
                    }

                    Velocity = new Vector3(
                        MoveDirection.X * _desiredMoveSpeed,
                        Velocity.Y,
                        MoveDirection.Z * _desiredMoveSpeed);
                }
                // apply dash move
                else if (currentState == State.DASH)
                {
                    Velocity = new Vector3(
                        MoveDirection.X * DashSpeed,
                        Velocity.Y,
                        MoveDirection.Z * DashSpeed);
                }
                // apply grapple hook desired move speed incrementation
                else if (currentState == State.GRAPPLE)
                {
                    if (_desiredMoveSpeed < MaxSpeed)
                    {
                        _desiredMoveSpeed += (float)(grapHookSpeed * delta);
                    }
                }
                // apply smooth move when walking, crouching, running
                else
                {
                    Velocity = new Vector3(
                        (float)Mathf.Lerp(Velocity.X, MoveDirection.X * _moveSpeed, _moveAcceleration * delta),
                        Velocity.Y,
                        (float)Mathf.Lerp(Velocity.Z, MoveDirection.Z * _moveSpeed, _moveAcceleration * delta)
                    );

                    // cancel desired move speed accumulation if the timer is out
                    if (HitGroundCooldown <= 0)
                    {
                        _desiredMoveSpeed = Velocity.Length();
                    }
                }
            }
            // if the character is not moving
            else
            {
                // apply smooth stop 
                Velocity = new Vector3(
                    (float)Mathf.Lerp(Velocity.X, 0.0, _moveAcceleration * delta),
                    Velocity.Y,
                    (float)Mathf.Lerp(Velocity.Z, 0.0, _moveAcceleration * delta)
                );

                // cancel desired move speed accumulation
                _desiredMoveSpeed = Velocity.Length();
            }
        }
        // move applies when the character is not on the floor (so if he's in the air)
        if (!IsOnFloor())
        {
            if (MoveDirection != Vector3.Zero)
            {
                // apply dash move
                if (currentState == State.DASH)
                {
                    Velocity = new Vector3(MoveDirection.X * DashSpeed, Velocity.Y, MoveDirection.Z * DashSpeed);
                }
                // apply slide move
                else if (currentState == State.SLIDE)
                {
                    _desiredMoveSpeed += (float)(2.5 * delta);
                    Velocity = new Vector3(MoveDirection.X * _desiredMoveSpeed, Velocity.Y, MoveDirection.Z * _desiredMoveSpeed);
                }
                // apply grapple hook desired move speed incrementation
                else if (currentState == State.GRAPPLE)
                {
                    if (_desiredMoveSpeed < MaxSpeed)
                    {
                        _desiredMoveSpeed += (float)(grapHookSpeed * delta);
                    }
                }
                // apply smooth move when in the air (air control)
                else
                {
                    if (_desiredMoveSpeed < MaxSpeed)
                    {
                        _desiredMoveSpeed += (float)(1.5 * delta);
                    }

                    // here, set the air control amount depending on a custom curve, to select it 
                    // with precision, depending on the desired move speed
                    float contrdDesMoveSpeed = DesiredMoveSpeedCurve.Sample(_desiredMoveSpeed / 100);
                    float contrdInAirMoveSpeed = InAirMoveSpeedCurve.Sample(_desiredMoveSpeed);

                    Velocity = new Vector3(
                        (float)Mathf.Lerp(Velocity.X, MoveDirection.X * contrdDesMoveSpeed, contrdInAirMoveSpeed * delta),
                        Velocity.Y,
                        (float)Mathf.Lerp(Velocity.Z, MoveDirection.Z * contrdDesMoveSpeed, contrdInAirMoveSpeed * delta)
                    );
                }
            }
            else
            {
                // accumulate desired speed for bunny hopping
                _desiredMoveSpeed = Velocity.Length();
            }
        }
        // move applies when the character is on the wall
        if (IsOnWall())
        {
            // apply on wall move
            if (currentState == State.ONWALL)
            {
                if (MoveDirection != Vector3.Zero)
                {
                    _desiredMoveSpeed += (float)(1.0 * delta);
                }

                Velocity = new Vector3(
                    MoveDirection.X * _desiredMoveSpeed,
                    Velocity.Y,
                    MoveDirection.Z * _desiredMoveSpeed);
            }
        }
        if (_desiredMoveSpeed >= MaxSpeed)
        {
            // set to ensure the character don't exceed the max speed authorized
            _desiredMoveSpeed = MaxSpeed;
        }

        _lastFramePosition = Position;
        _wasOnFloor = !IsOnFloor();
    }

    // this function manages the jump behaviour, depending on the different variables and states the character is in
    public void Jump(float jumpBoostValue, bool isJumpBoost)
    {
        bool canJump = false; // jump condition

        // the jump can only be applied if the player character is pulled up
        if (currentState == State.GRAPPLE && _lastFramePosition.Y > Position.Y && !DownDirJump)
        {
            if (!Mathf.IsEqualApprox(jumpBoostValue, 0.0f))
            {
                jumpBoostValue = 0.0f;
            }
            if (isJumpBoost)
            {
                isJumpBoost = false;
            }
            GrappleStateChanges();
            return;
        }
        // on wall jump 
        if (IsOnWall() && _canWallRun)
        {
            currentState = State.JUMP;
            // add some knockback in the opposite direction of the wall
            Velocity = GetWallNormal() * WallJumpVelocity;
            Velocity = new Vector3(Velocity.X, _jumpVelocity, Velocity.Z);
            JumpCooldown = _jumpCooldownRef;
        }
        else
        {
            // in air jump
            if (!IsOnFloor())
            {
                if (JumpCooldown <= 0)
                {
                    // determine if the character are in the conditions for enable coyote jump
                    if (_wasOnFloor && CoyoteJumpCooldown > 0.0 && _lastFramePosition.Y > Position.Y)
                        _coyoteJumpOn = true;

                    // if the character jump from a jumppad, the jump isn't taken into account in 
                    // the max numbers of jumps allowed, allowing the character to continusly 
                    // jump as long as it lands on a jumppad
                    if (JumpsInAirAllowed > 0
                        || (JumpsInAirAllowed <= 0
                        && isJumpBoost)
                        || _coyoteJumpOn)
                    {
                        // also, take into account if the character is coyote jumping
                        if (!isJumpBoost && !_coyoteJumpOn)
                        {
                            JumpsInAirAllowed -= 1;
                        }

                        JumpCooldown = _jumpCooldownRef;
                    }
                    _coyoteJumpOn = false;
                    canJump = true;
                }
            }
            // on floor jump
            else
            {
                JumpCooldown = _jumpCooldownRef;
                canJump = true;
            }
        }
        // apply jump
        if (canJump)
        {
            if (isJumpBoost)
            {
                JumpsInAirAllowed = _nbJumpsInAirAllowedRef;
            }
            currentState = State.JUMP;
            // apply directly jump velocity to y axis velocity, to give the character instant vertical forcez
            Velocity = new Vector3(
                Velocity.X,
                _jumpVelocity + jumpBoostValue,
                Velocity.Z);
            canJump = false;
        }
    }

    void JumpBuffering()
    {
        // if the character is falling, and the floor check raycast is colliding and the jump 
        // properties are good, enable jump buffering
        if (_floorCheck.IsColliding()
                && _lastFramePosition.Y > Position.Y
                && JumpsInAirAllowed <= 0
                && JumpCooldown <= 0.0)
            _jumpBuffOn = true;
    }

    void GrappleHookManagement(double delta)
    {
        // distance entre le personnae et le point d'ancrage du grappin
        float distToAnchorPoint = 0.0f;
        GrappleHookMove(delta, distToAnchorPoint);
        GrappleHookRopeManagement(distToAnchorPoint);
    }

    void GrappleHookMove(double delta, float distToAnchorPoint)
    {
        if (currentState == State.GRAPPLE)
        {
            // direction to move on
            MoveDirection = GlobalPosition.DirectionTo(anchorPoint); 
            // distance from anchor point to character
            distToAnchorPoint = GlobalPosition.DistanceTo(anchorPoint); 
            if (MoveDirection != Vector3.Zero)
            {
                // apply grapple hook move
                if (IsOnFloor())
                {
                    if (distToAnchorPoint > DistToStopGrappleIAir)
                    {
                        Velocity = Velocity.Lerp(MoveDirection * grapHookSpeed, (float)(GrapHookAccel * delta));
                    }
                    else
                    {
                        GrappleStateChanges();
                    }
                }
                if (!IsOnFloor())
                {
                    if (distToAnchorPoint > DistToStopGrappleOnFloor)
                    {
                        Velocity = Velocity.Lerp(MoveDirection * grapHookSpeed, (float)(GrapHookAccel * delta));
                    }
                    else
                    {
                        GrappleStateChanges();
                    }
                }
            }

        }
    }

    void GrappleHookRopeManagement(float distToAnchorPoint)
    {
        // hide the rope
        if (currentState != State.GRAPPLE)
        {
            if (_grapHookRope.Visible)
            {
                _grapHookRope.Visible = false;
            }
            return;
        }
        else
        {
            // show the rope at the corresponding point and direction
            if (!_grapHookRope.Visible)
            {
                _grapHookRope.Visible = true;
            }

            _grapHookRope.LookAt(anchorPoint);
            distToAnchorPoint = GlobalPosition.DistanceTo(anchorPoint);
            // change the scale to make the rope take all the direction width
            _grapHookRope.Scale = new Vector3(0.07f, 0.18f, distToAnchorPoint);
        }
    }

    // theses functions manages the differents changes and appliments the character will go trought 
    // when changing his current state
    void CrouchStateChanges()
    {
        currentState = State.CROUCH;
        _moveSpeed = CrouchSpeed;
        _moveAcceleration = crouchAcceleration;
        _moveDecceleration = CrouchDecceleration;

        _standHitbox.Disabled = true;
        _crouchHitbox.Disabled = false;

        if (!Mathf.IsEqualApprox(_mesh.Scale.Y, 0.7))
        {
            _mesh.Scale = new Vector3(_mesh.Scale.X, 0.7f, _mesh.Scale.Z);
            _mesh.Position = new Vector3(_mesh.Position.X, -0.5f, _mesh.Position.Z);
        }
    }

    void WalkStateChanges()
    {
        currentState = State.WALK;
        _moveSpeed = WalkSpeed;
        _moveAcceleration = WalkAcceleration;
        _moveDecceleration = WalkDecceleration;

        _standHitbox.Disabled = false;
        _crouchHitbox.Disabled = true;

        if (!Mathf.IsEqualApprox(_mesh.Scale.Y, 1.0))
        {
            _mesh.Scale = new Vector3(_mesh.Scale.X, 1.0f, _mesh.Scale.Z);
            _mesh.Position = new Vector3(_mesh.Position.X, 1.0f, _mesh.Position.Z);
        }
    }

    void RunStateChanges()
    {
        currentState = State.RUN;
        _moveSpeed = RunSpeed;
        _moveAcceleration = RunAcceleration;
        _moveDecceleration = RunDecceleration;

        _standHitbox.Disabled = false;
        _crouchHitbox.Disabled = true;

        if (!Mathf.IsEqualApprox(_mesh.Scale.Y, 1.0))
        {
            _mesh.Scale = new Vector3(_mesh.Scale.X, 1.0f, _mesh.Scale.Z);
            _mesh.Position = new Vector3(_mesh.Position.X, 1.0f, _mesh.Position.Z);
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
                _startSlideInAir = true;
            }
            else if (IsOnFloor() && _lastFramePosition.Y >= Position.Y)
            {
                // character can slide only on flat or downhill surfaces: 
                _desiredMoveSpeed += SlideSpeedAddon; // slide speed boost when on ground (for balance purpose)
                _startSlideInAir = false;
            }

            SlideTime = SlideTimeRef;
            _moveSpeed = _slideSpeed;
            if (InputDirection != Vector2.Zero)
            {
                _slideVector = InputDirection;
            }
            else
            {
                _slideVector = new Vector2(0, -1);
            }

            _standHitbox.Disabled = true;
            _crouchHitbox.Disabled = false;

            if (!Mathf.IsEqualApprox(_mesh.Scale.Y, 0.7))
            {
                _mesh.Scale = new Vector3(_mesh.Scale.X, 0.7f, _mesh.Scale.Z);
                _mesh.Position = new Vector3(_mesh.Position.X, -0.5f, _mesh.Position.Z);
            }
        }
        else if (currentState == State.SLIDE)
        {
            SlideTime = -1.0;
            TimeBeforeCanSlideAgain = _timeBeforeCanSlideAgainRef;
            if (_ceilingCheck.IsColliding())
            {
                CrouchStateChanges();
            }
            else
            {
                RunStateChanges();
            }
        }
    }

    void DashStateChanges()
    {
        // condition here, the state is changed only if the character is moving (so has an input direction)
        if (InputDirection != Vector2.Zero && TimeBeforeCanDashAgain <= 0.0 && DashesAllowed > 0)
        {
            currentState = State.DASH;
            DashesAllowed -= 1;
            _moveSpeed = DashSpeed;
            DashTime = _dashTimeRef;
            _velocityPreDash = Velocity; // save the pre dash velocity, to apply it when the dash is finished (to get back to a normal velocity)

            if (!Mathf.IsEqualApprox(_mesh.Scale.Y, 1.0))
            {
                _mesh.Scale = new Vector3(_mesh.Scale.X, 1.0f, _mesh.Scale.Z);
                _mesh.Position = new Vector3(_mesh.Position.X, 1.0f, _mesh.Position.Z);
            }
        }
    }

    void WallrunStateChanges()
    {
        // condition here, the state is changed only if the character speed is greater than the walk speed
        if (Velocity.Length() > WalkSpeed && currentState != State.DASH && currentState != State.CROUCH && _canWallRun)
        {
            currentState = State.ONWALL;
            Velocity = new Vector3(Velocity.X, Velocity.Y * WallGravityMultiplier, Velocity.Z); // gravity value became onwall one


            if (JumpsInAirAllowed != _nbJumpsInAirAllowedRef)
            {
                JumpsInAirAllowed = _nbJumpsInAirAllowedRef;
            }

            _standHitbox.Disabled = false;
            _crouchHitbox.Disabled = true;

            if (!Mathf.IsEqualApprox(_mesh.Scale.Y, 1.0))
            {
                _mesh.Scale = new Vector3(_mesh.Scale.X, 1.0f, _mesh.Scale.Z);
                _mesh.Position = new Vector3(_mesh.Position.X, 1.0f, _mesh.Position.Z);
            }
        }
    }

    void GrappleStateChanges()
    {
        // condition here, the state is changed only if the character isn't already grappling, and the grapple check is colliding
        if (_grappleHookCheck.IsColliding()
                && TimeBeforeCanGrappleAgain <= 0.0
                && currentState != State.GRAPPLE)
        {
            currentState = State.GRAPPLE;

            if (IsOnFloor())
            {
                Velocity = new Vector3(Velocity.X, GrappleLaunchJumpVelocity, Velocity.Z);
            }

            TimeBeforeCanGrappleAgain = timeBeforeCanGrappleAgainRef;
            if (JumpsInAirAllowed < _nbJumpsInAirAllowedRef)
            {
                JumpsInAirAllowed = _nbJumpsInAirAllowedRef;
            }

            _moveSpeed = grapHookSpeed;

            // get the collision point of the grapple hook raycast check
            anchorPoint = _grappleHookCheck.GetCollisionPoint();

            _standHitbox.Disabled = false;
            _crouchHitbox.Disabled = true;

            if (!Mathf.IsEqualApprox(_mesh.Scale.Y, 1.0))
            {
                _mesh.Scale = new Vector3(_mesh.Scale.X, 1.0f, _mesh.Scale.Z);
                _mesh.Position = new Vector3(_mesh.Position.X, 1.0f, _mesh.Position.Z);
            }
        }
        // the character is already grappling, so cut grapple state, and change to the one corresponding to his velocity
        else if (currentState == State.GRAPPLE)
        {
            if (!IsOnFloor())
            {
                if (Velocity.Y >= 0.0)
                {
                    currentState = State.JUMP;
                }
                else
                {
                    currentState = State.INAIR;
                }
            }
        }
    }

    void CollisionHandling()
    {
        // this function handle the collisions, but in this case, only the collision with a wall, 
        // to detect if the character can wallrun
        if (IsOnWall())
        {
            KinematicCollision3D lastCollision = GetSlideCollision(0);

            if (lastCollision != null)
            {
                CollisionObject3D collidedBody = lastCollision.GetCollider() as CollisionObject3D;
                if (collidedBody != null)
                {
                    uint layer = collidedBody.CollisionLayer;

                    // here, we check the layer of the collider, then we check if the layer 3 
                    // (walkableWall) is enabled, with 1 << 3-1. If theses two points are valid, the character can wallrun
                    if ((layer & (1 << 3 - 1)) != 0)
                    {
                        _canWallRun = true;
                    }
                    else
                    {
                        _canWallRun = false;
                    }
                }
                else
                {
                    // try the same for CSG shapes as they don't have a common ancestory re collision layer
                    CsgShape3D csgShape = lastCollision.GetCollider() as CsgShape3D;
                    if (csgShape != null)
                    {
                        uint layer = csgShape.CollisionLayer;

                        if ((layer & (1 << 3 - 1)) != 0)
                        {
                            _canWallRun = true;
                        }
                        else
                        {
                            _canWallRun = false;
                        }
                    }
                }
            }
        }
    }

    // this function handles the knockback mechanic
    void OnObjectToolSendKnockback(float knockbackAmount, Vector3 knockbackOrientation)
    {
        // opposite of the knockback tool orientation, times knockback amount
        Vector3 knockbackForce = -knockbackOrientation * knockbackAmount;
        Velocity += !IsOnFloor() ? knockbackForce : knockbackForce / OnFloorKnockbackDivider;
    }
}

/// <summary>
/// Only used to diplay "prettified" state names instead of raw enum identifiers via class extension
/// </summary>
public static class StateExtensions
{
    public static string GetDisplayName(this PlayerCharacter.State value)
    {
        var member = value.GetType().GetMember(value.ToString())[0];
        var attr = member.GetCustomAttribute<DisplayAttribute>();
        return attr?.GetName() ?? value.ToString();
    }
}
