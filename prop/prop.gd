@tool
class_name Prop
extends GeometryInstance3D

# TODO: dynamic collision option: `var enable_collision: bool`
#       when off it should not have a Body nor a CollisionShape

# Keep in sync with [property _impls]
@export_enum("Box", "Cylinder", "Sphere", "Stairs")
var shape: String = "Box":
	set(value):
		shape = value
		_prop_impl = _impls[shape]
		_update_shape()


@export_enum("dark", "green", "light", "orange", "purple", "red") 
var color: String = "orange":
	set(value):
		color = value
		_update()


@export_range(1, 10)
var type: int = 1:
	set(value):
		if shape == "Stairs":
			return
		type = value
		_update()


@export_custom(PROPERTY_HINT_LINK, "")
var prop_scale: Vector3 = Vector3.ONE:
	set(value):
		prop_scale = value.clamp(Vector3.ZERO, Vector3(10, 10, 10))
		_update_shape()


# Untyped self to trick Godot's type checking
var _u_self = self

# Keep in sync with [property shape]
var _impls: Dictionary = {
	"Box": BoxProp.new(),
	"Sphere": SphereProp.new(),
	"Cylinder": CylinderProp.new(),
	"Stairs": StairsProp.new()
}

var _prop_impl: PropImpl = _impls[shape]


func _ready() -> void:
	_update()
	_update_shape()


func _update_shape() -> void:
	if not is_node_ready():
		return
	
	if not _u_self is MeshInstance3D:
		printerr("Object is not a MeshInstance3D. " + ObjInfo.for_vi(self))
		return
		
	_prop_impl.update_shape(self)


func _update() -> void:
	var mat: BaseMaterial3D = _get_metarial()
	if not mat:
		printerr("Material not found for prop. " + ObjInfo.for_vi(self))
		return
	
	mat.albedo_texture = load("res://textures/prototype/%s/texture_%s.png" % [
		color,
		str(type).pad_zeros(2)
	])


func _get_metarial() -> BaseMaterial3D:
	if _u_self is MeshInstance3D:
		if not _u_self.mesh == null and _u_self.mesh.material is BaseMaterial3D:
			return _u_self.mesh.material

		for i in _u_self.get_surface_override_material_count():
			var override_mat: Material = _u_self.get_surface_override_material(i)
			
			if override_mat is BaseMaterial3D:
				return override_mat
	
	return material_override as BaseMaterial3D


func get_collision_shape() -> CollisionShape3D:
	return $Body/Collision as CollisionShape3D
