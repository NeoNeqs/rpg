extends RigidBody3D

@onready var mesh_instance_3d: MeshInstance3D = $"../MeshInstance3D"


func _input_event(camera: Camera3D, event: InputEvent, event_position: Vector3, normal: Vector3, shape_idx: int) -> void:
	var mouse_button_event := event as InputEventMouseButton
	
	if not mouse_button_event == null:
		if mouse_button_event.is_pressed() and mouse_button_event.button_index == MOUSE_BUTTON_LEFT:
			EventBus.entity_selected.emit(get_parent())
			var mat: StandardMaterial3D = mesh_instance_3d.get_surface_override_material(0)
			mat.emission_enabled = true
