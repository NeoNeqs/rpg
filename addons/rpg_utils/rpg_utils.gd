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
		
		found_label.meta_clicked.connect(func(p_data: Variant) -> void:
			if typeof(p_data) == TYPE_STRING:
				var str := str(p_data)
				if str.begins_with("res://"):
					var ext := str.get_extension()
					if ext in ["gd", "cs"]:
						EditorInterface.edit_script(load(str))
					elif ext in ["scn", "tscn"]:
						EditorInterface.open_scene_from_path(str)
					elif ext.is_empty() and ext.is_absolute_path():
						EditorInterface.get_file_system_dock().navigate_to_path(str)
					else:
						EditorInterface.edit_resource(load(str))
			else:
				OS.shell_open(p_data)
		)
	else:
		print("No RichTextLabel under @EditorLog was found.")


func _exit_tree() -> void:
	if panel:
		panel.queue_free()
	#queue_free()


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
