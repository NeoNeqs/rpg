class_name SpellSlot
extends InventorySlot


func update(p_item_stack: ItemStack) -> void:
	if p_item_stack == null or p_item_stack.item == null:
		
		icon_holder.texture = null
		text_holder.text = ""
		return
	
	icon_holder.texture = p_item_stack.item.get_icon()
	text_holder.text = p_item_stack.item.get_display_name()

func set_on_cooldown(p_time_usec: int) -> void:
	%Cooldown.start(p_time_usec)
