extends LineEdit

signal up_arrow_pressed
signal down_arrow_pressed


func _gui_input(p_event: InputEvent) -> void:
	var key_event := p_event as InputEventKey
	
	if not key_event == null:
		if key_event.is_pressed():
			if key_event.keycode == KEY_UP:
				up_arrow_pressed.emit()
			elif key_event.keycode == KEY_DOWN:
				down_arrow_pressed.emit()
