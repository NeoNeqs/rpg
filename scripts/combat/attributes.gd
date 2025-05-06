class_name Attributes
extends Resource


signal value_changed(p_attribute: StringName, p_delta: float)

static var _attributes: Array[StringName] = []


@export var strength: int = 0:
	set(new_value):
		value_changed.emit(&"strength", new_value - strength)
		strength = new_value

@export var dexterity: int = 0:
	set(new_value):
		value_changed.emit(&"dexterity", new_value - dexterity)
		dexterity = new_value

@export var intelligence: int = 0: # the clever usage of mana
	set(new_value):
		value_changed.emit(&"intelligence", new_value - intelligence)
		intelligence = new_value

@export var wisdom: int = 0: # mana capacity
	set(new_value):
		value_changed.emit(&"wisdom", new_value - wisdom)
		wisdom = new_value

@export var stamina: int = 0:
	set(new_value):
		value_changed.emit(&"stamina", new_value - stamina)
		stamina = new_value

@export var spirit: int = 0:
	set(new_value):
		value_changed.emit(&"spirit", new_value - spirit)
		spirit = new_value


@export_group("Attribute Multipliers")
@export var strength_multiplier: float = 1.0:
	set(new_value):
		value_changed.emit(&"strength_multiplier", new_value - strength_multiplier)
		strength_multiplier = new_value

@export var dexterity_multiplier: float = 1.0:
	set(new_value):
		value_changed.emit(&"dexterity_multiplier", new_value - dexterity_multiplier)
		dexterity_multiplier = new_value

@export var intelligence_multiplier: float = 1.0:
	set(new_value):
		value_changed.emit(&"intelligence_multiplier", new_value - intelligence_multiplier)
		intelligence_multiplier = new_value

@export var wisdom_multiplier: float = 1.0:
	set(new_value):
		value_changed.emit(&"wisdom_multiplier", new_value - wisdom_multiplier)
		wisdom_multiplier = new_value

@export var stamina_multiplier: float = 1.0:
	set(new_value):
		value_changed.emit(&"stamina_multiplier", new_value - stamina_multiplier)
		stamina_multiplier = new_value

@export var spirit_multiplier: float = 1.0:
	set(new_value):
		value_changed.emit(&"spirit_multiplier", new_value - spirit_multiplier)
		spirit_multiplier = new_value


@export_group("Armor", "armor_")
@export var armor: int = 0:
	set(new_value):
		value_changed.emit(&"armor", new_value - armor)
		armor = new_value

@export var armor_multiplier: float = 1.0:
	set(new_value):
		value_changed.emit(&"armor_multiplier", new_value - armor_multiplier)
		armor_multiplier = new_value

#@export var armor_curve: Curve:
	#set(new_value):
		#value_changed.emit(&"armor_curve", new_value - armor_curve)
		#armor_curve = new_value

@export var armor_penetration: int = 0:
	set(new_value):
		value_changed.emit(&"armor_penetration", new_value - armor_penetration)
		armor_penetration = new_value

@export var armor_penetration_multiplier: float = 1.00:
	set(new_value):
		value_changed.emit(&"armor_penetration_multiplier", new_value - armor_penetration_multiplier)
		armor_penetration_multiplier = new_value

#@export var armor_penetration_curve: Curve:
	#set(new_value):
		#value_changed.emit(&"armor_penetration_curve", new_value - armor_penetration_curve)
		#armor_penetration_curve = new_value


@export_group("Shadow Resistance", "shadow_")
@export var shadow_resistance: int = 0:
	set(new_value):
		value_changed.emit(&"shadow_resistance", new_value - shadow_resistance)
		shadow_resistance = new_value

@export var shadow_resistance_multiplier: float = 1.0:
	set(new_value):
		value_changed.emit(&"shadow_resistance_multiplier", new_value - shadow_resistance_multiplier)
		shadow_resistance_multiplier = new_value

#@export var shadow_resistance_curve: Curve:
	#set(new_value):
		#value_changed.emit(&"shadow_resistance_curve", new_value - shadow_resistance_curve)
		#shadow_resistance_curve = new_value

@export var shadow_penetration: int = 0:
	set(new_value):
		value_changed.emit(&"shadow_penetration", new_value - shadow_penetration)
		shadow_penetration = new_value

@export var shadow_penetration_multiplier: float = 1.0:
	set(new_value):
		value_changed.emit(&"shadow_penetration_multiplier", new_value - shadow_penetration_multiplier)
		shadow_penetration_multiplier = new_value

#@export var shadow_penetration_curve: Curve:
	#set(new_value):
		#value_changed.emit(&"shadow_penetration_curve", new_value - shadow_penetration_curve)
		#shadow_penetration_curve = new_value


@export_group("Nature Resistance", "nature_")
@export var nature_resistance: int = 0:
	set(new_value):
		value_changed.emit(&"nature_resistance", new_value - nature_resistance)
		nature_resistance = new_value

@export var nature_resistance_multiplier: float = 1.0:
	set(new_value):
		value_changed.emit(&"nature_resistance_multiplier", new_value - nature_resistance_multiplier)
		nature_resistance_multiplier = new_value

#@export var nature_resistance_curve: Curve:
	#set(new_value):
		#value_changed.emit(&"nature_resistance_curve", new_value - nature_resistance_curve)
		#nature_resistance_curve = new_value

@export var nature_penetration: int = 0:
	set(new_value):
		value_changed.emit(&"nature_penetration", new_value - nature_penetration)
		nature_penetration = new_value

@export var nature_penetration_multiplier: float = 1.0:
	set(new_value):
		value_changed.emit(&"nature_penetration_multiplier", new_value - nature_penetration_multiplier)
		nature_penetration_multiplier = new_value

#@export var nature_penetration_curve: Curve:
	#set(new_value):
		#value_changed.emit(&"nature_penetration_curve", new_value - nature_penetration_curve)
		#nature_penetration_curve = new_value

#func apply(p_effect: ModifierEffect) -> void:
	#var modifier_name: StringName = p_effect.get_modifier()
	#
	## NOTE: Currently this is left untyped since not all attributes might be int
	#var current_value: Variant = get(modifier_name)
	#set(modifier_name, current_value + p_effect.value)
#
#
#func remove(p_effect: ModifierEffect) -> void:
	#var modifier_name: StringName = p_effect.get_modifier()
	#
	## NOTE: Currently this is left untyped since not all attributes might be int
	#var current_value: Variant = get(modifier_name)
	#set(modifier_name, current_value - p_effect.value)


func get_stamina() -> int:
	return int(stamina * stamina_multiplier)


func get_strength() -> int:
	return int(strength * strength_multiplier)

func get_armor() -> int:
	return int(armor * armor_multiplier)


func get_armor_penetration() -> int:
	return int(armor_penetration * armor_penetration_multiplier)


func get_shadow_resistance() -> int:
	return int(shadow_resistance * shadow_resistance_multiplier)


func get_shadow_penetration() -> int:
	return int(shadow_penetration * shadow_penetration_multiplier)


static func _static_init() -> void:
	const IS_EXPORTED := (PROPERTY_USAGE_SCRIPT_VARIABLE | PROPERTY_USAGE_DEFAULT)
	for m: Dictionary in new().get_property_list():
		if (m["usage"] ^ IS_EXPORTED) == 0:
			_attributes.append(m["name"])


static func get_attributes_as_hint_string() -> String:
	return ','.join(_attributes)
