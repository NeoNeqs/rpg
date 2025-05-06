class_name PlayerCharacter
extends Node3D

@export var inventory: Inventory

@onready
var combat_system: CombatSystem = $CombatSystem


func _ready() -> void:
	EventBus.player_inventory_loaded.emit(inventory)
