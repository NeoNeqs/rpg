class_name Tooltip
extends PanelContainer


func update(p_resource: Resource) -> bool:
	if p_resource is Item or p_resource is Spell:
		%Tooltip.update(p_resource.get_tooltip())
		size = Vector2.ZERO
		return true
	
	%Tooltip.clear()
	return false

#func update(p_item_stack: ItemStack) -> bool:
	#if not p_item_stack or not p_item_stack.item:
		#%Tooltip.clear()
		#return false
	#
	#%Tooltip.update(p_item_stack.item.get_tooltip())
	#
	#size = Vector2.ZERO
	#return true
	##global_position = p_pos
	## HACK: force minimal size
	##update_minimum_size()
