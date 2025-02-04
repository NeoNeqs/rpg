class_name InventoryView
extends PanelContainer

signal selected_slot_changed(p_item_stack: ItemStack)

signal slot_selected(p_item_stack: ItemStack)
signal slot_unselected()

signal slot_hovered(p_slot: InventorySlot)
signal slot_unhovered()

@onready
var grid: GridContainer = $Grid

var _inventory: Inventory
var selected_index: int = -1

func _ready() -> void:
	mouse_filter = Control.MOUSE_FILTER_STOP


func set_data(p_inventory: Inventory) -> void:
	# TODO: add clear if _inventory not null
	
	_inventory = p_inventory
	_inventory.size_changed.connect(_on_size_changed)
	_inventory.items_changed.connect(_on_items_changed)
	
	# Wait for full init to access children nodes
	if not is_node_ready():
		await ready
	
	grid.columns = p_inventory.columns
	
	for item_stack: ItemStack in p_inventory.get_items():
		grid.add_child(_make_slot(item_stack))
	
	# HACK: shrink container to tightly fit all of the slots
	size = Vector2.ZERO


func _on_size_changed() -> void:
	var old_size: int = grid.get_child_count()
	var new_size: int = _inventory.get_size()
	var add_slot_count: int = maxi(0, new_size - old_size)
	var remove_slot_count: int = maxi(0, old_size - new_size)

	for i: int in add_slot_count:
		grid.add_child(_make_slot(_inventory.get_item_stack(old_size + i)))
	
	for i: int in remove_slot_count:
		var slot: Node = grid.get_child(old_size - 1 - i)
		slot.queue_free()


func _on_items_changed() -> void:
	for index: int in _inventory.get_size():
		var slot: InventorySlot = grid.get_child(index)
		slot.update(_inventory.get_item_stack(index))


func _make_slot(p_item_stack: ItemStack) -> InventorySlot:
	var slot: InventorySlot = AssetDB.InventorySlotScene.instantiate()
	slot.update(p_item_stack)
	
	slot.left_pressed.connect(slot_pressed.bind(slot, false))
	slot.right_pressed.connect(slot_pressed.bind(slot, true))
	slot.hovered.connect(on_slot_hovered.bind(slot))
	slot.unhovered.connect(slot_unhovered.emit)
	
	return slot


func on_slot_hovered(p_slot: InventorySlot) -> void:
	if not selected_index == -1:
		return
	
	slot_hovered.emit(p_slot)


func slot_pressed(p_pressed_slot: InventorySlot, p_single: bool) -> void:
	if selected_index == -1:
		var item_stack: ItemStack = _inventory.get_item_stack(p_pressed_slot.get_index())
		
		if item_stack.quantity == 0:
			return
		
		selected_index = p_pressed_slot.get_index()
		p_pressed_slot.select()
		
		slot_selected.emit(item_stack)
		return
	
	var items_left: bool = _inventory.handle_item_action(
		selected_index,
		p_pressed_slot.get_index(),
		p_single
	)
	
	if items_left:
		var selected_item_stack := _inventory.get_item_stack(selected_index) as ItemStack
		var pressed_item_stack := _inventory.get_item_stack(p_pressed_slot.get_index()) as ItemStack
		
		p_pressed_slot.update(pressed_item_stack)
		selected_slot_changed.emit(selected_item_stack)
		return
	
	var selected_slot := grid.get_child(selected_index) as InventorySlot
	selected_slot.unselect()
	selected_index = -1
	slot_unselected.emit()
	#TooltipManager.show_tooltip(_inventory.get_item_stack(p_pressed_slot.get_index()))


func get_inventory() -> Inventory:
	return _inventory
