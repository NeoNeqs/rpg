class_name ObjInfo

static func for_vi(p_vi: VisualInstance3D) -> String:
	return "Base RID: %d, Instance RID: %d, Object id: %d" % [
			p_vi.get_base().get_id(),
			p_vi.get_instance().get_id(),
			p_vi.get_instance_id()
	]
