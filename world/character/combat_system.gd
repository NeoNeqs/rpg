class_name CombatSystem
extends Node

enum DamageType {
	Physical,
	Shadow,
	Nature,
}

@export
var attributes: Attributes

var current_health: int
var current_mana: int


func _ready() -> void:
	current_health = attributes.get_max_health()
	current_mana = attributes.get_max_mana()


func apply_effect(p_effect: Effect, p_attacker: Attributes = null) -> void:
	if p_effect.apply_chance < 1:
		var chance: float = floor(randf() * 100) / 100.0
		if chance > p_effect.apply_chance:
			return
	
	# Instant effects
	if p_effect.ticks == 0:
		_apply_effect(p_effect, p_attacker)
		return
	
	if p_effect.is_immediate():
		_apply_effect(p_effect, p_attacker)
	
	var timer := Timer.new()
	timer.autostart = true
	timer.timeout.connect(_apply_periodic_effect.bind(timer, p_effect, p_attacker))
	timer.wait_time = p_effect.tick_timeout
	add_child(timer)


func damage_physical(p_effect: DamageEffect, p_attacker: Attributes) -> void:
	var damage_reduction: float = attributes.get_physical_damage_reduction()
	if p_attacker:
		damage_reduction += p_attacker.get_physical_damage_penetration()
	
	_damage(p_effect.value * damage_reduction)


func damage_shadow(p_effect: DamageEffect, p_attacker: Attributes) -> void:
	var damage_reduction: float = attributes.get_shadow_damage_reduction()
	if p_attacker:
		damage_reduction += p_attacker.get_shadow_damage_penetration()
		
	_damage(p_effect.value * damage_reduction)


func _damage(p_value: float) -> void:
	var test_health := int(current_health - p_value)
	if test_health <= 0:
		print("DEAD")
		return
	current_health = test_health


func _handle_damage_effect(p_effect: DamageEffect, p_attacker: Attributes) -> void:
	match p_effect.damage_type:
		DamageType.Physical:
			damage_physical(p_effect, p_attacker)
		DamageType.Shadow:
			damage_shadow(p_effect, p_attacker)
		_:
			Logger.cb.error("Unhandled \"{}\" case of DamageType enum.", [
				DamageType.keys()[p_effect.damage_type]
			])


func _apply_effect(p_effect: Effect, p_attacker: Attributes) -> void:
	match p_effect:
		var damage_effect when damage_effect is DamageEffect:
			_handle_damage_effect(damage_effect, p_attacker)
			print("Damage effect: %d %s" % [p_effect.value, p_effect.get_damage_name()])
		var modifier_effect when modifier_effect is ModifierEffect:
			attributes.apply(p_effect)
			print("Applying effect: %d %s" % [p_effect.value, p_effect.get_modifier()])
		_:
			Logger.cb.error("Unhandled Effect of type {}, found @ \"{}\"", [
				DebugUtils.nameof(p_effect.get_script()),
				p_effect.resource_path
			])
	

func _remove_effect(p_effect: Effect) -> void:
	if p_effect is ModifierEffect:
		attributes.remove(p_effect)
		print("Removing effect: %d %s" % [-p_effect.value * p_effect.ticks, p_effect.get_modifier()])
	elif p_effect is DamageEffect:
		pass


func _apply_periodic_effect(p_timer: Timer, p_effect: Effect, p_attacker: Attributes) -> void:
	if p_effect.tick():
		_apply_effect(p_effect, p_attacker)
		return
	
	if not p_effect.is_immediate():
		_apply_effect(p_effect, p_attacker)
	
	_remove_effect(p_effect)
	
	p_timer.stop()
	p_timer.queue_free()
