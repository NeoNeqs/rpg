@tool
extends EditorPlugin


func _enter_tree() -> void:
	var t := Control.new()
	add_control_to_bottom_panel(t, "")
	var bottom_panel := t.get_parent().get_parent()
	remove_control_from_bottom_panel(t)
	var editor_log := bottom_panel.get_child(0).get_child(0)
	var output: RichTextLabel = editor_log.get_child(1).get_child(0)
	for c in output.meta_clicked.get_connections():
		output.meta_clicked.disconnect(c["callable"])
		
	output.meta_clicked.connect(func (p_meta: Variant):
		if not typeof(p_meta) == TYPE_STRING:
			return
		
		var meta_str: String = str(p_meta)
		var args: PackedStringArray = meta_str.rsplit(':', false, 1)
		
		if not args[0].is_absolute_path():
			print(args[0])
			return
		
		for script: Script in get_editor_interface().get_script_editor().get_open_scripts():
			if script.resource_path == args[0]:
				get_editor_interface().edit_script(script, int(args[1]))
	)



func _exit_tree() -> void:
	pass
