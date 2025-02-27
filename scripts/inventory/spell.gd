class_name Spell
extends Resource

@export var name: String
@export var effects := EffectComponent.new()
@export var icon: Texture2D
@export var range_: int = 40
@export var gcd: float = 1.5

# TODO: 
#	- cost depending on a resource (mana, rage, etc.) component based?
#	- flags

func get_tooltip() -> String:
	var tooltip := "#color=white#%s#right#%dm" % [name, range_]
	
	return tooltip
