@tool
class_name AttributeComponent
extends ItemComponent

@export var attributes: Attributes


func get_tooltip() -> String:
	var tooltip := ""
#
	## TODO: extract this array through get_property_list in a static_init
	#var stats := ["strength", "stamina"]
	#for stat: String in stats:
		#var val: Variant = get(stat)
		#if not val == 0:
			#var sign_symbol: String = "+" if val > 0 else "-"
			#tooltip += "%s%d %s\n" % [sign_symbol, val, stat.capitalize()]

	return tooltip
