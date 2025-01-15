extends HSlider

@export var busName : String
var busIndex : int

func _ready():
	busIndex = AudioServer.get_bus_index(busName) #set the bus index
	value_changed.connect(volumeValueChange) #connect the change volume action
	
	value = db_to_linear(AudioServer.get_bus_volume_db(busIndex)) #convert decibels to linear (for stockage purpose)
	
func volumeValueChange(value : float):
	AudioServer.set_bus_volume_db(busIndex, linear_to_db(value)) #set the volume of the audio bus selected by the bus index
	
	
	
