class_name AttributeSystem
extends Node


var _total_attributes := Attributes.new()


func link(p_attributes: Attributes) -> void:
	if p_attributes.value_changed.is_connected(_on_attribute_changed):
		return
	
	p_attributes.value_changed.connect(_on_attribute_changed)


func unlink(p_attributes: Attributes) -> void:
	for attr: StringName in Attributes._attributes:
		_total_attributes.set(attr, _total_attributes.get(attr) - p_attributes.get(attr))
	p_attributes.value_changed.disconnect(_on_attribute_changed)

#region Proxy methods

func get_stamina() -> int:
	return _total_attributes.get_stamina()


func get_armor() -> int:
	return _total_attributes.get_armor()


func get_armor_penetration() -> int:
	return _total_attributes.get_armor_penetration()


func get_shadow_resistance() -> int:
	return _total_attributes.get_shadow_resistance()


func get_shadow_penetration() -> int:
	return _total_attributes.get_shadow_penetration()

#endregion


func _on_attribute_changed(p_attribute: StringName, p_delta: int) -> void:
	var current_total: int = _total_attributes.get(p_attribute)
	
	_total_attributes.set(p_attribute, current_total + p_delta)
