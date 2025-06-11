#@tool
#class_name DamageEffect
#extends Effect
#
#enum DamageType {
	#Physical,
	#Shadow,
	#Nature,
	#Fire,
#}
#
#@export var damage_type: DamageType
#
#func _init() -> void:
	#pass
#
#
#func _apply(p_attacker: CombatManager, p_target: CombatManager) -> void:
	#var damage: int = _apply_to_stats(p_attacker.stat_system, p_target.stat_system)
	#_logger.info("Dealing {} {} damage", [damage, DamageType.keys()[damage_type]])
	#p_target.combat_resources.take_damage(damage)
	#
	#
#
#func _apply_to_stats(p_attacker: StatSystem, p_target: StatSystem) -> int:
	#var damage: int
	#
	#match damage_type:
		#DamageType.Physical:
			#damage = _calc_physical_damage(p_attacker, p_target)
	#
	#return damage
#
#
#func _calc_physical_damage(p_attacker: StatSystem, p_target: StatSystem) -> int:
	#var damage: float = value
	#
	#var damage_reduction: float = p_target.get_physical_damage_reduction()
	#
	#if not p_attacker == null:
		#damage_reduction += p_attacker.get_physical_damage_penetration()
		#damage += p_attacker.get_physical_damage()
	#
	#return int(damage * damage_reduction)
