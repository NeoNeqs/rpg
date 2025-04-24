@tool
class_name DebugUtils
extends RefCounted


static func nameof(p_obj: Variant) -> StringName:
	var stringified: String = str(p_obj)
	
	if p_obj == null:
		return stringified

	# Godot's built-in type
	if stringified.contains("GDScriptNativeClass"):
		var built_in_name: String = str(p_obj.new())
		return built_in_name.substr(1, built_in_name.find("#", 1) - 1)

	# User defined type
	var _script: Script = p_obj.get_script()
	if _script == null:
		return stringified
	
	var _class_name: String = _script.get_global_name()

	if not _class_name.is_empty():
		return _class_name

	# Not a class_name defined class, which means it does not have a name
	return p_obj.get_instance_base_type()


static func humnaize_time(p_usec_time: int) -> String:
	if p_usec_time < 1_000:
		return str(p_usec_time) + "μs"

	if p_usec_time < 1_000_000:
		return str(p_usec_time / 1_000.0).pad_decimals(2) + "ms"

	if p_usec_time < 1_000_000_000:
		return str(p_usec_time / 1_000_000.0).pad_decimals(2) + "s"

	return str(p_usec_time / 1_000_000_000.0).pad_decimals(2) + "m"
