extends PanelContainer

@export var _line_edit: LineEdit
@export var _display: RichTextLabel

var command_manager := CommandManager.new()

var command_history := CommandHistory.new(1000)

func _enter_tree() -> void:
	add_child(command_manager)
	command_manager.register_container(InspectCommands.new())


func _exit_tree() -> void:
	command_manager.queue_free()


func focus() -> void:
	_line_edit.grab_focus()


func _on_command_entered(new_text: String) -> void:
	command_history.add(new_text)
	Logger.core.info(new_text)
	_line_edit.clear()
	var tokens: PackedStringArray = new_text.split(' ', false)
	if tokens.size() == 0:
		return
	
	var args: Array[String] = []
	for i: int in Vector2i(1, tokens.size()):
		var token: String = tokens[i]
		if token.strip_edges().ends_with('"') and args.size() > 0 and args[-1].begins_with('"'):
			args[-1] += " " + token
		else:
			args.append(token)
		
	for j: int in args.size():
		if args[j][0] == '"' and args[j][-1] == '"':
			args[j] = args[j].substr(1, args[j].length() - 2)
	var argc: int = args.size()
	
	var result: CommandManager.Validation = command_manager.is_valid_command(
		tokens[0], 
		argc
	)
	
	if result == CommandManager.Validation.InvalidName:
		Logger.core.error("Unknown command '{}'.", [tokens[0]])
		return
	
	if result == CommandManager.Validation.InvalidArgC:
		Logger.core.error(
			"Command '{}' takes min of '{}' and max of '{}' args, but '{}' were supplied.", 
			[
				tokens[0], 
				command_manager.get_min_argc(tokens[0]),
				command_manager.get_max_argc(tokens[0]),
				argc
			]
		)
		return
	
	var return_value: Variant = command_manager.invoke(tokens[0], args)
	if not return_value == null:
		Logger.core.info("{}", [return_value])
	_display.update()


func _on_line_edit_up_arrow_pressed() -> void:
	_line_edit.text = command_history.travel_back()
	await get_tree().physics_frame
	_line_edit.caret_column = _line_edit.text.length()


func _on_line_edit_down_arrow_pressed() -> void:
	_line_edit.text = command_history.travel_forward()
	await get_tree().physics_frame
	_line_edit.caret_column = _line_edit.text.length()


class CommandHistory extends RefCounted:
	var _history: PackedStringArray
	
	var _size: int
	var _start_index: int
	var _current_index: int
	var _last_placed_index: int
	
	func _init(p_size: int) -> void:
		_size = p_size
		_start_index = 0
		_current_index = 0
		_last_placed_index = -1
		_history = PackedStringArray()
		_history.resize(p_size)
	
	
	func add(p_command: String) -> void:
		var placement_index: int = _wrapped(_last_placed_index + 1)
		
		if not _history[placement_index].is_empty():
			_start_index = placement_index
		
		_history[placement_index] = p_command
		_last_placed_index = placement_index
		
		# NOTE: Do not replace this call with `placement_index`, lol
		_current_index = _wrapped(_last_placed_index + 1)
	
	
	func _wrapped(p_index: int) -> int:
		return wrap(p_index, 0, _history.size())
	
	
	func travel_back() -> String:
		var prev_index: int = _wrapped(_current_index - 1)
		if not _history[prev_index].is_empty():
			_current_index = _wrapped(_current_index - 1)
		
		return _history[_current_index]
	
	
	func travel_forward() -> String:
		var next_index: int = _wrapped(_current_index + 1)
		if not _history[_current_index].is_empty():
			_current_index = next_index
		
		return _history[_current_index]
	
	
	func get_current_command() -> String:
		return _history[_current_index]
