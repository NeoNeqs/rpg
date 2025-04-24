class_name Tooltip
extends PanelContainer

const s_offset := Vector2(20, 0)


func _process(delta: float) -> void:
	if not visible:
		return
	
	var mouse_pos: Vector2 = get_viewport().get_mouse_position()
	var new_pos: Vector2 = mouse_pos + s_offset
	var test_bounds: Vector2 = new_pos + size
	
	if test_bounds.x > get_viewport_rect().size.x:
		new_pos.x = mouse_pos.x - size.x - s_offset.x
	if test_bounds.y > get_viewport_rect().size.y:
		new_pos.y = mouse_pos.y - size.y - s_offset.y
	
	global_position = new_pos


func update(p_item_stack: ItemStack) -> void:
	if not p_item_stack or not p_item_stack.item:
		%Tooltip.clear()
		visible = false
		set_process(false)
		return
	
	%Tooltip.update(p_item_stack.item.get_tooltip())
	
	# HACK: force minimal size
	size = Vector2.ZERO
	visible = true
	set_process(true)
