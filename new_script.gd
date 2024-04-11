extends Node

@export var i : JoltGeneric6DOFJoint3D;

func _ready():
	i.set_indexed("linear_limit_x/enabled", false);
