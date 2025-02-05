class_name Spell
extends Resource

@export var name: String
@export var effects: EffectComponent
@export var icon: Texture2D
@export var range_: int
@export var gcd: float

# TODO: 
#	- cost depending on a resource (mana, rage, etc.) component based?
#	- flags
