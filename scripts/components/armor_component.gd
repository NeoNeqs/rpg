@tool
class_name ArmorComponent
extends ItemComponent

enum SlotType {
	Head,
	Neck,
	Chest,
}

enum ArmorType {
	Light,
	Medium,
	Heavy,
}

@export var armor: int
@export var slot: SlotType
@export var armor_type: ArmorType
@export var current_durability: int
@export var max_durability: int

func is_allowed(p_other: ItemComponent) -> bool:
	return (
		p_other is ArmorComponent and
		slot == p_other.slot and
		int(armor_type) <= int(p_other.armor_type)
	)


func get_tooltip() -> String:
	return """%s#right##color=red##right#%s
%d Armor (%d/%d)
""" % [SlotType.keys()[slot], ArmorType.keys()[armor_type], armor, current_durability, max_durability]
