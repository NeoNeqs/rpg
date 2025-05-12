class_name UI
extends Control

signal empty_region_pressed()

@export var inventory_manager: InventoryManager

func _gui_input(p_event: InputEvent) -> void:
	var mouse_event := p_event as InputEventMouseButton
	if not mouse_event == null:
		if mouse_event.button_index == MOUSE_BUTTON_LEFT and mouse_event.is_released():
			empty_region_pressed.emit()
			#if inventory_manager._is_selected() and inventory_manager.selected_inventory_view.inventory.editable:
				## TODO: prompt for deletion with a confirmation dialog 
				#inventory_manager.delete_selected()
