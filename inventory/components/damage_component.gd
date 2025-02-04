class_name DamageComponent
extends Component

@export var damage_type: CombatSystem.DamageType
@export var min_damage: int
@export var max_damage: int
@export_range(1.5, 4.0, 0.1) var speed: float


func get_tooltip() -> String:
	var tooltip := "%d - %d %s damage #right# %.01f speed" % [
		min_damage, max_damage, CombatSystem.DamageType.keys()[damage_type],
		speed
	]
	
	return tooltip
