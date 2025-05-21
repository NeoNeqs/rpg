extends Node
#@tool
#class_name SpellComponent
#extends ItemComponent
#
#enum Result {
	#Casted,
	#Next,
	#NoCast,
#}
#
##@export var behavior: SpellBehavior
#@export var effects: Array[Effect]
#
#
#func is_allowed(p_other: ItemComponent) -> bool:
	#return (
		#p_other is SpellComponent or p_other is ChainSpellComponent
	#)
#
#
#func cast() -> Result:
	##if behavior == null:
		##return Result.NoCast
	#
	##behavior.on_cast()
	#return Result.Casted
