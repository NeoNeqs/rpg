class_name StatSystem
extends Node

signal on_death()

@export var attribute_system: AttributeSystem

@export var armor_curve: Curve
@export var armor_penetration_curve: Curve

@export var shadow_resistance_curve: Curve
@export var shadow_penetration_curve: Curve

var current_health: int

const STAMINA_TO_HEALTH: int = 10

func _ready() -> void:
	current_health = get_max_health()


func damage(p_value: float) -> void:
	var test_health := int(current_health - p_value)
	if test_health <= 0:
		on_death.emit()
		return
	
	Logger.combat.info("Dealt {} damage.", [p_value])
	current_health = test_health


func get_max_health() -> int:
	return attribute_system.get_stamina() * STAMINA_TO_HEALTH


func get_physical_damage_reduction() -> float:
	return armor_curve.sample(attribute_system.get_armor())


func get_physical_damage_penetration() -> float:
	return armor_penetration_curve.sample(attribute_system.get_armor_penetration())


func get_shadow_damage_reduction() -> float:
	return shadow_penetration_curve.sample(
		attribute_system.get_shadow_resistance()
	)


func get_shadow_damage_penetration() -> float:
	return shadow_penetration_curve.sample(
		attribute_system.get_shadow_penetration()
	)
