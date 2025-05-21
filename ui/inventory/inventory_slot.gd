#class_name InventorySlot
extends Control
#
#signal left_pressed()
#signal right_pressed()
#signal hovered()
#signal unhovered()
#
#@export var text_holder: Label
#@export var icon_holder: TextureRect
#
#
#func _gui_input(p_event: InputEvent) -> void:
	#if p_event is InputEventMouseButton and p_event.is_released():
		#match p_event.button_index:
			#MOUSE_BUTTON_LEFT:
				#left_pressed.emit()
			#MOUSE_BUTTON_RIGHT:
				#right_pressed.emit()
#
#
#func set_on_cooldown(p_cooldown_in_usec: int) -> void:
	#var cooldown: Cooldown = _get_cooldown_display()
	#if cooldown == null:
		#return
	#
	#cooldown.start(p_cooldown_in_usec)
#
#
#func reset_cooldown() -> void:
	#var cooldown: Cooldown = _get_cooldown_display()
	#if cooldown == null:
		#return
	#
	#cooldown.reset()
#
#
#func _get_cooldown_display() -> Cooldown:
	#var cooldown: Cooldown = icon_holder.get_node_or_null("Cooldown")
	#if cooldown == null:
		#Logger.ui.error(
			#"Slot id='{}', index='{}' does not have a cooldown node attached.",
			#[get_instance_id(), get_index()]
		#)
		#return null
	#
	#return cooldown
#
#func update(_p_item_stack: ItemStack) -> void:
	#assert(false, "Do not call this method.")
#
#
#func select() -> void:
	#modulate = Color(1.0, 1.0, 1.0, 0.5)
#
#
#func unselect() -> void:
	#modulate = Color(1.0, 1.0, 1.0, 1.0)
#
#
#func _on_mouse_entered() -> void:
	#hovered.emit()
#
#
#func _on_mouse_exited() -> void:
	#unhovered.emit()
