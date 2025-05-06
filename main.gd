extends Node


func _ready() -> void:
	assert(get_child(0) is UI, "UI node must be the first child in the scene.")
	assert(get_child(1) is Node3D, "World node must be the first child in the scene.")
	assert(get_child_count() == 2, "Main node must not contain more than 2 nodes.")
	
	$World/Character/CombatManager._current_target = $World/TestEnemy
