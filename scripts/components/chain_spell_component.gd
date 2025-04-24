class_name ChainSpellComponent
extends SpellComponent

var current: int = -1

@export var spells: Array[Item]

func _init() -> void:
	pass


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
	
	#spells[current].use()
	return Result.Next

#func cast() -> bool:
	#if current == -1:
		#current = wrapi(current + 1, -1, spells.size())
		#return false
	#
	## TODO: add a guard to prevent recurrsion.
	#spells[current].use()
	#current = wrapi(current + 1, -1, spells.size())
	#
	#return true

func get_icon_override() -> Texture2D:
	if current == -1:
		return null
	
	return spells[current].icon


func get_display_name_override() -> String:
	if current == -1:
		return ""
	return spells[current].display_name


func get_spell() -> Item:
	if current == -1:
		return null
	return spells[current]
