class_name View
extends UIELement

@export var _holder: Control
@export var _inventory: Inventory
@export var _slot_scene: PackedScene

signal slot_pressed(p_view: View, p_slot: Slot, p_single: bool)

signal slot_hovered(p_view: View, p_slot: Slot)
signal slot_unhovered()


func _ready() -> void:
	resize()


func set_data(p_inventory: Inventory) -> void:
	_inventory = p_inventory
	_inventory.size_changed.connect(resize)
	_inventory.items_changed.connect(_on_items_changed)
	
	# Wait for full init to access children nodes
	if not is_node_ready():
		await ready
	
	if _holder is GridContainer and p_inventory is ItemInventory:
		var _grid := _holder as GridContainer
		var _item_inventory := p_inventory as ItemInventory
		_grid.columns = _item_inventory.columns
	
	resize()
	
	# shrink container to tightly fit all of the slots
	size = custom_minimum_size


func get_element_at(p_index: int) -> Resource:
	return _inventory.get_element_at(p_index)


func is_empty_at(p_index: int) -> bool:
	return _inventory.is_empty_at(p_index)


func resize() -> void:
	var old_size: int = _holder.get_child_count()
	var new_size: int = _inventory.get_size()
	var add_slot_count: int = maxi(0, new_size - old_size)
	var remove_slot_count: int = maxi(0, old_size - new_size)

	for i: int in add_slot_count:
		_holder.add_child(_make_slot(_inventory.get_element_at(old_size + i)))
	
	for i: int in remove_slot_count:
		var slot: Node = _holder.get_child(old_size - 1 - i)
		slot.queue_free()


func _make_slot(p_resource: Resource) -> Node:
	var slot := _slot_scene.instantiate() as Slot
	if slot == null:
		# TODO: log error
		return
	
	slot.update(p_resource)
	slot.hovered.connect(slot_hovered.emit.bind(self, slot))
	slot.unhovered.connect(slot_unhovered.emit)
	slot.left_pressed.connect(slot_pressed.emit.bind(self, slot, false))
	slot.right_pressed.connect(slot_pressed.emit.bind(self, slot, true))
	
	return slot


func get_slot(p_index: int) -> Slot:
	return _holder.get_child(p_index) as Slot


func _on_items_changed() -> void:
	for index: int in _inventory.get_size():
		var slot := _holder.get_child(index) as Slot
		if slot == null:
			# TODO: log error
			continue
		slot.update(_inventory.get_element_at(index))
