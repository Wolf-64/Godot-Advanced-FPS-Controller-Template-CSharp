using Godot;
using System;

public partial class VolumeSlider : HSlider
{
	[Export]
	public string BusName { get; set; }
	int busIndex;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		busIndex = AudioServer.GetBusIndex(BusName); // set the bus index
		ValueChanged += (value) => VolumeValueChange(value); // connect the change volume action
		Value = Mathf.DbToLinear(AudioServer.GetBusVolumeDb(busIndex)); // convert decibels to linear (for stockage purpose)
	}


	public void VolumeValueChange(double value)
	{
		AudioServer.SetBusVolumeDb(busIndex, (float)Mathf.LinearToDb(value)); // set the volume of the audio bus selected by the bus index
	}
}
