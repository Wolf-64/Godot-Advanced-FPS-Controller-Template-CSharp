using Godot;
using System;

public partial class VolumeSlider : HSlider
{
    [Export]
    public string BusName { get; set; }
    private int _busIndex;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        _busIndex = AudioServer.GetBusIndex(BusName);
        // connect the change volume action
        ValueChanged += (value) => VolumeValueChange(value);
        // convert decibels to linear (for stockage purpose)
        Value = Mathf.DbToLinear(AudioServer.GetBusVolumeDb(_busIndex));
    }


    public void VolumeValueChange(double value)
    {
        // set the volume of the audio bus selected by the bus index
        AudioServer.SetBusVolumeDb(_busIndex, (float)Mathf.LinearToDb(value));
    }
}
