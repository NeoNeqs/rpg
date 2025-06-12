#class_name InspectCommands
#extends CommandContainer
#
#
#func _init() -> void:
	#pass
#
#
#
#func command_call(p_what: String, p_method: String, p_args: String = "") -> Variant:
	#var obj: Object = instance_from_id(int(p_what))
	#
	#if not obj == null:
		#return obj.callv(p_method, _parse_args(p_args))
	#
	#var node_path := NodePath(p_what)
	#var node: Node
	#if p_what.is_relative_path():
		#node = get_node(node_path)
	#else:
		#node = get_tree().get_root().get_node(node_path)
	#if not node == null:
		#return node.callv(p_method, _parse_args(p_args))
	#
	#return null
#
#func _parse_args(p_args: String) -> Array:
	#var result: Array[String] = []
	#
	#var parts: PackedStringArray = p_args.split(' ', false)
	#for part: String in parts:
		#if part.strip_edges().ends_with("'") and result.size() > 0 and result[-1].begins_with("'"):
			#result[-1] += " " + part
		#else:
			#result.append(part.strip_edges())
	#
	#for i in result.size():
		#if result[i][0] == "'" and result[i][-1] == "'":
			#result[i] = result[i].substr(1, result[i].length() - 2)
	#return result
#
#
#func command_ins(p_what: String, p_property: String = "") -> void:
	#command_inspect(p_what, p_property)
#
#
#func command_inspect(p_what: String, p_property: String = "") -> void:
	#var obj: Object = instance_from_id(int(p_what))
	#if not obj == null:
		#if p_property.is_empty():
			#_inspect(obj)
		##else:
			##logger.info("{}: ", [DebugUtils.nameof(obj)])
			##logger.info("{} = {}", [p_property, obj.get(p_property)])
		#
		#return
	#
	#var node_path := NodePath(p_what)
	#var data: Array = get_tree().get_root().get_node_and_resource(node_path)
	#
	#if data[0] == null:
		##logger.error("No object found with id '{}'.", [p_what])
		#return
	#
	#_inspect(data[0])
	#
	#if not data[1] == null:
		#_inspect(data[1])
	#elif not str(data[2]).is_empty():
		#var sub_property: String = str(data[2]).substr(1)
		##logger.info("{} = {}", [sub_property, data[0].get(sub_property)])
#
#
#func _inspect(p_object: Object) -> void:
	#var _base: String = ""
	#var _name: String = ""
	#var _script_variables: Array[String] = []
	#
	#for p: Dictionary in p_object.get_property_list():
		#if p["usage"] == PROPERTY_USAGE_CATEGORY:
			#_base = _name
			#_name = p["name"]
		#elif not (p["usage"] & PROPERTY_USAGE_SCRIPT_VARIABLE) == 0:
			#if p["type"] == TYPE_OBJECT:
				#_script_variables.append(
					#"%s: %s = %s" % [p["name"], p["hint_string"], p_object.get(p["name"])]
				#)
			#else:
				#_script_variables.append(
					#"%s: %s = %s" % [p["name"], type_string(p["type"]), p_object.get(p["name"])]
				#)
	#
	##logger.info("{} ({}): ", [_name, _base])
	##for sv: String in _script_variables:
		##logger.info(sv)
