@tool
class_name ArmorComponent
extends ItemComponent

enum Slot {
	Head,
	Neck,
	Chest,
}

enum Type {
	Light,
	Medium,
	Heavy,
}

@export var slot: Slot
@export var armor_type: Type
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
 Armor (%d/%d)
""" % [Slot.keys()[slot], Type.keys()[armor_type], current_durability, max_durability]
