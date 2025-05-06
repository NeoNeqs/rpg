@tool
extends EditorPlugin

var MainPanel: PackedScene
var panel: Control


func _enter_tree() -> void:
	MainPanel = load("res://addons/rpg_utils/panel.tscn")
	panel = MainPanel.instantiate()
	EditorInterface.get_editor_main_screen().add_child(panel)
	_make_visible(false)


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
