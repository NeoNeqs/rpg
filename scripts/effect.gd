@tool
class_name Effect
extends Resource

signal finished()

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
var _logger := Logger.combat

func _init() -> void:
	assert(false, "Do not create this object.")

# TODO: make a RNG singleton that will handle chance rolls:
#       - roll n sided dice
#       - roll n sided dice with advantage
#       - roll weighted
#       - roll with pity (this is rng with state, the more you role the more
#         likely you're to get the desired number

func apply(p_attacker: CombatManager, p_target: CombatManager) -> Timer:
	if apply_chance < 1:
		var chance: float = floor(randf() * 100) / 100.0
		if chance > apply_chance:
			return null
	
	var damage: int = 0
	
	# Instant and permament effect
	if ticks == 0:
		_apply(p_attacker, p_target)
		return null
	
	if is_immediate():
		_apply(p_attacker, p_target)
	
	var timer := Timer.new()
	timer.autostart = true
	timer.timeout.connect(_apply_periodic_effect.bind(timer, p_attacker, p_target))
	timer.wait_time = tick_timeout
	
	return timer


func _apply_periodic_effect(p_timer: Timer, p_attacker: CombatManager, p_target: CombatManager) -> void:
	if _tick():
		_apply(p_attacker, p_target)
		return

	if not is_immediate():
		_apply(p_attacker, p_target)
	
	_remove(p_attacker, p_target)
	
	p_timer.stop()
	p_timer.queue_free()


func _apply(_p_attacker: CombatManager, _p_target: CombatManager) -> void:
	assert(false, "Do not call this method, idiot!")


func _remove(_p_attacker: CombatManager, _p_target: CombatManager) -> void:
	finished.emit()


func _tick() -> bool:
	_current_tick -= 1
	return _current_tick > 0


func is_immediate() -> bool:
	return not (flags & Flags.Immediate) == 0


	#var damage_effect := p_effect as DamageEffect
	#if not damage_effect == null:
		#var damage_value: int = damage_effect.apply(target, self)
		#stat_system.damage(damage_value)
		#return
	#
	#var modifier_effect := p_effect as ModifierEffect
	#if not modifier_effect == null:
		#character_attributes.apply(p_effect)
		#_logger.info("Applying effect: {} {}", [p_effect.value, p_effect.get_modifier()])
		#return
	#
	#_logger.error("Unhandled Effect type '{}', found @ '{}'.", [
		#DebugUtils.nameof(p_effect.get_script()),
		#p_effect.resource_path
	#])

func _get_property_list() -> Array[Dictionary]:
	return [{
		"name": "flags",
		"type": TYPE_INT,
		"usage": PROPERTY_USAGE_DEFAULT | PROPERTY_USAGE_SCRIPT_VARIABLE,
		"hint": PROPERTY_HINT_FLAGS,
		"hint_string": ','.join(Flags.keys()),
	}]
