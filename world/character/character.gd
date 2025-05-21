extends Node
#class_name PlayerCharacter
#extends Entity
#
#
#@export var inventory: Inventory
#
#
#func _ready() -> void:
	#EventBus.player_inventory_loaded.emit(inventory)
	#EventBus.player_inventory_loaded.emit(combat_manager.armory)
	#EventBus.hotbar_key_pressed.connect(_on_hotbar_key_pressed)
	#
	#
	#EventBus.character_attributes_loaded.emit(combat_manager.base_attributes)
	#EventBus.total_attributes_loaded.emit(combat_manager.attribute_system._total_attributes)
#
#
#func _on_hotbar_key_pressed(p_item: Item) -> void:
	#if combat_manager._current_target == null:
		#print("no target in ", name)
		#return
	#
	##var target_combat_manager: CombatManager = combat_manager._current_target.combat_manager
	#if not p_item.spell_casted.is_connected(combat_manager.apply_effects):
		#p_item.spell_casted.connect(
			#combat_manager.apply_effects, 
			#ConnectFlags.CONNECT_ONE_SHOT
		#)
	#
	#p_item.use()
