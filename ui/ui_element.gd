class_name UIELement
extends PanelContainer

var _drag_enabled: bool = false

func _gui_input(p_event: InputEvent) -> void:
	var mouse_button_event := p_event as InputEventMouseButton
	if not mouse_button_event == null:
		if mouse_button_event.button_index == MOUSE_BUTTON_LEFT:
			_drag_enabled = mouse_button_event.is_pressed()
	
	var mouse_motion_event := p_event as InputEventMouseMotion
	if not mouse_motion_event == null and _drag_enabled:
		position += p_event.relative
		position = position.clamp(
			Vector2.ZERO,
			get_viewport_rect().size - size
		)
