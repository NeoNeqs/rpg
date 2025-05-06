class_name CombatResources
extends Resource

signal died()

enum ResourceType {
	Rage,
	Mana,
	Energy
}

var current_health: int
var currect_resource_amount: int

var resource_type: ResourceType


func take_damage(p_damage: int) -> void:
	var test_health: int = current_health - p_damage
	
	if test_health <= 0:
		died.emit()
		return
	
	current_health = test_health
