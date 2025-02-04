@tool
class_name ModifierEffect
extends Effect

var modifier: String

# IMPORTANT: This empty constructors must be here.
# 			 It prevents a call to super._init() since Effect is abstract
func _init() -> void:
	pass

func get_modifier() -> String:
	return modifier

func get_tooltip() -> String:
	var tooltip := "#color=f111ff#" + super.get_tooltip()
	
	if value > 0:
		tooltip += "Increases "
	else:
		tooltip += "Decreases "
	tooltip += "%s by %d" % [modifier, value]
	
	if not ticks == 0:
		tooltip += " for %d seconds" % (ticks * tick_timeout)
	tooltip += '.\n'

	return tooltip

func _get_property_list() -> Array[Dictionary]:
	return [{
		"name": &"modifier",
		"type": TYPE_STRING,
		"hint": PROPERTY_HINT_ENUM,
		"hint_string": Attributes.get_attributes_as_hint_string(),
		"usage": PROPERTY_USAGE_SCRIPT_VARIABLE | PROPERTY_USAGE_DEFAULT
	}]
