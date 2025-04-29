extends PanelContainer

enum Validation {
	Valid,
	InvalidName,
	InvalidArgC,
}

@export var _line_edit: LineEdit
@export var _display: RichTextLabel

const COMMAND_PREFIX: String = "command_"

static var commands: Dictionary[String, Array] = {}

func _ready() -> void:
	_register_commands()


func focus() -> void:
	_line_edit.grab_focus()


func _register_commands() -> void:
	for m: Dictionary in get_method_list():
		var _name: String = m["name"]
		var min_argc: int = m["args"].size() - m["default_args"].size()
		var max_argc: int = m["args"].size()
		
		if _name.begins_with(COMMAND_PREFIX):
			commands[_name] = range(min_argc, max_argc + 1)


func _on_command_entered(new_text: String) -> void:
	_line_edit.clear()
	var tokens: PackedStringArray = new_text.split(' ', false)
	if tokens.size() == 0:
		return
	
	var method_name: String = COMMAND_PREFIX + tokens[0]
	var result: Validation = _is_valid_command(method_name, tokens.size() - 1)
	
	if result == Validation.InvalidName:
		Logger.core.error("Unknown command '{}'.", [tokens[0]])
		return
	elif result == Validation.InvalidArgC:
		Logger.core.error(
			"Command '{}' takes min of '{}' and max of '{}' args, but '{}' were supplied.", 
			[tokens[0], commands[method_name][0], commands[method_name][-1], tokens.size() - 1]
		)
		return
	
	callv(method_name, tokens.slice(1))
	_display.update()

func _is_valid_command(p_name: String, p_argc: int) -> Validation:
	if not p_name in commands:
		return Validation.InvalidName
	
	if not p_argc in commands[p_name]:
		return Validation.InvalidArgC
	
	return Validation.Valid


func command_ins(p_obj_id: String, p_property: String = "") -> void:
	command_inspect(p_obj_id, p_property)


func command_inspect(p_what: String, p_property: String = "") -> void:
	var obj: Object = instance_from_id(int(p_what))
	if not obj == null:
		if p_property.is_empty():
			_inspect(obj)
		else:
			Logger.core.info("{} = {}", [p_property, obj.get(p_property)])
		
		return
	
	var node_path := NodePath(p_what)
	var data: Array = get_node_and_resource(node_path)
	if not data[0] == null:
		_inspect(data[0])
		if not data[1] == null:
			_inspect(data[1])
		else:
			var sub_property: String = str(data[2]).substr(1)
			Logger.core.info("{} = {}", [sub_property, data[0].get(sub_property)])
		return
	
	Logger.core.error("No object found with id '{}'.", [p_what])


func _inspect(p_object: Object) -> void:
	var _base: String = ""
	var _name: String = ""
	var _script_variables: Array[String] = []
	
	for p: Dictionary in p_object.get_property_list():
		if p["usage"] == PROPERTY_USAGE_CATEGORY:
			_base = _name
			_name = p["name"]
		elif not (p["usage"] & PROPERTY_USAGE_SCRIPT_VARIABLE) == 0:
			if p["type"] == TYPE_OBJECT:
				_script_variables.append(
					"%s: %s = %s" % [p["name"], p["hint_string"], p_object.get(p["name"])]
				)
			else:
				_script_variables.append(
					"%s: %s = %s" % [p["name"], type_string(p["type"]), p_object.get(p["name"])]
				)
	
	Logger.core.info("{} ({})", [_name, _base])
	for sv: String in _script_variables:
		Logger.core.info(sv)
