@tool
class_name StatsComponent
extends ItemComponent

@export_range(-500, 500) var strength: int = 0

@export_range(-500, 500) var stamina: int = 0


func get_tooltip() -> String:
	var tooltip := ""

	var stats := ["strength", "stamina"]
	for stat: String in stats:
		var val: Variant = get(stat)
		if not val == 0:
			var sign_symbol: String = "+" if val > 0 else "-"
			tooltip += "%s%d %s\n" % [sign_symbol, val, stat.capitalize()]

	return tooltip
