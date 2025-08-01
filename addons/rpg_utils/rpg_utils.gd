@tool
extends EditorPlugin

var MainPanel: PackedScene
var panel: Control



func _enter_tree() -> void:
	MainPanel = load("res://addons/rpg_utils/panel.tscn")
	panel = MainPanel.instantiate()
	
	EditorInterface.get_editor_main_screen().add_child(panel)
	_make_visible(false)
	
	var root: Node = EditorInterface.get_base_control()
	var found_label: RichTextLabel = _find_output_label(root)

	if found_label:
		for dict: Dictionary in found_label.get_signal_connection_list("meta_clicked"):
			found_label.disconnect("meta_clicked", dict["callable"])
		
		found_label.meta_clicked.connect(_output_meta_clicked)
	else:
		print("No RichTextLabel under @EditorLog was found.")
	

func _exit_tree() -> void:
	if panel:
		panel.queue_free()


func _get_plugin_icon() -> Texture2D:
	return EditorInterface.get_editor_theme().get_icon(&"Node", &"EditorIcons")


func _get_plugin_name() -> String:
	return "RPG"


func _has_main_screen() -> bool:
	return true


func _make_visible(visible):
	if panel:
		panel.visible = visible


# Credit: u/fariazz ~ https://www.reddit.com/r/godot/comments/1hl301w/comment/m3sg4z5/
# Works in: 4.4.1-stable
func _find_output_label(current: Node) -> RichTextLabel:
	# Check if current node is a RichTextLabel
	if current is RichTextLabel:
		var parent: Node = current.get_parent()
		if parent:
			var grandparent: Node = parent.get_parent()
			# Only check 'name' if the grandparent is valid
			if grandparent and grandparent.name.begins_with("@EditorLog"):
				return current

	# Recurse through children
	for child in current.get_children():
		var found: RichTextLabel = _find_output_label(child)
		if found:
			return found

	return null

# Works in: 4.4.1-stable
func _find_script_editor_debugger(current: Node) -> MarginContainer:
	# Check if current node is a RichTextLabel
	if current is MarginContainer:
		if current.get_child_count() >= 2 and current.get_child(0, true) is TabContainer and current.get_child(1, true) is ConfirmationDialog:
			return current

	# Recurse through children
	for child in current.get_children():
		var found: MarginContainer = _find_script_editor_debugger(child)
		if found:
			return found

	return null

func _find_scene_tree_dock(current: Node) -> VBoxContainer:
	# Check if current node is a RichTextLabel
	if current is VBoxContainer:
		if current.get_child_count() >= 13 and current.get_child(0, true) is HBoxContainer:
			return current

	# Recurse through children
	for child in current.get_children():
		var found: VBoxContainer = _find_scene_tree_dock(child)
		if found:
			return found

	return null

# Works in: 4.4.1-stable
func _output_meta_clicked(p_data: Variant) -> void:
	if not typeof(p_data) == TYPE_STRING:
		printerr("Can't handle meta of type %d." % [typeof(p_data)])
		return
	
	var str_data := str(p_data)
	if str_data.begins_with("res://"):
		var ext := str_data.get_extension()
		
		if ext in ["gd", "cs"]:
			EditorInterface.get_file_system_dock().navigate_to_path(str_data)
			EditorInterface.edit_script(load(str_data))
		elif ext in ["scn", "tscn"]:
			EditorInterface.get_file_system_dock().navigate_to_path(str_data)
			EditorInterface.open_scene_from_path(str_data)
		elif ext.is_empty() and ext.is_absolute_path():
			EditorInterface.get_file_system_dock().navigate_to_path(str_data)
		elif ResourceLoader.exists(str_data):
			EditorInterface.get_file_system_dock().navigate_to_path(str_data)
			EditorInterface.edit_resource(load(str_data))
		else:
			printerr("Could not handle data '%s'" % str_data)
	elif str_data.begins_with('^'):
		var colon_index := str_data.find(':')
		if colon_index == -1:
			printerr("Meta data is missing a ':' to signify the object id.")
			return
		var at_symbol_index := str_data.find('@', colon_index + 1)
		if at_symbol_index == -1:
			printerr("Meta data is missing a '@' to signify the scene file.")
			return
		if EditorInterface.is_playing_scene():
			var node_object_id := int(str_data.substr(colon_index + 1, at_symbol_index - colon_index - 1))
			
			var root: Node = EditorInterface.get_base_control()
			var script_editor_debugger := _find_script_editor_debugger(root)
			
			var scene_tree_dock := _find_scene_tree_dock(root)
			# Open the remote tab
			(scene_tree_dock.get_child(1).get_child(0) as Button).pressed.emit()
			script_editor_debugger.emit_signal("remote_tree_select_requested", node_object_id)
		else:
			# start from 1 to omit the '^' at the beginning
			var node_path := str_data.substr(1, colon_index - 1)
			var scene_file := str_data.substr(at_symbol_index + 1)
			if scene_file.is_empty():
				printerr("Could not find selected node. No scene file was provided.")
				return
			
			EditorInterface.open_scene_from_path(scene_file)
			var node := EditorInterface.get_edited_scene_root().get_node_or_null(node_path)
			if node == null:
				printerr("Could not find node '%s' in '%s'. It no longer exists." % [node_path, scene_file])
				return
			
			EditorInterface.edit_node(node)
	else:
		OS.shell_open(p_data)
