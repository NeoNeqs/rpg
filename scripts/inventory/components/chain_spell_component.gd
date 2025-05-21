@tool
class_name ChainSpellComponent
extends SpellComponent

var current: int = -1

@export var spells: Array[Item]


func chain() -> void:
	current = wrapi(current + 1, -1, spells.size())


func is_allowed(p_other: ItemComponent) -> bool:
	return super.is_allowed(p_other)


func get_next_spell() -> Item:
	var next: int = wrapi(current + 1, -1, spells.size())
	if next == -1:
		return null
	return spells[next]


func cast() -> Result:
	if current == -1:
		return super.cast()
	
	return Result.Next


func get_spell() -> Item:
	if current == -1:
		return null
	return spells[current]
