class_name CameraController
extends Node

@export var arm: Node3D
@export var pivot: Node3D
@export var model: Node3D

@export_range(0.1, 5, 0.1) var sensitivity: float = 3

@export_subgroup("Spring", "spring")
@export var spring_max_length: float = 10.0
@export var spring_min_length: float = 1.0
@export var spring_step: float = 2.0

var _free_look_enabled: bool = false
var _turn_enabled: bool = false
var _last_position := Vector2.INF

# FIXME: when holding right clicking and scrolling at the same time, the camera
#        uncontrollably jumps around.
# TODO: reimplement this script from the ground up
# TODO: don't put Debug.visible like this. Possible solution:
#       - make another global class that will always be in a release build
#         with `process_input` flag
# TODO: implement MouseModeManager state machine. It must have clear states, at
#       least a default one 

func _unhandled_input(event: InputEvent) -> void:
	if event is InputEventMouseButton:
		if event.is_action("camera_zoom_in") and event.pressed:
			_zoom(-spring_step)
		elif event.is_action("camera_zoom_out") and event.pressed:
			_zoom(spring_step)
		
		if event.is_action("camera_free_look"):
			if event.pressed:
				DisplayServer.mouse_set_mode(DisplayServer.MOUSE_MODE_CAPTURED)
			else: 
				DisplayServer.mouse_set_mode(DisplayServer.MOUSE_MODE_VISIBLE)
			_turn_enabled = event.pressed
		elif event.is_action("camera_turn"):
			if event.pressed:
				DisplayServer.mouse_set_mode(DisplayServer.MOUSE_MODE_CAPTURED)
			else: 
				DisplayServer.mouse_set_mode(DisplayServer.MOUSE_MODE_VISIBLE)
			_turn_enabled = event.pressed
	elif event is InputEventMouseMotion:
		# HACK: When mouse mode is set to Captured, clicking any mouse button 
		#       triggers an InputEventMouseMotion with a relative Vector that 
		#       is unrealistically large, even though the mouse hasn’t moved 
		#       (or can't move) that much. This causes the camera to rotate 
		#       rapidally in a short amount of time.
		#       The following check prevents that.
		if absf(event.screen_relative.x) > 100 or absf(event.screen_relative.y) > 100:
			return
		
		if _turn_enabled:
			pivot.rotate_y(-sensitivity * event.screen_relative.x * 0.001)
			arm.rotate_x(-sensitivity * event.screen_relative.y * 0.001)
			arm.rotation.x = clampf(arm.rotation.x, -PI / 2, PI / 4)
#func _unhandled_input(event: InputEvent) -> void:
	#if Debug.visible:
		#return
	#
	#if event is InputEventMouseButton:
		#_handle_mouse_button(event)
	#elif event is InputEventMouseMotion:
		#_handle_mouse_motion(event)
#
#
#func _handle_mouse_button(event: InputEventMouseButton) -> void:
	#if event.is_action("camera_zoom_in"):
		#_zoom(-spring_step)
	#elif event.is_action("camera_zoom_out"):
		#_zoom(spring_step)
	#
	## Prevent camera turn when dragging InventorySlot
	#if DisplayServer.mouse_get_mode() == DisplayServer.MOUSE_MODE_HIDDEN:
		#return
#
	#if event.is_action("camera_free_look"):
		#_free_look_enabled = event.pressed
	#elif event.is_action("camera_turn"):
		#_turn_enabled = event.pressed
#
	## Align model with camera when turning starts
	#if _turn_enabled:
		#_align_model_with_camera()
	#
	#_update_mouse_mode()
#
#func _update_mouse_mode() -> void:
	#if not _free_look_enabled == _turn_enabled:
		## Prevent warping when the mode didn't change
		#if DisplayServer.mouse_get_mode() == DisplayServer.MOUSE_MODE_CAPTURED:
			#return
		#_last_position = get_viewport().get_mouse_position()
		#DisplayServer.mouse_set_mode(DisplayServer.MOUSE_MODE_CAPTURED)
	#elif not _free_look_enabled and not _turn_enabled and not _last_position == Vector2.INF:
		#DisplayServer.mouse_set_mode(DisplayServer.MOUSE_MODE_VISIBLE)
		#get_viewport().warp_mouse(_last_position)
		## Only warp mouse once
		#_last_position = Vector2.INF
#
#
#func _handle_mouse_motion(event: InputEventMouseMotion) -> void:
	#if _turn_enabled:
		#_align_model_with_camera()
		#model.rotate_y(-event.relative.x * sensitivity * 0.001)
	#
	#if _free_look_enabled or _turn_enabled:
		#pivot.rotate_y(-event.relative.x * sensitivity * 0.001)
		#arm.rotate_x(-event.relative.y * sensitivity * 0.001)
		#arm.rotation.x = clampf(arm.rotation.x, -PI / 2, PI / 4)
#
#
#
##func _unhandled_input(event: InputEvent) -> void:
	##if event is InputEventMouseButton:
		##if event.is_action("camera_zoom_in"):
			##_zoom(-spring_step)
		##elif event.is_action("camera_zoom_out"):
			##_zoom(spring_step)
		##
		### TODO: make this less hacky with some global state maybe
		##
		### HACK: Don't handle camera turn when dragging InventorySlot
		##if DisplayServer.mouse_get_mode() == DisplayServer.MOUSE_MODE_HIDDEN:
			##return
		##
		##if event.is_action("camera_free_look"):
			##_free_look_enabled = event.pressed
		##elif event.is_action("camera_turn"):
			##_turn_enabled = event.pressed
		##
		### Don't wait for mouse motion to update the model
		##if _turn_enabled:
			##_align_model_with_camera()
		##
		##if (_free_look_enabled and not _turn_enabled) or (not _free_look_enabled and _turn_enabled):
			##if not DisplayServer.mouse_get_mode() == DisplayServer.MOUSE_MODE_CAPTURED:
				##_last_position = get_viewport().get_mouse_position()
				##DisplayServer.mouse_set_mode(DisplayServer.MOUSE_MODE_CAPTURED)
		##elif not _free_look_enabled and not _turn_enabled and not _last_position == Vector2.INF:
			##DisplayServer.mouse_set_mode(DisplayServer.MOUSE_MODE_VISIBLE)
			##get_viewport().warp_mouse(_last_position)
			##_last_position = Vector2.INF
	##elif event is InputEventMouseMotion:
		##if _turn_enabled:
			##_align_model_with_camera()
			##model.rotate_y(-event.relative.x * sensitivity * 0.001)
			##
		##if _free_look_enabled or _turn_enabled:
			##pivot.rotate_y(-event.relative.x * sensitivity * 0.001)
			##arm.rotate_x(-event.relative.y * sensitivity * 0.001)
			##arm.rotation.x = clampf(arm.rotation.x, -PI / 2, PI / 4)
#
#
func _zoom(p_value: float) -> void:
	arm.spring_length = lerp(
		arm.spring_length,
		arm.spring_length + p_value, 0.1
	)
	 
	arm.spring_length = clampf(
		arm.spring_length,
		spring_min_length,
		spring_max_length
	)

#func _align_model_with_camera() -> void:
	#var from := Quaternion(model.global_basis.orthonormalized())
	#var to := Quaternion(pivot.global_basis.orthonormalized())
	#if not from == to:
		#model.global_basis = Basis(from.slerp(to, 1))
