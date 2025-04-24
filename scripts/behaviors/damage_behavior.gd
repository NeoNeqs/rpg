class_name DamageBehavior
extends SpellBehavior

@export var amount: int = 0

func _init() -> void:
	pass

func on_cast() -> bool:
	print("Dealt %d Damage" % amount)
	return true
