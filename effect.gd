#abstract
@tool
class_name Effect
extends Resource


enum Flags {
	Immediate = (1 << 0)
}

@export var value: float
@export_range(0, 1, 0.01) var apply_chance: float

@export_range(1, 30, 1) var tick_timeout: int = 3
@export_range(0, 100, 1) var ticks: int:
	set(v):
		ticks = v
		_current_tick = v

# exported through _get_property_list
var flags: int

var _current_tick: int

func _init() -> void:
	assert(false, "Don't instantiate base class Effect")


func tick() -> bool:
	_current_tick -= 1
	return _current_tick > 0


func is_immediate() -> bool:
	return not (flags & Flags.Immediate) == 0


func get_tooltip() -> String:
	if apply_chance >= 1:
		return "Equip: "
	
	return "On hit (%d%%): " % int(apply_chance * 100.0)


func apply(_p_target: CombatSystem, _p_attacker: CombatSystem) -> int:
	assert(false, "Don't call apply on the base class Effect")
	return 0


func _get_property_list() -> Array[Dictionary]:
	return [{
		"name": "flags",
		"type": TYPE_INT,
		"usage": PROPERTY_USAGE_DEFAULT | PROPERTY_USAGE_SCRIPT_VARIABLE,
		"hint": PROPERTY_HINT_FLAGS,
		"hint_string": ','.join(Flags.keys()),
	}]
