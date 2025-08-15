using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public partial class HUD: Control
{
	// @onready
	Label currentStateLabelText;
	// @onready
	Label moveSpeedLabelText;
	// @onready
	Label desiredMoveSpeedLabelText;
	// @onready
	Label velocityLabelText;
	// @onready
	Label nbJumpsAllowedInAirLabelText;
	// @onready
	Label nbDashsAllowedLabelText;
	// @onready
	Label slideWaitTimeLabelText;
	// @onready
	Label dashWaitTimeLabelText;
	// @onready
	Label knockbackToolWaitTimeLabelText;
	// @onready
	Label grappleToolWaitTimeLabelText;
	// @onready
	Label framesPerSecondLabelText;
	// @onready
	ColorRect speedLinesContainer;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		currentStateLabelText = GetNode<Label>("HBoxContainer/VBoxContainer2/CurrentStateLabelText");
		moveSpeedLabelText = GetNode<Label>("HBoxContainer/VBoxContainer2/MoveSpeedLabelText");
		desiredMoveSpeedLabelText = GetNode<Label>("HBoxContainer/VBoxContainer2/DesiredMoveSpeedLabelText");
		velocityLabelText = GetNode<Label>("HBoxContainer/VBoxContainer2/VelocityLabelText");
		nbJumpsAllowedInAirLabelText = GetNode<Label>("HBoxContainer/VBoxContainer2/NbJumpsInAirLabelText");
		nbDashsAllowedLabelText = GetNode<Label>("HBoxContainer/VBoxContainer2/NbDashsAllowedLabelText");
		slideWaitTimeLabelText = GetNode<Label>("HBoxContainer/VBoxContainer2/SlideWaitTimeLabelText");
		dashWaitTimeLabelText = GetNode<Label>("HBoxContainer/VBoxContainer2/DashWaitTimeLabelText");
		knockbackToolWaitTimeLabelText = GetNode<Label>("HBoxContainer/VBoxContainer2/KnockbackToolWaitTimeLabelText");
		grappleToolWaitTimeLabelText = GetNode<Label>("HBoxContainer/VBoxContainer2/GrappleToolWaitTimeLabelText");
		framesPerSecondLabelText = GetNode<Label>("HBoxContainer/VBoxContainer2/FramesPerSecondLabelText");
		speedLinesContainer = GetNode<ColorRect>("SpeedLinesContrainer");
		speedLinesContainer.Visible = false; // the speed lines will only be displayed when the character will dashing
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		// this function manage the frames per second displayment
		framesPerSecondLabelText.Text = Engine.GetFramesPerSecond().ToString();
	}

	// this function manage the current state displayment
	public void displayCurrentState(PlayerCharacter.State currentState)
	{
		// set the state name to display according to the parameter value
		currentStateLabelText.Text = Enum.GetName(currentState);
	}

	/// <summary>
	/// This function manages the move speed display
	/// </summary>
	public void displayMoveSpeed(float moveSpeed)
	{
		moveSpeedLabelText.Text = moveSpeed.ToString();
	}

	public void displayDesiredMoveSpeed(float desiredMoveSpeed)
	{
		// this function manage the desired move speed displayment
		desiredMoveSpeedLabelText.Text = desiredMoveSpeed.ToString();
	}
	public void displayVelocity(float velocity)
	{
		// this function manage the current velocity displayment
		velocityLabelText.Text = velocity.ToString();
	}

	public void displayNbJumpsAllowedInAir(int nbJumpsAllowedInAir)
	{
		// this function manage the nb jumps allowed left displayment
		nbJumpsAllowedInAirLabelText.Text = nbJumpsAllowedInAir.ToString();
	}

	public void displayNbDashsAllowed(int nbDashsAllowed)
	{
		// this function manage the nb dashs allowed left displayment
		nbDashsAllowedLabelText.Text = nbDashsAllowed.ToString();
	}

	public void displaySlideWaitTime(double slideWaitTime)
	{
		slideWaitTimeLabelText.Text = slideWaitTime.ToString();
	}


	public void displayDashWaitTime(double dashWaitTime)
	{
		dashWaitTimeLabelText.Text = dashWaitTime.ToString();
	}

	public void displayKnockbackToolWaitTime(double timeBefCanUseAgain)
	{
		// this function manage the knockback tool time left displayment
		knockbackToolWaitTimeLabelText.Text = timeBefCanUseAgain.ToString();
	}

	public void displayGrappleHookToolWaitTime(double timeBefCanUseAgain)
	{
		// this function manage the grapple hook time left displayment
		grappleToolWaitTimeLabelText.Text = timeBefCanUseAgain.ToString();
	}

	public void displaySpeedLinesAsync(double dashTime)
	{
		// this function manages the speed lines displayment (only when the character is dashing)
		speedLinesContainer.Visible = true;
		// when the dash is finished, hide the speed lines
		//await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
		//await get_tree().create_timer(dashTime).timeout
		GetTree().CreateTimer(dashTime).Timeout += () => speedLinesContainer.Visible = false;
	}
}