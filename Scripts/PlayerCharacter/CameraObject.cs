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
    private float _targetFOV;
    private float _lastFOV;
    private float _addonFOV;
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
    private float _shakeForce;
    [Export]
    public float ShakeDuration { get; set; } = 0.35f;
    private float _shakeDurationRef;
    [Export]
    public float ShakeFade { get; set; } = 6f;
    RandomNumberGenerator rng = new RandomNumberGenerator();
    private bool _canCameraShake = false;

    // input variables
    [ExportGroup("input variables")]
    Vector2 mouseInput;
    [Export]
    public float MouseInputSpeed { get; set; } = 2f;
    private Vector2 _playCharInputDir;

    //  references variables
    // @onready
    private Camera3D _camera;
    // @onready
    private PlayerCharacter _playerChar;
    // @onready
    private PauseMenu _pauseMenu;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        _camera = GetNode<Camera3D>("Camera3D");
        _playerChar = GetNode<PlayerCharacter>("..");
        _pauseMenu = GetNode<PauseMenu>("../PauseMenu");

        Input.MouseMode = Input.MouseModeEnum.Captured;

        _lastFOV = BaseFOV;
        _shakeDurationRef = ShakeDuration;
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        // this function manage camera rotation (360 on x axis, blocked at <= -60 and >= 60 on 
        // y axis, to not having the character do a complete head turn, which will be kinda weird)
        if (!_pauseMenu.PauseMenuEnabled)
        { // can only rotate when the ui is not opened
            if (@event is InputEventMouseMotion mouseMotion)
            {
                RotateY(-mouseMotion.Relative.X * XAxisSensibility);
                _camera.RotateX(-mouseMotion.Relative.Y * YAxisSensibility);

                _camera.Rotation = new Vector3(
                    Mathf.Clamp(
                        _camera.Rotation.X,
                        Mathf.DegToRad(MaxUpAngleView),
                        Mathf.DegToRad(MaxDownAngleView)),
                    _camera.Rotation.Y,
                    _camera.Rotation.Z);
                // get position of the mouse in a 2D sceen, so save it in a Vector2 
                mouseInput = mouseMotion.Relative; 
            }
        }
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        Applies((float)delta);
        CameraBob((float)delta);
        CameraTilt((float)delta);
        FOVChange((float)delta);
        _lastFOV = _targetFOV; // get the last FOV used
    }


    public void Applies(float delta)
    {
        // this function manage the differents camera modifications relative to a specific state, 
        // except for the FOV
        float newPosY = 0.0f;
        float newRotZ = 0.0f;
        switch (_playerChar.currentState)
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
                newRotZ = Mathf.Lerp(
                    Rotation.Z,
                    Mathf.DegToRad(6.0f) *
                        (!Mathf.IsEqualApprox(_playCharInputDir.X, 0.0f)
                            ? _playCharInputDir.X 
                            : Mathf.DegToRad(6.0f)),
                    SlideCameraLerpSpeed * delta);
                break;
            case PlayerCharacter.State.SLIDE:
                // lean the camera a bit more
                newPosY = Mathf.Lerp(Position.Y, 0.715f + SlideCameraDepth, CrouchCameraLerpSpeed * delta);
                newRotZ = Mathf.Lerp(
                    Rotation.Z,
                    Mathf.DegToRad(10.0f) *
                        (!Mathf.IsEqualApprox(_playCharInputDir.X, 0.0f)
                            ? _playCharInputDir.X 
                            : Mathf.DegToRad(10.0f)),
                    SlideCameraLerpSpeed * delta);
                break;
        }

        Position = new Vector3(Position.X, newPosY, Position.Z);
        Rotation = new Vector3(Rotation.X, Rotation.Y, newRotZ);
    }

    public void CameraBob(float delta)
    {
        // this function manage the bobbing of the camera when the character is moving
        if (_playerChar.currentState != PlayerCharacter.State.SLIDE
                && _playerChar.currentState != PlayerCharacter.State.DASH)
        {
            // the bobbing doesn't apply when the character is sliding or is dashing
            HeadBobValue += delta * _playerChar.Velocity.Length() * (_playerChar.IsOnFloor() ? 1.0f : 0.0f);

            Transform3D transform = _camera.Transform;
            transform.Origin = Headbob(HeadBobValue);
            _camera.Transform = transform; // apply the bob effect obtained to the camera
        }
    }

    public Vector3 Headbob(float time)
    {
        // some trigonometry stuff here, basically it uses the cosinus and sinus functions 
        // (sinusoidal function) to get a nice and smooth bob effect
        float y = Mathf.Sin(time * BobFrequency) * BobAmplitude;
        float x = Mathf.Cos(time * BobFrequency / 2) * BobAmplitude;
        return new Vector3(x, y, 0.0f);
    }

    public void CameraTilt(float delta)
    {
        // this function manage the camera tilting when the character is moving on the x axis (left and right)
        if (_playerChar.MoveDirection != Vector3.Zero
                && _playerChar.currentState != PlayerCharacter.State.CROUCH
                && _playerChar.currentState != PlayerCharacter.State.SLIDE)
        {
            // the camera tilting doesn't apply when the character is not moving, or is crouching or walking  
            _playCharInputDir = _playerChar.InputDirection; // get input direction to know where the character is heading to
                                                            // apply smooth tilt movement
            if (!_playerChar.IsOnFloor())
            {
                Rotation = new Vector3(
                    Rotation.X,
                    Rotation.Y,
                    Mathf.Lerp(Rotation.Z, -_playCharInputDir.X * CamTiltRotationValue / 1.6f, CamTiltRotationSpeed * delta));
            }
            else
            {
                Rotation = new Vector3(
                    Rotation.X,
                    Rotation.Y,
                    Mathf.Lerp(Rotation.Z, -_playCharInputDir.X * CamTiltRotationValue, CamTiltRotationSpeed * delta));
            }
        }
    }
    public void FOVChange(float delta)
    {
        // FOV addon used to keep a logic FOV (for example, FOV when the character jumps right 
        // after running should be a bit higher than when he jumps right after walking)
        if (Mathf.IsEqualApprox(_lastFOV, BaseFOV))
        {
            _addonFOV = 0f;
        }
        if (Mathf.IsEqualApprox(_lastFOV, RunFOV))
        {
            _addonFOV = 10f;
        }
        if (Mathf.IsEqualApprox(_lastFOV, SlideFOV))
        {
            _addonFOV = 30f;
        }

        // get the corresponding FOV to the current state the character is
        switch (_playerChar.currentState)
        {
            case PlayerCharacter.State.IDLE:
                _targetFOV = BaseFOV;
                break;
            case PlayerCharacter.State.CROUCH:
                _targetFOV = CrouchFOV;
                break;
            case PlayerCharacter.State.WALK:
                _targetFOV = BaseFOV;
                break;
            case PlayerCharacter.State.RUN:
                _targetFOV = RunFOV;
                break;
            case PlayerCharacter.State.SLIDE:
                _targetFOV = SlideFOV;
                break;
            case PlayerCharacter.State.DASH:
                _targetFOV = DashFOV;
                break;
            case PlayerCharacter.State.JUMP:
                _targetFOV = BaseFOV + _addonFOV;
                break;
            case PlayerCharacter.State.INAIR:
                _targetFOV = BaseFOV + _addonFOV;
                break;
        }

        // smoothly apply the FOV
        // the dash state has it's own get-to-FOV speed, because the action is very quick and 
        // so the FOV change won't be seen with the regular get-to-FOV speed
        if (_playerChar.currentState == PlayerCharacter.State.DASH)
        {
            _camera.Fov = Mathf.Lerp(_camera.Fov, _targetFOV, FovChangeSpeedWhenDash * delta);
        }
        else
        {
            _camera.Fov = Mathf.Lerp(_camera.Fov, _targetFOV, FovChangeSpeed * delta);
        }
    }
}
