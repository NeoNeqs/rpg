@tool
class_name Clock
extends RefCounted

var _calle: Object
var _start_time: int
var _section: String


func _init(p_calle: Object, p_section: String = "") -> void:
	_calle = p_calle
	_section = p_section
	_start_time = Time.get_ticks_usec()


func start() -> void:
	_start_time = Time.get_ticks_usec()


func stop() -> void:
	var end := Time.get_ticks_usec()
	print("{}::{} took {}".format([
			DebugUtils.nameof(_calle.get_script()),
			_section,
			DebugUtils.humnaize_time(end - _start_time)
	], "{}"))
