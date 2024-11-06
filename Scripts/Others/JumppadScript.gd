extends CSGCylinder3D

#class name
class_name Jumppad

#value variables
@export_group("value variables")
@export var jumpBoostValue : float

func _on_area_3d_area_entered(area):
	if area.get_parent() is PlayerCharacter: area.get_parent().jump(jumpBoostValue, true)
