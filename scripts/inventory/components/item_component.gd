@tool
#class_name ItemComponent
extends Resource


func is_allowed(_p_other: ItemComponent) -> bool:
	return true


func get_tooltip() -> String:
	return ""
