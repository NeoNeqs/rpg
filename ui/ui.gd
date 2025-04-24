class_name UI
extends Control



func _gui_input(p_event: InputEvent) -> void:
	if p_event is InputEventMouseButton:
		if p_event.button_index == MOUSE_BUTTON_LEFT and p_event.is_released():
			if $InventoryManager/DragSlot.visible:
				print("DELETE ITEM")
			
