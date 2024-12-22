@tool
class_name StairsProp
extends PropImpl


func update_shape(p_prop: Prop) -> void:
	var mat: Material = p_prop.mesh.material
	if not p_prop.mesh is SphereMesh:
		p_prop.mesh = SphereMesh.new()
	p_prop.mesh.material = mat
	p_prop.mesh.radius = p_prop.prop_scale.x / 2
	p_prop.mesh.height = p_prop.prop_scale.y
	
	var collision: CollisionShape3D = p_prop.get_collision_shape()
	if not collision.shape is SphereShape3D:
		collision.shape = SphereShape3D.new()
	collision.shape.radius = p_prop.prop_scale.x / 2
