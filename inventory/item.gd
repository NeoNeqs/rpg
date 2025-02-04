@tool
class_name Item
extends Resource

enum Rarity {
	Common,
	Uncommon,
	Rare,
	Epic,
	Legendary,
}

@export var display_name: String = ""
@export_multiline var lore: String = ""
@export var icon: Texture2D = null
@export var rarity: Rarity = Rarity.Common
@export_range(0, 1_000_000) var price: int = 0
@export_range(1, 100) var level: int
@export_range(1, 1_000) var stack_size: int = 1

# exported through `_get_property_list`
# IMPORTANT: it's serialized as an Array and deserialiazed as Dictionary.
# 			 See details in `_get_property_list`, `_get` and `_set` overrides 
var _components := {}

static var registered_components: Array[Script] = [
	ArmorComponent, 
	DamageComponent,
	StatsComponent,
	EffectComponent,
]


func get_tooltip() -> String:
	var tooltip: String = _get_colored_name()
	
	if level > 1 and int(rarity) > int(Rarity.Common):
		tooltip += "Item Level %d\n" % level
	
	for cmpnt_type: Script in registered_components:
		var component: Component = get_component(cmpnt_type)
		if component == null:
			continue
		
		tooltip += component.get_tooltip()
	
	if not lore.is_empty():
		#tooltip += '\n"[color=purple]%s[/color]"' % lore
		tooltip += '\n"#color=purple#%s#color=white#"' % lore
	
	return tooltip


func copy(p_deep := false, p_copy_components := false, p_cmp_deep := false) -> Item:
	var new: Item = duplicate(p_deep)
	
	if not p_copy_components:
		return new
	
	new._components = new._components.duplicate(p_cmp_deep)
	return new


func get_component(p_script: Script) -> Component:
	var key: StringName = p_script.get_global_name()
	
	if key in _components:
		return _components[key]
	
	return null


func get_rarity_color() -> Color:
	match rarity:
		Rarity.Common:
			return Color.GRAY
		Rarity.Uncommon:
			return Color.LIME_GREEN
		Rarity.Rare:
			return Color.DODGER_BLUE
		Rarity.Epic:
			return Color.BLUE_VIOLET
		Rarity.Legendary:
			return Color.ORANGE_RED
	
	return Color.BLACK


func _get_colored_name() -> String:
	return "#size=20##color=%s#%s (%s)\n" % [
		get_rarity_color().to_html(), display_name, Rarity.keys()[rarity]
	]

#region Editor stuff

func _get_property_list() -> Array[Dictionary]:
	return [{
		"name": &"components",
		"type": TYPE_ARRAY,
		"hint": PROPERTY_HINT_TYPE_STRING,
		"hint_string": "24/17:Component",
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
		
		for cmp: Component in value:
			if not cmp or cmp.get_script().get_base_script() == null:
				new_components_map[&"null"] = null
				continue
			
			var key: StringName = cmp.get_script().get_global_name()
			# Copy existing component to prevent data lost
			if key in _components:
				new_components_map[key] = _components[key]
			else:
				new_components_map[key] = cmp
		
		_components = new_components_map
		return true
	return false

#endregion
