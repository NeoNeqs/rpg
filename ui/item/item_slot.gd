class_name ItemSlot
extends InventorySlot


@export var border_holder: Panel


func update(p_item_stack: ItemStack) -> void:
	if p_item_stack == null or p_item_stack.item == null:
		_set_border_color(Color.BLACK)
		
		icon_holder.texture = null
		text_holder.text = ""
		return
	
	_set_border_color(p_item_stack.item.get_rarity_color())
	
	icon_holder.texture = p_item_stack.item.get_icon()
	if p_item_stack.quantity <= 1:
		text_holder.text = ""
	else:
		text_holder.text = str(p_item_stack.quantity)


func _set_border_color(p_color: Color) -> void:
	if border_holder == null:
		return
	
	var stylebox: StyleBoxFlat = border_holder.get_theme_stylebox("panel")
	
	if stylebox == null:
		return
	
	stylebox.border_color = p_color
