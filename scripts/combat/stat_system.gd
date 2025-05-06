class_name StatSystem
extends Resource


var attribute_system: AttributeSystem

@export var armor_curve: Curve
@export var armor_penetration_curve: Curve
@export var stamina_curve: Curve
@export var strength_curve: Curve

const STAMINA_TO_HEALTH_MULITPLIER: int = 10

func get_max_health() -> int:
	return int(stamina_curve.sample(attribute_system.get_stamina()))
	#return attribute_system.get_stamina() * STAMINA_TO_HEALTH_MULITPLIER


func get_physical_damage_reduction() -> float:
	return armor_curve.sample(attribute_system.get_armor())


func get_physical_damage_penetration() -> float:
	return armor_penetration_curve.sample(attribute_system.get_armor_penetration())


func get_physical_damage() -> float:
	return strength_curve.sample(attribute_system.get_strength())
