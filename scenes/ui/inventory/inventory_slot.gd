class_name InventorySlot
extends Button

signal left_pressed()
signal right_pressed()
signal hovered()
signal unhovered()


func _gui_input(p_event: InputEvent) -> void:
	if p_event is InputEventMouseButton and p_event.is_released():
		#get_viewport().set_input_as_handled()
		match p_event.button_index:
			MOUSE_BUTTON_LEFT:
				left_pressed.emit()
			MOUSE_BUTTON_RIGHT:
				right_pressed.emit()


func update(p_item_stack: ItemStack) -> void:
	if p_item_stack == null or p_item_stack.item == null:
		_set_border_color(Color.BLACK)
		
		icon = null
		$Quantity.text = ""
		return
	
	_set_border_color(p_item_stack.item.get_rarity_color())
	
	icon = p_item_stack.item.icon
	if p_item_stack.quantity <= 1:
		$Quantity.text = ""
	else:
		$Quantity.text = str(p_item_stack.quantity)


func select() -> void:
	modulate = Color(1.0, 1.0, 1.0, 0.5)


func unselect() -> void:
	modulate = Color(1.0, 1.0, 1.0, 1.0)


func _set_border_color(p_color: Color) -> void:
	var stylebox: StyleBoxFlat = $Border.get_theme_stylebox("panel")
	
	stylebox.border_color = p_color


func _on_mouse_entered() -> void:
	hovered.emit()


func _on_mouse_exited() -> void:
	unhovered.emit()


func _get_inventory_view() -> InventoryView:
	return get_parent().get_parent() as InventoryView


func get_item_stack() -> ItemStack:
	return _get_inventory_view().inventory.get_item_stack(get_index())
