#@tool
##class_name AttributeEffect
#extends Effect
#
##exported through _get_property_list2
#var modifier: String
#
#func _init() -> void:
	#pass
#
#func _apply(p_attacker: CombatManager, p_target: CombatManager) -> void:
	#var current_value: Variant = p_target.base_attributes.get(modifier)
	#
	#p_target.base_attributes.set(modifier, current_value + value)
#
#
#func _remove(p_attacker: CombatManager, p_target: CombatManager) -> void:
	#super._remove(p_attacker, p_target)
#
#
#func _get_property_list() -> Array[Dictionary]:
	#return [{
		#"name": &"modifier",
		#"type": TYPE_STRING,
		#"hint": PROPERTY_HINT_ENUM,
		#"hint_string": Attributes.get_attributes_as_hint_string(),
		#"usage": PROPERTY_USAGE_SCRIPT_VARIABLE | PROPERTY_USAGE_DEFAULT
	#}]
