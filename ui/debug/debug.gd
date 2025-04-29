extends Control


func _ready() -> void:
	get_parent().visible = false


func _input(event: InputEvent) -> void:
	var key_event := event as InputEventKey
	if not key_event == null:
		if key_event.keycode == KEY_QUOTELEFT and key_event.is_pressed():
			get_parent().visible = not get_parent().visible
			$DebugConsole.focus()
			DisplayServer.mouse_set_mode(DisplayServer.MOUSE_MODE_VISIBLE)
			get_viewport().set_input_as_handled()
