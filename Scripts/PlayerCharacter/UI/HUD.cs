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

		// the speed lines will only be displayed when the character is dashing
		_speedLinesContainer.Visible = false; 
	}

	public override void _Process(double delta)
	{
		_framesPerSecondLabelText.Text = Engine.GetFramesPerSecond().ToString();
	}

	public void DisplayCurrentState(PlayerCharacter.State currentState)
	{
		_currentStateLabelText.Text = currentState.GetDisplayName();
	}

	public void DisplayMoveSpeed(float moveSpeed)
	{
		_moveSpeedLabelText.Text = moveSpeed.ToString();
	}

	public void DisplayDesiredMoveSpeed(float desiredMoveSpeed)
	{
		_desiredMoveSpeedLabelText.Text = desiredMoveSpeed.ToString();
	}
	public void DisplayVelocity(float velocity)
	{
		_velocityLabelText.Text = velocity.ToString();
	}

	public void DisplayNbJumpsAllowedInAir(int nbJumpsAllowedInAir)
	{
		_nbJumpsAllowedInAirLabelText.Text = nbJumpsAllowedInAir.ToString();
	}

	public void DisplayNbDashsAllowed(int nbDashsAllowed)
	{
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
		_knockbackToolWaitTimeLabelText.Text = timeBefCanUseAgain.ToString();
	}

	public void DisplayGrappleHookToolWaitTime(double timeBefCanUseAgain)
	{
		_grappleToolWaitTimeLabelText.Text = timeBefCanUseAgain.ToString();
	}

	public void DisplaySpeedLinesAsync(double dashTime)
	{
		_speedLinesContainer.Visible = true;
		
		// when the dash is finished, hide the speed lines
		GetTree().CreateTimer(dashTime).Timeout += () => _speedLinesContainer.Visible = false;
	}
}