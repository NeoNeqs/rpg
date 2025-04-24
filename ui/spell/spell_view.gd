class_name SpellView
extends InventoryView



func _make_slot(p_item_stack: ItemStack) -> InventorySlot:
	var spell_slot: SpellSlot = slot_scene.instantiate()
	spell_slot.update(p_item_stack)
	
	spell_slot.left_pressed.connect(slot_pressed.emit.bind(self, spell_slot, false))
	spell_slot.right_pressed.connect(slot_pressed.emit.bind(self, spell_slot, true))
	spell_slot.hovered.connect(slot_hovered.emit.bind(spell_slot))
	spell_slot.unhovered.connect(slot_unhovered.emit)
	return spell_slot

#func _ready() -> void:
	#var spell_slot: SpellSlot = AssetDB.SpellSlotScene.instantiate()
	#spell_slot.right_pressed.connect(_on_slot_right_pressed.bind(spell_slot))
	#var item_stack: ItemStack = inv.get_item_stack(0)
	#if not item_stack.item == null:
		#item_stack.item.used.connect(func on_used(cooldown_in_usec: int) -> void:
			##spell_slot.update(item_stack)
			#spell_slot.set_on_cooldown(cooldown_in_usec)
		#)
	#spell_slot.update(inv.get_item_stack(0))
	#%List.add_child(spell_slot)
#
#func _on_slot_right_pressed(p_slot: InventorySlot) -> void:
	#var index: int = p_slot.get_index()
	#
	#var item_stack: ItemStack = inv.get_item_stack(index)
	#var spell: Item = item_stack.item
	#if spell == null:
		#print("Spell was null")
		#return
	#
	#spell.use()
	#p_slot.update(item_stack)
