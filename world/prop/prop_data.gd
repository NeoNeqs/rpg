@tool
class_name PropData
extends Resource

@export
var use_triplanar: bool = false:
	set(value):
		use_triplanar = value
		emit_changed()

@export
var mesh: Mesh = null


#func get_mesh() -> Mesh:
	#assert(false, "Function is not implemented")
	#return null
