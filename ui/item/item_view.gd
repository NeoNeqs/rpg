class_name ItemView
extends InventoryView


func _setup_container() -> void:
	if container is GridContainer:
		(container as GridContainer).columns = inventory.columns


func _make_slot(p_item_stack: ItemStack) -> InventorySlot:
	var slot: InventorySlot = slot_scene.instantiate()
	slot.update(p_item_stack)
	
	slot.left_pressed.connect(slot_pressed.emit.bind(self, slot, false))
	slot.right_pressed.connect(slot_pressed.emit.bind(self, slot, true))
	slot.hovered.connect(slot_hovered.emit.bind(slot))
	slot.unhovered.connect(slot_unhovered.emit)
	
	return slot
