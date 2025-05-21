@tool
class_name EffectComponent
extends ItemComponent

@export
var effects: Array[Effect] = []


func get_tooltip() -> String:
	var tooltip := ""
	
	for effect: Effect in effects:
		if effect.value == 0:
			continue
			
		tooltip += effect.get_tooltip()
			
	return tooltip
