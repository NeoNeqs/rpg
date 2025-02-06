class_name UIELement
extends PanelContainer

var _drag_enabled: bool = false

func _gui_input(p_event: InputEvent) -> void:
	if p_event is InputEventMouseButton:
		if p_event.button_index == MOUSE_BUTTON_LEFT:
			_drag_enabled = p_event.is_pressed()
		
	elif p_event is InputEventMouseMotion and _drag_enabled:
		position += p_event.relative
		position = position.clamp(
			Vector2.ZERO,
			get_viewport_rect().size - size
		)
