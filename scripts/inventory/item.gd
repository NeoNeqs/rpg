@tool
class_name Item
extends Resource

signal used(p_time_usec: int)
signal spell_casted(p_effect: Effect)

static var _logger: Logger = Logger.new("Item", Logger.LogLevel.Debug)

# NOTE: You almost never want to use these. Use getter functions from below.
@export var display_name: String = ""
@export_multiline var lore: String = ""
@export var icon: Texture2D = null
@export_range(0, 1_000_000) var price: int = 0
@export_range(1, 1_000) var stack_size: int = 1
@export_range(0.0, 3600.0) var cooldown: float = 1.0

var _last_cast_time: int = Time.get_ticks_usec()

# exported through `_get_property_list`
# IMPORTANT: it's serialized as an Array and deserialiazed as Dictionary.
# 			 See details in `_get_property_list`, `_get` and `_set` overrides 
var _components := {}

# Used for automatic tooltip creation
static var registered_components: Array[Script] = [
	ArmorComponent, 
	DamageComponent,
	AttributeComponent,
	EffectComponent,
]


func make_copy(p_deep := false, p_copy_components := false, p_cmp_deep := false) -> Item:
	var new: Item = duplicate(p_deep)
	
	if not p_copy_components:
		return new
	
	new._components = new._components.duplicate(p_cmp_deep)
	return new


func get_component(p_script: Script) -> ItemComponent:
	var key: StringName = p_script.get_global_name()
	
	if key in _components:
		return _components[key]
	
	return null


#region Item.get_tooltip() and helpers

func get_tooltip() -> String:
	var tooltip: String = "%s"
	
	var detailed_item_component: DetailedItemComponent = get_component(DetailedItemComponent)
	if not detailed_item_component == null:
		tooltip = detailed_item_component.get_tooltip()
	
	tooltip = tooltip % get_display_name()
	
	for cmpnt_type: Script in registered_components:
		var component: ItemComponent = get_component(cmpnt_type)
		if component == null:
			continue
		
		tooltip += component.get_tooltip()
	
	if not lore.is_empty():
		tooltip += '\n"#color=purple#%s#color=white#"' % lore
	
	if price > 0:
		tooltip += "\nSell price: %d coins" % price
	
	return tooltip


func get_rarity_color() -> Color:
	var detailed_item_component: DetailedItemComponent = get_component(DetailedItemComponent)
	if not detailed_item_component == null:
		return detailed_item_component.get_rarity_color()
	
	return Color.BLACK

#endregion


#region Members' getters

# The functions below properly resolve item properties when Components are used.

func get_icon() -> Texture2D:
	var chain_spell_component: ChainSpellComponent = get_component(ChainSpellComponent)
	
	if chain_spell_component == null:
		return icon
	
	var overridden_spell: Item = chain_spell_component.get_spell()
	if overridden_spell == null:
		return icon
	
	return overridden_spell.icon


func get_display_name() -> String:
	var chain_spell_component: ChainSpellComponent = get_component(ChainSpellComponent)
	
	if chain_spell_component == null:
		return display_name
	
	var overridden_spell: Item = chain_spell_component.get_spell()
	if overridden_spell == null:
		return display_name
	
	return overridden_spell.display_name

#endregion


#region Item.use() and helpers

func use() -> void:
	var spell_component: SpellComponent = get_component(SpellComponent)
	if spell_component != null:
		_handle_spell_component(spell_component)
	
	var chain_spell_component: ChainSpellComponent = get_component(ChainSpellComponent)
	if chain_spell_component != null:
		_handle_chain_spell_component(chain_spell_component)


func _handle_spell_component(p_spell_component: SpellComponent) -> void:
	var cooldown_usec := int(cooldown * 1_000_000)

	if is_on_cooldown(cooldown_usec):
		_logger.info(
			"Spell '{}' is still on cooldown for {}.", 
			[
				get_display_name(), 
				DebugUtils.humnaize_time(get_remaining_cooldown())]
		)
		return
	
	var cast_result: SpellComponent.Result = p_spell_component.cast()
	
	if cast_result == SpellComponent.Result.NoCast:
		_logger.error(
			"Spell '{}' does not have a behavior.",
			[get_display_name()]
		)
		return
	
	used.emit(cooldown_usec)
	spell_casted.emit(p_spell_component.effect)
	_last_cast_time = Time.get_ticks_usec()


