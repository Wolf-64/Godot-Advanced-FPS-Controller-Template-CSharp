using Godot;
using System;

public partial class ObjectTool : Node3D
{
	[ExportGroup("Knockback variables")]
	[Export]
	public float knockbackAmount { get; set; } = 36f;
	[Export]
	public float waitTimeBefCanUseKnobaAgain { get; set; } = 0.31f;
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
		waitTimeBefCanUseKnobaAgainRef = waitTimeBefCanUseKnobaAgain;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		use((float)delta);
		timeManagement((float)delta);
		sendProperties();
	}

	public void use(float _delta)
	{
		if (Input.IsActionJustPressed("useKnockbackTool"))
		{
			// send a knockback action to the character
			if (waitTimeBefCanUseKnobaAgain <= 0.0)
			{
				waitTimeBefCanUseKnobaAgain = waitTimeBefCanUseKnobaAgainRef;

				EmitSignal(SignalName.sendKnockback, knockbackAmount, -GlobalTransform.Basis.Z.Normalized());
				animationPlayer.Play("useKnockbackTool");
			}
		}
	}

	public void timeManagement(float delta)
	{
		if (waitTimeBefCanUseKnobaAgain > 0.0)
			waitTimeBefCanUseKnobaAgain -= delta;
	}

	public void sendProperties()
	{
		// display knockback tool properties
		hud.displayKnockbackToolWaitTime(waitTimeBefCanUseKnobaAgain);
	}
}
