class_name UI
extends Control


@export var inventory_manager: InventoryManager

func _gui_input(p_event: InputEvent) -> void:
	if p_event is InputEventMouseButton:
		var mouse_event := p_event as InputEventMouseButton
		if mouse_event.button_index == MOUSE_BUTTON_LEFT and mouse_event.is_released():
			if inventory_manager._is_selected() and inventory_manager.selected_inventory_view.inventory.editable:
				inventory_manager.delete_selected()
