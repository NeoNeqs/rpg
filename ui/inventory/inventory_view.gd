class_name InventoryView
extends UIELement

signal slot_pressed(p_view: InventoryView, p_slot: InventorySlot, p_single: bool)

signal slot_hovered(p_slot: InventorySlot)
signal slot_unhovered()

#var _grid: GridContainer = $Grid
@export
var container: Container

@export
var slot_scene: PackedScene

var inventory: Inventory


func set_data(p_inventory: Inventory) -> void:
	inventory = p_inventory
	inventory.size_changed.connect(resize)
	inventory.items_changed.connect(_on_items_changed)
	
	# Wait for full init to access children nodes
	if not is_node_ready():
		await ready
	
	#_grid.columns = p_inventory.columns
	_setup_container()
	
	resize()
	
	# shrink container to tightly fit all of the slots
	size = custom_minimum_size


func _setup_container() -> void:
	assert(false, "Do not call this method.")

func resize() -> void:
	var old_size: int = container.get_child_count()
	var new_size: int = inventory.get_size()
	var add_slot_count: int = maxi(0, new_size - old_size)
	var remove_slot_count: int = maxi(0, old_size - new_size)

	for i: int in add_slot_count:
		container.add_child(_make_slot(inventory.get_item_stack(old_size + i)))
	
	for i: int in remove_slot_count:
		var slot: Node = container.get_child(old_size - 1 - i)
		slot.queue_free()


func _on_items_changed() -> void:
	for index: int in inventory.get_size():
		var slot: InventorySlot = container.get_child(index)
		slot.update(inventory.get_item_stack(index))


func _make_slot(p_item_stack: ItemStack) -> InventorySlot:
	assert(false, "Do not call this method.")
	return null
	#var slot: InventorySlot = slot_scene.instantiate()
	##var slot: InventorySlot = AssetDB.ItemSlotScene.instantiate()
	#slot.update(p_item_stack)
	#
	#slot.left_pressed.connect(slot_pressed.emit.bind(self, slot, false))
	#slot.right_pressed.connect(slot_pressed.emit.bind(self, slot, true))
	#slot.hovered.connect(slot_hovered.emit.bind(slot))
	#slot.unhovered.connect(slot_unhovered.emit)
	#
	#return slot


func get_slot(p_index: int) -> InventorySlot:
	return container.get_child(p_index) as InventorySlot
