class_name SpellInventory
extends Inventory

@export var _spells: Array[Spell] = []


func get_element_at(p_index: int) -> Spell:
	if p_index < 0 or p_index >= _spells.size():
		return null
	
	return _spells[p_index]


func handle_item_action(p_from: int, p_to_inv: Inventory, p_to: int, p_single: bool) -> ActionResult:
	return ActionResult.NoLeftover

func set_spell(p_index: int, p_spell: Spell) -> void:
	pass 


func is_empty_at(p_index: int) -> bool:
	return _spells[p_index] == null


func get_size() -> int:
	return _spells.size()
