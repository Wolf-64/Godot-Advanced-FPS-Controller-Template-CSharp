using Godot;
using System;

public partial class HUD: Control
{
	// @onready
	private Label _currentStateLabelText;
	// @onready
	private Label _moveSpeedLabelText;
	// @onready
	private Label _desiredMoveSpeedLabelText;
	// @onready
	private Label _velocityLabelText;
	// @onready
	private Label _nbJumpsAllowedInAirLabelText;
	// @onready
	private Label _nbDashsAllowedLabelText;
	// @onready
	private Label _slideWaitTimeLabelText;
	// @onready
	private Label _dashWaitTimeLabelText;
	// @onready
	private Label _knockbackToolWaitTimeLabelText;
	// @onready
	private Label _grappleToolWaitTimeLabelText;
	// @onready
	private Label _framesPerSecondLabelText;
	// @onready
	private ColorRect _speedLinesContainer;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_currentStateLabelText = GetNode<Label>("HBoxContainer/VBoxContainer2/CurrentStateLabelText");
		_moveSpeedLabelText = GetNode<Label>("HBoxContainer/VBoxContainer2/MoveSpeedLabelText");
		_desiredMoveSpeedLabelText = GetNode<Label>("HBoxContainer/VBoxContainer2/DesiredMoveSpeedLabelText");
		_velocityLabelText = GetNode<Label>("HBoxContainer/VBoxContainer2/VelocityLabelText");
		_nbJumpsAllowedInAirLabelText = GetNode<Label>("HBoxContainer/VBoxContainer2/NbJumpsInAirLabelText");
		_nbDashsAllowedLabelText = GetNode<Label>("HBoxContainer/VBoxContainer2/NbDashsAllowedLabelText");
		_slideWaitTimeLabelText = GetNode<Label>("HBoxContainer/VBoxContainer2/SlideWaitTimeLabelText");
		_dashWaitTimeLabelText = GetNode<Label>("HBoxContainer/VBoxContainer2/DashWaitTimeLabelText");
		_knockbackToolWaitTimeLabelText = GetNode<Label>("HBoxContainer/VBoxContainer2/KnockbackToolWaitTimeLabelText");
		_grappleToolWaitTimeLabelText = GetNode<Label>("HBoxContainer/VBoxContainer2/GrappleToolWaitTimeLabelText");
		_framesPerSecondLabelText = GetNode<Label>("HBoxContainer/VBoxContainer2/FramesPerSecondLabelText");
		_speedLinesContainer = GetNode<ColorRect>("SpeedLinesContrainer");
		_speedLinesContainer.Visible = false; // the speed lines will only be displayed when the character will dashing
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		// this function manage the frames per second displayment
		_framesPerSecondLabelText.Text = Engine.GetFramesPerSecond().ToString();
	}

	// this function manage the current state displayment
	public void DisplayCurrentState(PlayerCharacter.State currentState)
	{
		// set the state name to display according to the parameter value
		_currentStateLabelText.Text = currentState.GetDisplayName();
	}

	/// <summary>
	/// This function manages the move speed display
	/// </summary>
	public void DisplayMoveSpeed(float moveSpeed)
	{
		_moveSpeedLabelText.Text = moveSpeed.ToString();
	}

	public void DisplayDesiredMoveSpeed(float desiredMoveSpeed)
	{
		// this function manage the desired move speed displayment
		_desiredMoveSpeedLabelText.Text = desiredMoveSpeed.ToString();
	}
	public void DisplayVelocity(float velocity)
	{
		// this function manage the current velocity displayment
		_velocityLabelText.Text = velocity.ToString();
	}

	public void DisplayNbJumpsAllowedInAir(int nbJumpsAllowedInAir)
	{
		// this function manage the nb jumps allowed left displayment
		_nbJumpsAllowedInAirLabelText.Text = nbJumpsAllowedInAir.ToString();
	}

	public void DisplayNbDashsAllowed(int nbDashsAllowed)
	{
		// this function manage the nb dashs allowed left displayment
		_nbDashsAllowedLabelText.Text = nbDashsAllowed.ToString();
	}

	public void DisplaySlideWaitTime(double slideWaitTime)
	{
		_slideWaitTimeLabelText.Text = slideWaitTime.ToString();
	}


	public void DisplayDashWaitTime(double dashWaitTime)
	{
		_dashWaitTimeLabelText.Text = dashWaitTime.ToString();
	}

	public void DisplayKnockbackToolWaitTime(double timeBefCanUseAgain)
	{
		// this function manage the knockback tool time left displayment
		_knockbackToolWaitTimeLabelText.Text = timeBefCanUseAgain.ToString();
	}

	public void DisplayGrappleHookToolWaitTime(double timeBefCanUseAgain)
	{
		// this function manage the grapple hook time left displayment
		_grappleToolWaitTimeLabelText.Text = timeBefCanUseAgain.ToString();
	}

	public void DisplaySpeedLinesAsync(double dashTime)
	{
		// this function manages the speed lines displayment (only when the character is dashing)
		_speedLinesContainer.Visible = true;
		// when the dash is finished, hide the speed lines
		//await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
		//await get_tree().create_timer(dashTime).timeout
		GetTree().CreateTimer(dashTime).Timeout += () => _speedLinesContainer.Visible = false;
	}
}