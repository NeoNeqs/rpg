class_name Attributes
extends Resource

@export var strength: int = 0
@export var dexterity: int = 0
@export var intelligence: int = 0 # the clever usage of mana
@export var wisdom: int = 0 # mana capacity
@export var stamina: int = 0
@export var spirit: int = 0


@export_group("Physical Defense", "armor_")
@export var armor: int = 0
@export var armor_multiplier: float = 1.0
@export var armor_penetration: int = 0
@export var armor_penetration_multiplier: float = 0


@export_group("Shadow Defense", "shadow_")
@export var shadow_resistance: int = 0
@export var shadow_resistance_multiplier: float = 1.0
@export var shadow_penetration: int = 0
@export var shadow_penetration_multiplier: float = 0


@export_group("Stats Multipliers")
@export var strength_multiplier: int = 0
@export var dexterity_multiplier: int = 0
@export var intelligence_multiplier: int = 0
@export var wisdom_multiplier: int = 0
@export var stamina_multiplier: int = 0
@export var spirit_multiplier: int = 0


@export_group("Internal")
@export var haste: int = 0


const HEALTH_MULTIPLIER: int = 10
const MANA_MULTIPLIER: int = 15

const ARMOR_CAP: int = 2000
const RESISTANCE_CAP: int = 2000
const PENETRATION_CAP: int = 2000

func apply(p_effect: ModifierEffect) -> void:
	var modifier_name: StringName = p_effect.get_modifier()
	
	# NOTE: Currently this is left untyped since not all attributes might be int
	var current_value: Variant = get(modifier_name)
	set(modifier_name, current_value + p_effect.value)


func remove(p_effect: ModifierEffect) -> void:
	var modifier_name: StringName = p_effect.get_modifier()
	
	# NOTE: Currently this is left untyped since not all attributes might be int
	var current_value: Variant = get(modifier_name)
	set(modifier_name, current_value - p_effect.value)


func get_max_health() -> int:
	return stamina * HEALTH_MULTIPLIER


func get_max_mana() -> int:
	return wisdom * MANA_MULTIPLIER


func get_physical_damage_reduction() -> float:
	# when get_real_armor() == ARMOR_CAP this effectively gives 25 % dmg reduction
	return 1.0 - (get_real_armor() / (ARMOR_CAP * 4.0))


func get_physical_damage_penetration() -> float:
	return get_real_armor_penetration() / (PENETRATION_CAP * 6.0)


func get_shadow_damage_reduction() -> float:
	return 1.0 - (get_real_shadow_resistance() / (RESISTANCE_CAP * 4.0))


func get_shadow_damage_penetration() -> float:
	return get_real_shadow_penetration() / (PENETRATION_CAP * 6.0)


func get_real_armor() -> int:
	return clampi(int(armor * armor_multiplier), 0, ARMOR_CAP)

func get_real_armor_penetration() -> int:
	return clampi(int(armor_penetration * armor_penetration_multiplier), 0, PENETRATION_CAP)

func get_real_shadow_resistance() -> int:
	return clampi(int(shadow_resistance * shadow_resistance_multiplier), 0, RESISTANCE_CAP)


func get_real_shadow_penetration() -> int:
	return clampi(int(shadow_penetration * shadow_resistance_multiplier), 0, PENETRATION_CAP)


static func get_attributes_as_hint_string() -> String:
	const EXPORT_USAGE: int = PROPERTY_USAGE_SCRIPT_VARIABLE | PROPERTY_USAGE_DEFAULT
	
	var attribute_names := PackedStringArray()
	var dummy := Attributes.new()

	for prop: Dictionary in dummy.get_property_list():
		if not (prop["usage"] & EXPORT_USAGE) == EXPORT_USAGE:
			continue
		attribute_names.append(prop["name"])
	
	return ','.join(attribute_names)
