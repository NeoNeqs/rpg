extends Node
#class_name HotbarView
#extends ItemView
#
#
#enum Type {
	#Main,
	#Aux1
#}
#
#enum Modifiers {
	#None = 0,
	#Shift = (1 << 0),
	#Alt = (1 << 1),
	#Ctrl = (1 << 2),
	#Meta = (1 << 3)
#}
#
#@export
#var hotbar_type: Type = Type.Main
#
#@export_flags("Shift:1", "Alt:2", "Ctrl:4", "Meta:16")
#var modifiers: int = 0
#
#const PREFIX: String = "hotbar_"
#const MAX_HOTBAR_SIZE: int = 9
#
#const PRIMARY_KEYS: Array[Key] = [
	#KEY_1, KEY_2, KEY_3, 
	#KEY_4, KEY_5, KEY_Q, 
	#KEY_E, KEY_R, KEY_F
#]
#
#func _init() -> void:
	#assert(PRIMARY_KEYS.size() <= MAX_HOTBAR_SIZE)
#
#
#func _ready() -> void:
	#if inventory:
		#inventory.items_changed.connect(on_items_changed)
#
#
#func set_data(p_inventory: Inventory) -> void:
	## Special case where await is used:
	## necessary so that the container has it's children init-ed.
	#await super.set_data(p_inventory)
	#
	#_set_default_keybinds()
#
#
#func on_items_changed() -> void:
	#var index: int = 0
	#for item_stack: ItemStack in inventory._items:
		#var slot: InventorySlot = container.get_child(index)
		#_setup_item_used_signaling(item_stack, slot)
		#index += 1
#
#
#func _unhandled_key_input(event: InputEvent) -> void:
	#for i: int in container.get_child_count():
		#if event.is_action_pressed((PREFIX + _get_hotbar_name()) + ("_%s" % i)):
			#var item_stack: ItemStack = inventory.get_item_stack(i)
			#if item_stack.item == null:
				#continue
			#
			#EventBus.hotbar_key_pressed.emit(item_stack.item)
			##item_stack.item.use()
			#break
#
## TODO: Implement NFInputMap singleton which will handle registering of actions
##       and events and key collisions
#func _set_default_keybinds() -> void:
	#var action: String
	#for i: int in container.get_child_count():
		#action = (PREFIX + _get_hotbar_name()) + ("_%s" % i)
		#if not InputMap.has_action(action):
			#InputMap.add_action(action)
			#Logger.input.info("Registered action '{}' for hotbar '{}'", [2, _get_hotbar_name()])
		#
		#var action_events: Array[InputEvent] = InputMap.action_get_events(action)
		#
		#if action_events.size() > 0:
			#continue
		#
		#var event := InputEventKey.new()
		#event.keycode = PRIMARY_KEYS[i]
		#event.shift_pressed = modifiers & (1 << 0)
		#event.alt_pressed = modifiers & (1 << 1)
		#event.ctrl_pressed = modifiers & (1 << 2)
		#event.meta_pressed = modifiers & (1 << 3)
		#InputMap.action_add_event(action, event)
#
#
#func _get_hotbar_name() -> String:
	#return str(Type.keys()[hotbar_type]).to_lower()
