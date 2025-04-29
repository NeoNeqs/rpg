@tool
class_name DetailedItemComponent
extends ItemComponent

enum Rarity {
	Common,
	Uncommon,
	Rare,
	Epic,
	Legendary,
}

@export var rarity: Rarity = Rarity.Common
@export_range(1, 100) var level: int


func is_allowed(p_other: ItemComponent) -> bool:
	return (
		p_other is DetailedItemComponent
	)


func get_tooltip() -> String:
	var tooltip: String = _get_colored_name_format()
	if level > 1 and int(rarity) > int(Rarity.Common):
		tooltip += "Item Level %d\n" % level
	
	return tooltip


func _get_colored_name_format() -> String:
	return "#size=20##color={0}#%s ({1})\n".format([
		get_rarity_color().to_html(), Rarity.keys()[rarity]
	])


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
