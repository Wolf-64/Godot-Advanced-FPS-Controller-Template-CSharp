using Godot;
using System;

public partial class ObjectTool : Node3D
{
    [ExportGroup("Knockback variables")]

    /// <summary>
    /// Force of knockback action.
    /// </summary>
    [Export]
    public float KnockbackAmount { get; set; } = 36f;

    /// <summary>
    /// Cooldown of knockback action.
    /// </summary>
    [Export]
    public float WaitTimeBefCanUseKnobaAgain { get; set; } = 0.31f;
    private float _waitTimeBefCanUseKnobaAgainRef;

    // @onready
    private Node3D _knockbackToolAttackPoint;
    // @onready
    private AnimationPlayer _animationPlayer;
    // @onready
    private HUD _hud;

    [Signal]
    public delegate void sendKnockbackEventHandler(float amount, Vector3 direction);

    public override void _Ready()
    {
        _knockbackToolAttackPoint = GetNode<Node3D>("KnockbackTool/KnockbackToolAttackPoint");
        _animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        _hud = GetNode<HUD>("../../../HUD");
        _waitTimeBefCanUseKnobaAgainRef = WaitTimeBefCanUseKnobaAgain;
    }

    public override void _Process(double delta)
    {
        Use((float)delta);
        TimeManagement((float)delta);
        SendProperties();
    }

    /// <summary>
    /// Invokes knockback action.
    /// </summary>
    /// <param name="_delta"></param>
    private void Use(float _delta)
    {
        if (Input.IsActionJustPressed("useKnockbackTool"))
        {
            // send a knockback action to the character
            if (WaitTimeBefCanUseKnobaAgain <= 0.0)
            {
                WaitTimeBefCanUseKnobaAgain = _waitTimeBefCanUseKnobaAgainRef;

                EmitSignal(SignalName.sendKnockback, KnockbackAmount, -GlobalTransform.Basis.Z.Normalized());
                _animationPlayer.Play("useKnockbackTool");
            }
        }
    }

    /// <summary>
    /// Handles cooldown.
    /// </summary>
    /// <param name="delta"></param>
    private void TimeManagement(float delta)
    {
        if (WaitTimeBefCanUseKnobaAgain > 0.0)
            WaitTimeBefCanUseKnobaAgain -= delta;
    }

    /// <summary>
    /// Handles HUD value display.
    /// </summary>
    private void SendProperties()
    {
        // display knockback tool properties
        _hud.DisplayKnockbackToolWaitTime(WaitTimeBefCanUseKnobaAgain);
    }
}
