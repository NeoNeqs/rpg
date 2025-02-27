## Base / abstract class for all inventories.
class_name Inventory
extends Resource

signal size_changed()
signal items_changed()

enum ActionResult {
	NotAllowed,
	Allowed,
	Invalid,
	NoLeftover,
}


func get_size() -> int:
	assert(false, "Do not call this method.")
	return -1

func is_empty_at(p_index: int) -> bool:
	assert(false, "Do not call this method.")
	return false


func handle_item_action(p_from: int, p_to_inv: Inventory, p_to: int, p_single: bool) -> ActionResult:
	## selected -> pressed
	##
	## SpellBook -> Hotbar
	## Hotbar -> None
	## ItemInv -> Hotbar
	## ItemInv -> ItemInv
	if self is ItemInventory and p_to_inv is ItemInventory:
		return ActionResult.Allowed
	
	return ActionResult.NotAllowed


func get_element_at(p_index: int) -> Resource:
	assert(false, "Do not call this method.")
	return null
