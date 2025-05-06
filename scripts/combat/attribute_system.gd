@tool
class_name AttributeSystem
extends Resource


var _total_attributes := Attributes.new()


func link(p_attributes: Attributes) -> void:
	if p_attributes.value_changed.is_connected(_on_attribute_changed):
		return
	
	for attr_name: StringName in Attributes._attributes:
		var current_total: Variant = _total_attributes.get(attr_name)
		var removed_value: Variant = p_attributes.get(attr_name)
		current_total += removed_value
		
		_total_attributes.set(attr_name, current_total)
	
	p_attributes.value_changed.connect(_on_attribute_changed)


func unlink(p_attributes: Attributes) -> void:
	if not p_attributes.value_changed.is_connected(_on_attribute_changed):
		return
	
	for attr_name: StringName in Attributes._attributes:
		var current_total: Variant = _total_attributes.get(attr_name)
		var removed_value: Variant = p_attributes.get(attr_name)
		current_total -= removed_value
		
		_total_attributes.set(attr_name, current_total)
	p_attributes.value_changed.disconnect(_on_attribute_changed)


func _on_attribute_changed(p_attribute: StringName, p_delta: int) -> void:
	var current_total: int = _total_attributes.get(p_attribute)
	
	_total_attributes.set(p_attribute, current_total + p_delta)


func get_stamina() -> int:
	return _total_attributes.get_stamina()


func get_strength() -> int:
	return _total_attributes.get_strength()


func get_armor() -> int:
	return _total_attributes.get_armor()


func get_armor_penetration() -> int:
	return _total_attributes.get_armor_penetration()
