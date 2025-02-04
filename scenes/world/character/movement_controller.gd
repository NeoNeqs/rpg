class_name MovementController
extends Node

@export var body: CharacterBody3D
@export var model: Node3D

const SPEED: float = 5.0
const JUMP_VELOCITY: float = 4.5


func _physics_process(delta: float) -> void:
	if not body.is_on_floor():
		body.velocity += body.get_gravity() * delta
	
	if Input.is_action_just_pressed("ui_accept") and body.is_on_floor():
		body.velocity.y = JUMP_VELOCITY
	
	var input_dir := Input.get_vector("strafe_left", "strafe_right", "forward", "backwards")
	var direction := (model.transform.basis * Vector3(input_dir.x, 0, input_dir.y)).normalized()
	
	if direction:
		body.velocity.x = direction.x * SPEED
		body.velocity.z = direction.z * SPEED
	else:
		body.velocity.x = move_toward(body.velocity.x, 0, SPEED)
		body.velocity.z = move_toward(body.velocity.z, 0, SPEED)
	
	body.move_and_slide()
