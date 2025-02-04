class_name CameraArm extends Node3D
#
#@export_subgroup("Spring", "spring")
#@export var spring_max_length: float = 10.0
#@export var spring_min_length: float = 1.0
#@export var spring_step: float = 8.0
#
#@export_range(0.1, 5, 0.1) var sensitivity: float = 3
#
#@onready var _spring: SpringArm3D = $Arm
#@onready var _player: PlayerCharacter = get_parent().get_parent_node_3d()
#
#var _free_look_enabled: bool = false
#var _turn_enabled: bool = false
#var _last_position := Vector2.INF
#
#
#func _unhandled_input(event: InputEvent) -> void:
	#if event is InputEventMouseButton:
		#if event.is_action("camera_zoom_in"):
			#_zoom(-spring_step)
		#elif event.is_action("camera_zoom_out"):
			#_zoom(spring_step)
		#
		#if event.is_action("camera_free_look"):
			#_free_look_enabled = event.pressed
		#elif event.is_action("camera_turn"):
			#_turn_enabled = event.pressed
		#if _turn_enabled:
			#_align_model_with_camera()
		#
		#if _free_look_enabled and not _turn_enabled or not _free_look_enabled and _turn_enabled:
			#_last_position = get_viewport().get_mouse_position()
			#DisplayServer.mouse_set_mode(DisplayServer.MOUSE_MODE_CAPTURED)
		#elif not _free_look_enabled and not _turn_enabled and not _last_position == Vector2.INF:
			#DisplayServer.mouse_set_mode(DisplayServer.MOUSE_MODE_VISIBLE)
			#get_viewport().warp_mouse(_last_position)
			#_last_position = Vector2.INF
	#elif event is InputEventMouseMotion:
		#if _turn_enabled:
			#_align_model_with_camera()
			#_player.model.rotate_y(-event.relative.x * sensitivity * 0.001)
			#
		#if _free_look_enabled or _turn_enabled:
			#rotate_y(-event.relative.x * sensitivity * 0.001)
			#_spring.rotate_x(-event.relative.y * sensitivity * 0.001)
			#_spring.rotation.x = clampf(_spring.rotation.x, -PI / 2, PI / 4)
#
#func _zoom(p_value: float) -> void:
	#_spring.spring_length = lerp(
		#_spring.spring_length,
		#_spring.spring_length + p_value, 0.1
	#)
	 #
	#_spring.spring_length = clampf(
		#_spring.spring_length,
		#spring_min_length,
		#spring_max_length
	#)
#
