@tool
class_name Prop
extends MeshInstance3D


# Must match the filenames of props in res://prop/impl/
@export_enum("box", "capsule", "cylinder", "sphere", "stairs") var prop_shape: String = "box":
	set(value):
		# Prevents Inspesctor dock redraw when editing the object
		if not prop_shape == value:
			_update_shape(value)  # must be called before assignment
		if value == "stairs":
			texture = 8
		prop_shape = value
		_update_collision()

@export_enum("dark", "green", "light", "orange", "purple", "red") var color: String = "orange":
	set(value):
		color = value
		_update_texture()

@export_range(1, 10) var texture: int = 1:
	set(value):
		if prop_shape == "stairs" and not value == 8:
			#Logger.physics.warn("Can't change texture for Stairs Shape")
			texture = 8
		else:
			texture = value
		_update_texture()

@export var enable_collision: bool = true:
	set(value):
		enable_collision = value
		_update_collision()

var _prop_data: PropData


func _enter_tree() -> void:
	if material_override == null:
		material_override = StandardMaterial3D.new()

	set_notify_local_transform(true)

	_update_shape(prop_shape)
	_update_texture()


func _ready() -> void:
	# update the collision once
	_update_collision()


func _update_shape(p_new_shape: String) -> void:
	_prop_data = load(_get_prop_path(p_new_shape))

	#mesh = _prop_data.mesh.duplicate(true)

	var mat: BaseMaterial3D = get_metarial()
	mat.uv1_triplanar = _prop_data.use_triplanar
	mat.uv1_world_triplanar = _prop_data.use_triplanar


func _update_collision() -> void:
	if not is_node_ready():
		return
		
	if not enable_collision:
		return

	for child: Node in get_children():
		if child is PhysicsBody3D:
			remove_child(child)
			child.queue_free()

	# Don't generate collisions when editing a scene in the editor
	if not Engine.is_editor_hint():
		create_convex_collision(true, true)


func _update_texture() -> void:
	var mat: BaseMaterial3D = get_metarial()
	if not mat:
		#Logger.physics.warn("Prop '{}' does not have any materials set", [get_instance_id()])
		return

	mat.albedo_texture = load(
		"res://assets/textures/prototype/%s/texture_%s.png" % [
			color, str(texture).pad_zeros(2)
		]
	)


func _get_prop_path(p_name: String) -> String:
	return "res://resources/prop_data/%s.tres" % [p_name.to_lower()]


func get_metarial() -> BaseMaterial3D:
	for i: int in get_surface_override_material_count():
		var override_mat: Material = get_surface_override_material(i)

		if override_mat is BaseMaterial3D:
			return override_mat

	return material_override as BaseMaterial3D
