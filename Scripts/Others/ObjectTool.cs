using Godot;
using System;

public partial class ObjectTool : Node3D
{
	[ExportGroup("Knockback variables")]
	[Export]
	public float KnockbackAmount { get; set; } = 36f;
	[Export]
	public float WaitTimeBefCanUseKnobaAgain { get; set; } = 0.31f;
	float waitTimeBefCanUseKnobaAgainRef;

	// @onready
	Node3D knockbackToolAttackPoint;
	// @onready
	AnimationPlayer animationPlayer;
	// @onready
	HUD hud;

	[Signal]
	public delegate void sendKnockbackEventHandler(float amount, Vector3 direction);

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		knockbackToolAttackPoint = GetNode<Node3D>("KnockbackTool/KnockbackToolAttackPoint");
		animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
		hud = GetNode<HUD>("../../../HUD");
		waitTimeBefCanUseKnobaAgainRef = WaitTimeBefCanUseKnobaAgain;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		Use((float)delta);
		TimeManagement((float)delta);
		SendProperties();
	}

	public void Use(float _delta)
	{
		if (Input.IsActionJustPressed("useKnockbackTool"))
		{
			// send a knockback action to the character
			if (WaitTimeBefCanUseKnobaAgain <= 0.0)
			{
				WaitTimeBefCanUseKnobaAgain = waitTimeBefCanUseKnobaAgainRef;

				EmitSignal(SignalName.sendKnockback, KnockbackAmount, -GlobalTransform.Basis.Z.Normalized());
				animationPlayer.Play("useKnockbackTool");
			}
		}
	}

	public void TimeManagement(float delta)
	{
		if (WaitTimeBefCanUseKnobaAgain > 0.0)
			WaitTimeBefCanUseKnobaAgain -= delta;
	}

	public void SendProperties()
	{
		// display knockback tool properties
		hud.DisplayKnockbackToolWaitTime(WaitTimeBefCanUseKnobaAgain);
	}
}
