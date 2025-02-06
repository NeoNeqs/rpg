class_name PlayerCharacter
extends Node3D

var inventory: Inventory

@onready
var combat_system: CombatSystem = $CombatSystem


func _ready() -> void:
	inventory = preload("res://resources/player_inventory.tres")
	EventBus.player_inventory_loaded.emit(inventory)
#	combat_system.apply_effect(load("res://instant_poison.tres"))
#	combat_system.apply_effect(load("res://blade_flurry.tres"))
#	combat_system.apply_effect(load("res://corruption.tres"))
