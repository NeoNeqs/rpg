class_name DamageBehavior
extends SpellBehavior

@export var amount: int = 0

func _init() -> void:
	pass

func on_cast() -> bool:
	Logger.core.info("Dealing '{}' damage", [amount])
	return true
