@tool
class_name CylinderProp
extends PropImpl


func update_shape(p_prop: Prop) -> void:
	var mat: Material = p_prop.mesh.material
	if not p_prop.mesh is CylinderMesh:
		p_prop.mesh = CylinderMesh.new()
	p_prop.mesh.material = mat
	p_prop.mesh.height = p_prop.prop_scale.y
	
	var collision: CollisionShape3D = p_prop.get_collision_shape()
	if not collision.shape is CylinderShape3D:
		collision.shape = CylinderShape3D.new()
	collision.shape.height = p_prop.prop_scale.y
	collision.shape.radius = p_prop.prop_scale.x / 2
