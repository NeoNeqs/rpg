class_name PlayerCharacter
extends Entity


@export var inventory: Inventory


func _ready() -> void:
	EventBus.player_inventory_loaded.emit(inventory)
	EventBus.player_inventory_loaded.emit(combat_manager.armory)
	EventBus.hotbar_key_pressed.connect(_on_hotbar_key_pressed)
	EventBus.entity_selected.connect(
		func(p_entity: Entity) -> void:
			combat_manager._current_target = p_entity
	)
	
	EventBus.character_attributes_loaded.emit(combat_manager.base_attributes)
	EventBus.total_attributes_loaded.emit(combat_manager.attribute_system._total_attributes)
	#_setup_attribute_callbacks(
		#EventBus.character_attributes_changed, 
		#combat_manager.base_attributes
	#)
	#
	#_setup_attribute_callbacks(
		#EventBus.total_attributes_changed,
		#combat_manager.attribute_system._total_attributes
	#)
	
	#var l_callable := \
		#func on_attribute_change(p_attribute: StringName, p_delta: float) -> void:
			#EventBus.character_attributes_changed.emit(combat_manager.base_attributes)
	#
	#combat_manager.base_attributes.value_changed.connect(l_callable)
	#l_callable.call(&"", 0)
#
#func _setup_attribute_callbacks(p_signal: Signal, p_attributes: Attributes) -> void:
	#var l_callable := \
		#func on_attribute_change(p_attribute: StringName, p_delta: float) -> void:
			#p_signal.emit(p_attributes)
	#
	#p_attributes.value_changed.connect(l_callable)
	#l_callable.call(&"", 0)


func _on_hotbar_key_pressed(p_item: Item) -> void:
	if combat_manager._current_target == null:
		print("no target in ", name)
		return
	
	#var target_combat_manager: CombatManager = combat_manager._current_target.combat_manager
	if not p_item.spell_casted.is_connected(combat_manager.apply_effects):
		p_item.spell_casted.connect(
			combat_manager.apply_effects, 
			ConnectFlags.CONNECT_ONE_SHOT
		)
	
	p_item.use()