func _handle_chain_spell_component(p_chain_spell_component: ChainSpellComponent) -> void:
	var spell: Item = p_chain_spell_component.get_spell()
	if spell == null:
		spell = self
	
	var cooldown_usec := int(spell.cooldown * 1_000_000)

	if is_on_cooldown(cooldown_usec):
		_logger.info(
			"Spell '{}' is still on cooldown for {}.", 
			[
				spell.get_display_name(), 
				DebugUtils.humnaize_time(get_remaining_cooldown())
			]
		)
		return

	var result: SpellComponent.Result = p_chain_spell_component.cast()
	
	match result:
		SpellComponent.Result.Casted:
			_logger.info("Casting spell '{}'.", [spell.get_display_name()])
			_complete_cast(p_chain_spell_component, p_chain_spell_component.effect)
		SpellComponent.Result.Next:
			var spell_comp: SpellComponent = spell.get_component(SpellComponent)
			if spell_comp == null:
				_logger.error(
					"Spell '{}' in a spell chain of '{}' does not have a SpellComponent.",
					# IMPORTANT: display_name here is intended
					[spell.get_display_name(), display_name] 
				)
				return
			
			if is_on_cooldown(cooldown_usec):
				return
			
			var _result: SpellComponent.Result = spell_comp.cast()
			_logger.debug("Casting spell '{}'.", [spell.get_display_name()])
			_complete_cast(p_chain_spell_component, spell_comp.effect)
		SpellComponent.Result.NoCast:
			_logger.debug("Spell '{}' has no behavior!", [spell.get_display_name()])


func _complete_cast(p_chain_spell_component: ChainSpellComponent, p_effect: Effect) -> void:
	p_chain_spell_component.chain()
	var next_spell: Item = p_chain_spell_component.get_spell()
	if next_spell == null:
		next_spell = self
	used.emit(next_spell.cooldown * 1_000_000)
	
	#var next_spell_spell_component := next_spell.get_component(SpellComponent)
	spell_casted.emit(p_effect)
	_last_cast_time = Time.get_ticks_usec()


func is_on_cooldown(cooldown_usec: int) -> bool:
	var current_time := Time.get_ticks_usec()
	return (_last_cast_time + cooldown_usec > current_time)


func get_remaining_cooldown() -> int:
	var csc: ChainSpellComponent = get_component(ChainSpellComponent)
	if csc == null:
		return 0
	var cooldown_in_usec: int
	var spell: Item = csc.get_spell()
	if spell == null:
		cooldown_in_usec = int(cooldown * 1_000_000.0)
	else:
		cooldown_in_usec = int(spell.cooldown * 1_000_000.0)
	
	return _last_cast_time + cooldown_in_usec - Time.get_ticks_usec()

#endregion


#region @export components

## This is where components are exported / serialized. 
## The Inspector sees it as an Array, but it's serialized as a dictionary.
## This allows to lookup components in nearly O(1) time (most of the time).

func _get_property_list() -> Array[Dictionary]:
	return [{
		"name": &"components",
		"type": TYPE_ARRAY,
		"hint": PROPERTY_HINT_TYPE_STRING,
		"hint_string": "24/17:ItemComponent",
		"usage": PROPERTY_USAGE_EDITOR
	},
	{
		"name": &"_components",
		"type": TYPE_DICTIONARY,
		"usage": PROPERTY_USAGE_NO_EDITOR | PROPERTY_USAGE_SCRIPT_VARIABLE
	}]


func _validate_property(property: Dictionary) -> void:
	if property.name == &"type":
		property.usage |= PROPERTY_USAGE_UPDATE_ALL_IF_MODIFIED


func _get(property: StringName) -> Variant:
	if property == &"components":
		return _components.values()
	return null


func _set(property: StringName, value: Variant) -> bool:
	if property == &"components":
		var new_components_map := {}
		
		# For some reason editor can sometimes spit out Array[EncodedObjectAsID] 
		# instead of Array[ItemComponents]. 
		# It can happen when updating components while the game is running in the editor.
		for variant: Variant in value:
			var component := variant as ItemComponent
			if not component or component.get_script().get_base_script() == null:
				new_components_map[&"null"] = null
				continue
			
			var key: StringName = component.get_script().get_global_name()
			# Copy existing component to prevent data lost
			if key in _components:
				new_components_map[key] = _components[key]
			else:
				new_components_map[key] = component
		
		_components = new_components_map
		return true
	return false

#endregion
