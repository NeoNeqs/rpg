class_name ArmorComponent
extends CombatComponent

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

@export
var armor: int

@export
var slot: Slot

@export
var armor_type: Type


func get_tooltip() -> String:
	return """%s#right##color=red#%s
%d Armor (%d/%d)
""" % [Slot.keys()[slot], Type.keys()[armor_type], armor, total_durability, total_durability]
