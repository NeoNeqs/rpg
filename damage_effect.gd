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

func apply(p_target: CombatSystem, p_attacker: CombatSystem) -> void:
	var damage: int
	
	match damage_type:
		CombatSystem.DamageType.Physical:
			damage = calculate_physical_damage(p_target.attributes, p_attacker.attributes)
		CombatSystem.DamageType.Shadow:
			damage = calculate_shadow_damage(p_target.attributes, p_attacker.attributes)
		CombatSystem.DamageType.Nature:
			Logger.combat.critical("TODO: implement nature damage type and others.")
		_:
			Logger.combat.error(
				"Unhandled DamageType case '{}'.", 
				CombatSystem.DamageType.keys()[damage_type]
			)
	Logger.combat.info("Damage={}", [damage])

func calculate_physical_damage(p_target: Attributes, p_attacker: Attributes) -> int:
	var damage_reduction: float = p_target.get_physical_damage_reduction() # 0.75
	if not p_attacker == null:
		damage_reduction += p_attacker.get_physical_damage_penetration()
	
	damage_reduction = clampf(damage_reduction, 0.0, 1.0)
	
	return int(value * damage_reduction)


func calculate_shadow_damage(p_target: Attributes, p_attacker: Attributes) -> int:
	var damage_reduction: float = p_target.get_shadow_damage_reduction()
	if not p_attacker == null:
		damage_reduction += p_attacker.get_shadow_damage_penetration()
	
	damage_reduction = clampf(damage_reduction, 0.0, 1.0)
	
	return int(value * damage_reduction)

#func _damage_physical(p_effect: DamageEffect, p_attacker: Attributes) -> void:
	#var damage_reduction: float = 1.0 - attributes.get_physical_damage_reduction()
	#if p_attacker:
		#damage_reduction += p_attacker.get_physical_damage_penetration()
	#
	#_damage(p_effect.value * damage_reduction)


#func _damage_shadow(p_effect: DamageEffect, p_attacker: Attributes) -> void:
	#var damage_reduction: float = 1.0 - attributes.get_shadow_damage_reduction()
	#if p_attacker:
		#damage_reduction += p_attacker.get_shadow_damage_penetration()
		#
	#_damage(p_effect.value * damage_reduction)
#

func get_damage_name() -> String:
	return CombatSystem.DamageType.keys()[damage_type]
