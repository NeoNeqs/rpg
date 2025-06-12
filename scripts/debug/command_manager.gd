#class_name CommandManager 
#extends Node
#
#var _registered_commands: Dictionary[String, Array] = {}
#var _commands: Array[CommandContainer] = []
#
#const COMMAND_PREFIX: String = "command_"
#
#enum Validation {
	#Valid,
	#InvalidName,
	#InvalidArgC,
#}
#
#func _ready() -> void:
	#_register_commands()
#
#
#func _register_commands() -> void:
	#for container: CommandContainer in _commands:
		#_register_container_commands(container)
#
#
#func invoke(p_name: String, p_args: Array[String]) -> Variant:
	#p_name = COMMAND_PREFIX + p_name
	#
	#var container: CommandContainer = _registered_commands[p_name][0]
	#
	#return container.callv(p_name, p_args)
#
#
#func register_container(p_command_container: CommandContainer) -> bool:
	#if p_command_container in _commands:
		##Logger.core.warn(
			##"Command container '{}' is already registered.", 
			##[p_command_container.name]
		##)
		#return false
	#
	#_commands.append(p_command_container)
	#return true
#
#
#func get_min_argc(p_name: String) -> int:
	#p_name = COMMAND_PREFIX + p_name
	#
	#return _registered_commands[p_name][1]
#
#
#func get_max_argc(p_name: String) -> int:
	#p_name = COMMAND_PREFIX + p_name
	#
	#return _registered_commands[p_name][2]
#
#
#func _register_container_commands(p_container: CommandContainer) -> void:
	#add_child(p_container)
	#
	#for m: Dictionary in p_container.get_method_list():
		#var _name: String = m["name"]
		#var min_argc: int = m["args"].size() - m["default_args"].size()
		#var max_argc: int = m["args"].size()
		#
		#if not _name.begins_with(COMMAND_PREFIX):
			#continue
			#
		##if _name in _registered_commands:
			##Logger.core.error(
				##"Command '{}' is already registerd, probably by another container.",
				##[_name]
			##)
		#
		#_registered_commands[_name] = [p_container, min_argc, max_argc]
#
#
#func is_valid_command(p_name: String, p_argc: int) -> Validation:
	#p_name = COMMAND_PREFIX + p_name
	#
	#if not p_name in _registered_commands:
		#return Validation.InvalidName
	#
	#if not p_argc in _registered_commands[p_name]:
		#return Validation.InvalidArgC
#
	#return Validation.Valid
