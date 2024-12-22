@tool
class_name BoxProp
extends PropImpl


func update_shape(p_prop: Prop) -> void:
	var mat: Material = p_prop.mesh.material
	
	# Prevents Inspesctor dock redraw / update when changing script variables
	if not p_prop.mesh is BoxMesh:
		p_prop.mesh = BoxMesh.new()
	p_prop.mesh.material = mat
	p_prop.mesh.size = p_prop.prop_scale
	
	var collision: CollisionShape3D = p_prop.get_collision_shape()
	if not collision.shape is BoxShape3D:
		collision.shape = BoxShape3D.new()
	collision.shape.size = p_prop.prop_scale
