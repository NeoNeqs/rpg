## Manages Combat Systems of the parent Entity and target entity
#class_name CombatManager
extends Node

#var _logger := Logger.combat

#var _current_target: Entity
#
#@export var attribute_system: AttributeSystem
#@export var stat_system: StatSystem
#
#@export var base_attributes: Attributes
#@export var armory: Inventory
#
#@export var combat_resources: CombatResources
#
#
#
#func _ready() -> void:
	#stat_system.attribute_system = attribute_system
	#attribute_system.link(base_attributes)
	#_link_armory_attributes()
	#
	#armory.item_changed.connect(_link_armor_attribute)
	#armory.item_about_to_change.connect(_unlink_armor_attribute)
#
	#combat_resources.current_health = stat_system.get_max_health()
#
#
#func _link_armory_attributes() -> void:
	#for item_stack: ItemStack in armory._items:
		#if item_stack.item == null:
			#continue
		#_link_armor_attribute(item_stack.item)
#
#
#func _link_armor_attribute(p_item: Item) -> void:
	#if p_item == null:
		#return
	#var l_attributes := p_item.get_attributes()
	#if not l_attributes == null:
		#attribute_system.link(l_attributes)
		#
#
#
#func _unlink_armor_attribute(p_item: Item) -> void:
	#if p_item == null:
		#return
	#var l_attributes := p_item.get_attributes()
	#if not l_attributes == null:
		#attribute_system.unlink(l_attributes)
#
#func apply_effects(p_effects: Array[Effect]) -> void:
	#for effect: Effect in p_effects:
		#apply_effect(effect)
#
#
#func apply_effect(p_effect: Effect) -> void:
	#var timer: Timer = p_effect.apply(self, _current_target.combat_manager)
#
	#if not timer == null:
		#add_child(timer)
	
