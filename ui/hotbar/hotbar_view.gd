class_name HotbarView
extends ItemView


func _ready() -> void:
	inventory.items_changed.connect(on_items_changed)


func on_items_changed() -> void:
	var index: int = 0
	for item_stack: ItemStack in inventory._items:
		var slot: InventorySlot = container.get_child(index)
		_setup_item_used_signaling(item_stack, slot)
		index += 1

func _unhandled_key_input(event: InputEvent) -> void:
	for i: int in mini(4, container.get_child_count()):
		if event.is_action_pressed("hotbar_%s" % (i + 1)):
			var item_stack: ItemStack = inventory.get_item_stack(i)
			if item_stack.item == null:
				continue
			
			item_stack.item.use()
