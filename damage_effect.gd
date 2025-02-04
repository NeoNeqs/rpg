@tool
class_name DamageEffect
extends Effect

# IMPORTANT: This empty constructors must be here.
# 			 It prevents a call to super._init() since Effect is abstract
func _init() -> void:
	pass

@export
var damage_type: CombatSystem.DamageType

func get_tooltip() -> String:
	var tooltip := "#color=04C404#" + super.get_tooltip()
	
	tooltip += "Damages a target wth %d %s damage" % [
		value, get_damage_name()
	]
	
	if not ticks == 0:
		tooltip += " every %d seconds" % tick_timeout
	
	tooltip += '.\n'
	
	return tooltip

func get_damage_name() -> String:
	return CombatSystem.DamageType.keys()[damage_type]
