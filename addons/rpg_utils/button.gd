@tool
extends Button

func _pressed() -> void:
	ResourceDB.Load()
	
	# Hack: force reload edited object so that inspector displays fresh information from ResourceDB
	var edited_object := EditorInterface.get_inspector().get_edited_object()
	EditorInterface.get_inspector().edit(null)
	EditorInterface.get_inspector().edit(edited_object)
	
