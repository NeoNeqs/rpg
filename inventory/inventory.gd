@tool
class_name Inventory
extends Resource

# At no point there can be ItemStack = null

# ItemStack must not change, that is the ItemStack at index 0,
# needs to be the same object throughout the lifetime of this Inventory object

# When shrinking inventory you can only delete ItemStacks which have item = null
#

signal items_changed
signal size_changed


@export var _items: Array[ItemStack] = []:
	set = __set_items_override

@export var columns: int = 2


# Returns state of p_from ItemStack after operation is done:
# True: there is items left in p_from
# False: all items in p_from are gone
func handle_item_action(p_from: int, p_to: int, p_single: bool) -> bool:
	if not _is_valid_index(p_from):
		return false
	
	if not _is_valid_index(p_to):
		return false
	
	# TEST
	if _items[p_from].is_empty():
		return false
	
	# TEST
	if p_from == p_to:
		return false
	
	# TEST
	if _items[p_to].is_empty():
		return _move(p_from, p_to, p_single)
	
	# TEST
	# NOTE: Special case when destination is full. 
	#		Otherwise it would try to stack, which would change nothing or
	#		it would try to stack, returning false which makes it inconsistent
	#		with the proper behavior.
	if _items[p_to].quantity == _items[p_to].item.stack_size:
		return _swap(p_from, p_to) # not
	
	if _items[p_from].item == _items[p_to].item:
		return _stack(p_from, p_to, p_single)
	
	# TEST
	return _swap(p_from, p_to)

# True: resize was successful
# False: resized failed
#func resize(p_new_size: int) -> bool:
	#var old_size: int = items.size()
	#var add_slot_count: int = maxi(0, p_new_size - old_size)
	#var remove_slot_count: int = maxi(0, old_size - p_new_size)
	#
	#if remove_slot_count > 0:
		#var empty_indecies: Array[int] = _get_empty()
		#
		#if empty_indecies.size() < remove_slot_count:
			#return false
		#
		#for i in remove_slot_count:
			#pass
	#
	#for i in add_slot_count:
		#items.append(ItemStack.new())
	#
	

func get_item_stack(p_index: int) -> ItemStack:
	if not _is_valid_index(p_index):
		return null
	
	return _items[p_index]


func set_item(p_item: Item, p_index: int, p_quantity: int = 1) -> bool:
	if not _is_valid_index(p_index):
		return false
	
	# TEST: especially the order, since item has to be set before quantity
	_items[p_index].item = p_item
	_items[p_index].quantity = p_quantity
	items_changed.emit()
	return true

func is_item_empty(p_index: int) -> bool:
	return _items[p_index].quantity == 0


func get_items() -> Array[ItemStack]:
	return _items


func get_size() -> int:
	return _items.size()


func _move(p_from: int, p_to: int, p_single: bool) -> bool:
	# TEST
	if p_single:
		_items[p_to].item = _items[p_from].item
		_items[p_from].quantity -= 1
		_items[p_to].quantity += 1
		items_changed.emit()
		return not _items[p_from].quantity == 0
	
	# TEST
	var temp: ItemStack = _items[p_to]
	_items[p_to] = _items[p_from]
	_items[p_from] = temp
	items_changed.emit()
	return false


func _swap(p_from: int, p_to: int) -> bool:
	# TEST
	var temp: ItemStack = _items[p_to]
	_items[p_to] = _items[p_from]
	_items[p_from] = temp
	
	items_changed.emit()
	#return false
	return true


func _stack(p_from: int, p_to: int, p_single: bool) -> bool:
	# TEST:
	var total: int = _items[p_to].quantity + _items[p_from].quantity
	
	var take_from_total: int
	if p_single:
		take_from_total = mini(_items[p_to].quantity + 1, _items[p_to].item.stack_size)
	else:
		take_from_total = mini(total, _items[p_to].item.stack_size)
	_items[p_to].quantity = take_from_total

	var quantity_left: int = maxi(0, total - take_from_total)
	_items[p_from].quantity = quantity_left
	items_changed.emit()
	return not _items[p_from].quantity == 0


func _get_empty() -> Array[int]:
	var result: Array[int] = []
	
	for index: int in _items.size():
		if _items[index].quantity == 0:
			result.append(index)
	return result

func _is_valid_index(p_index: int) -> bool:
	# TEST:
	if p_index >= 0 and p_index < _items.size():
		return true

	Logger.be.debug("Index out of range of inventory buffer. Index={}, size={}", [p_index, get_size()])
	return false


func __set_items_override(p_new_items: Array[ItemStack]) -> void:
	# 1. Handle size difference
	# 1a. Add slots
	# 1b. Remove slots
	
	# 2. Now that the sizes of old and new array are equal, copy ItemStack contents
	#    to exixisting one
	if not _items.size() == p_new_items.size():
		size_changed.emit(p_new_items.size())

	_items = p_new_items
	for i in _items.size():
		if _items[i] == null:
			_items[i] = ItemStack.new()
