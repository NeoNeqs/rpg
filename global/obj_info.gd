class_name ObjInfo

static func for_vi(p_vi: VisualInstance3D) -> String:
	return "Base RID: %d, Instance RID: %d, Object id: %d" % [
			p_vi.get_base().get_id(),
			p_vi.get_instance().get_id(),
			p_vi.get_instance_id()
	]

static func nameof(p_obj: Variant) -> StringName:
	var stringified: String = str(p_obj)
	
	# Godot's built-in type
	if stringified.contains("GDScriptNativeClass"):
		var built_in_name: String = str(p_obj.new())
		return built_in_name.substr(1, built_in_name.find('#', 1) - 1)
	
	# User defined type
	var _class_name: String = p_obj.get_global_name()
	
	if not _class_name.is_empty():
		return _class_name
	
	# No class_name means it's a normal class and 
	# those don't have their names exposed
	return p_obj.get_instance_base_type()
