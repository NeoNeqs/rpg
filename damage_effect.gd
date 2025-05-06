@tool
class_name DamageEffect
extends Effect

@export
var damage_type: CombatSystem.DamageType

var _logger := Logger.combat

func _init() -> void:
	pass


func get_tooltip() -> String:
	var tooltip := "#color=04C404#" + super.get_tooltip()
	
	tooltip += "Damages a target wth %d %s damage" % [
		value, get_damage_name()
	]
	
	if not ticks == 0:
		tooltip += " every %d seconds" % tick_timeout
	
	tooltip += '.\n'
	
	return tooltip


func apply(p_target: CombatSystem, p_attacker: CombatSystem) -> int:
	var damage: int
	
	match damage_type:
		CombatSystem.DamageType.Physical:
			damage = calculate_physical_damage(p_target, p_attacker)
		
		#CombatSystem.DamageType.Shadow:
			#damage = calculate_shadow_damage(
				#p_target.attribute_system.total_attributes, 
				#p_attacker.attribute_system.total_attributes
			#)
		#
		CombatSystem.DamageType.Nature:
			_logger.critical("TODO: implement.")
		CombatSystem.DamageType.Fire:
			#_logger.critical("TODO: implement.")
			damage = int(value)
		_:
			_logger.debug(
				"Unhandled DamageType case '{}'.", 
				[CombatSystem.DamageType.keys()[damage_type]]
			)
	
	p_target.stat_system.damage(damage)
	
	return damage

func calculate_physical_damage(p_target: CombatSystem, p_attacker: CombatSystem) -> int:
	var target_stats: StatSystem = p_target.stat_system
	var damage_reduction: float = target_stats.get_physical_damage_reduction()
	
	if not p_attacker == null:
		var attacker_stats: StatSystem = p_attacker.stat_system
		damage_reduction += attacker_stats.get_physical_damage_penetration()
	
	return int(value * damage_reduction)


func calculate_shadow_damage(p_target: Attributes, p_attacker: Attributes) -> int:
	var target_stats: StatSystem = p_target.stat_system
	var damage_reduction: float = target_stats.get_shadow_damage_reduction()
	
	if not p_attacker == null:
		var attacker_stats: StatSystem = p_attacker.stat_system
		damage_reduction += attacker_stats.get_shadow_damage_penetration()
	
	return int(value * damage_reduction)


func get_damage_name() -> String:
	return CombatSystem.DamageType.keys()[damage_type]
