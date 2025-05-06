class_name CombatSystem
extends Node


@export var is_player: bool = false

enum DamageType {
	Physical,
	Shadow,
	Nature,
	Fire,
}

@export var attribute_system: AttributeSystem
@export var stat_system: StatSystem

@export var character_attributes: Attributes
@export var armory: Inventory


var _logger := Logger.combat
var target: CombatSystem


# TODO:
# Attributes: str, dex, int, armor, armor_pen, etc.
# Stats: current hp, mp, special points, haste etc.
# Flags: is_stunned, is_feared, is_locked, etc.


func _ready() -> void:
	target = $"../TestTarget"
	EventBus.hotbar_key_pressed.connect(_on_hotbar_key_pressed)
	
	if is_player:
		EventBus.player_inventory_loaded.emit(armory)
		
		character_attributes.value_changed.connect(func(p_attribute: StringName, p_delta: float) -> void:
			EventBus.character_attributes_changed.emit(character_attributes)
		)
		# Must be emitted before it's linked to the attribute_system
		# This allows UI to display attributes immediately
		character_attributes.value_changed.emit(&"", 0)
	
	attribute_system.link(character_attributes)
	
	armory.item_changed.connect(_apply_armor_attributes)
	armory.item_about_to_change.connect(_remove_armor_attributes)


func _apply_armor_attributes(p_item: Item, _p_index: int) -> void:
	var attribute_component := p_item.get_component(AttributeComponent) as AttributeComponent
	if attribute_component == null:
		return
	
	attribute_system.link(attribute_component.attributes)


func _remove_armor_attributes(p_item: Item, _p_index: int) -> void:
	var attribute_component := p_item.get_component(AttributeComponent) as AttributeComponent
	if attribute_component == null:
		return
	
	attribute_system.unlink(attribute_component.attributes)


func _on_hotbar_key_pressed(p_spell: Item) -> void:
	if not p_spell.spell_casted.is_connected(apply_effect):
		p_spell.spell_casted.connect(apply_effect, ConnectFlags.CONNECT_ONE_SHOT)
	
	p_spell.use()


func apply_effect(p_effect: Effect) -> void:
	if p_effect.apply_chance < 1:
		var chance: float = floor(randf() * 100) / 100.0
		if chance > p_effect.apply_chance:
			return
	
	# Instant and permament effect
	if p_effect.ticks == 0:
		_apply_effect(p_effect)
		return
	
	if p_effect.is_immediate():
		_apply_effect(p_effect)
	
	var timer := Timer.new()
	timer.autostart = true
	timer.timeout.connect(_apply_periodic_effect.bind(timer, p_effect))
	timer.wait_time = p_effect.tick_timeout
	add_child(timer)


func _apply_effect(p_effect: Effect) -> void:
	var damage_effect := p_effect as DamageEffect
	if not damage_effect == null:
		var damage_value: int = damage_effect.apply(target, self)
		stat_system.damage(damage_value)
		return
	
	var modifier_effect := p_effect as ModifierEffect
	if not modifier_effect == null:
		character_attributes.apply(p_effect)
		_logger.info("Applying effect: {} {}", [p_effect.value, p_effect.get_modifier()])
		return
	
	_logger.error("Unhandled Effect type '{}', found @ '{}'.", [
		DebugUtils.nameof(p_effect.get_script()),
		p_effect.resource_path
	])


func _remove_effect(p_effect: Effect) -> void:
	if p_effect is ModifierEffect:
		character_attributes.remove(p_effect)
		_logger.info("Removing effect: {} {}", [-p_effect.value * p_effect.ticks, p_effect.get_modifier()])
	elif p_effect is DamageEffect:
		pass


func _apply_periodic_effect(p_timer: Timer, p_effect: Effect) -> void:
	if p_effect.tick():
		_apply_effect(p_effect)
		return
	
	if not p_effect.is_immediate():
		_apply_effect(p_effect)
	
	_remove_effect(p_effect)
	
	p_timer.stop()
	p_timer.queue_free()
