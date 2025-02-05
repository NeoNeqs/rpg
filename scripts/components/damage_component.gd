@tool
class_name DamageComponent
extends ItemComponent

enum WeaponType {
	Mace,
	SpellBook,
	Dagger,
	Sword,
}

@export var damage_type: CombatSystem.DamageType = CombatSystem.DamageType.Physical
@export var weapon_type: WeaponType = WeaponType.Mace
@export var min_damage: int = 0
@export var max_damage: int = 1
@export_range(1.5, 4.0, 0.1) var speed: float = 1.5


func is_allowed(p_other: ItemComponent) -> bool:
	if p_other is DamageComponent:
		prints(weapon_type, p_other.weapon_type)
	return (
		p_other is DamageComponent and
		weapon_type == p_other.weapon_type
	)


func get_tooltip() -> String:
	var tooltip := "%d - %d %s damage #right# %0.2f speed" % [
		min_damage, max_damage, CombatSystem.DamageType.keys()[damage_type],
		speed
	]
	
	return tooltip
