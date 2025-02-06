@tool
class_name Inventory
extends Resource

# At no point there can be ItemStack = null

# ItemStack must not change, that is the ItemStack at index 0,
# needs to be the same object throughout the lifetime of this Inventory object

# When shrinking inventory you can only delete ItemStacks which have item = null

signal items_changed
signal size_changed


@export var _items: Array[ItemStack] = []:
	set = __set_items_override

@export var columns: int = 2

func _init(p_size: int = 1) -> void:
	if _items.is_empty():
		var new: Array[ItemStack] = []
		new.resize(p_size)
		_items = new

# Returns state of p_from ItemStack after operation is done:
# True: there is items left in p_from
# False: all items in p_from are gone
# TODO: change return value to be an enum State (LeftOver, NoLeftOver, NotAllowed)
func handle_item_action(p_from: int, p_to_inv: Inventory, p_to: int, p_single: bool) -> bool:
	if not _is_valid_index(p_from):
		return false # return NoLeftOver
	
	if not p_to_inv._is_valid_index(p_to):
		return false # return NoLeftOver
	
	if _items[p_from].is_empty():
		return false # return NoLeftOver
	
	if self == p_to_inv and p_from == p_to:
		return false # return NoLeftOver
	
	if not _is_allowed(p_from, p_to_inv, p_to):
		return false # return NotAllowed
	
	if p_to_inv._items[p_to].is_empty():
		return _move(p_from, p_to_inv, p_to, p_single)
	
	if not p_to_inv._is_allowed(p_to, self, p_from):
		return false # return NotAllowed
	
	# NOTE: Special case when destination is full. 
	#		Otherwise it would try to stack, which would do nothing or
	#		it would try to stack, returning false which makes it inconsistent
	#		with the proper behavior.
	if p_to_inv._items[p_to].quantity == p_to_inv._items[p_to].item.stack_size:
		return _swap(p_from, p_to_inv, p_to)
	
	if _items[p_from].item == p_to_inv._items[p_to].item:
		return _stack(p_from, p_to_inv, p_to, p_single)
	
	# TEST
	return _swap(p_from, p_to_inv, p_to)

func _is_allowed(p_from: int, p_to_inv: Inventory, p_to: int) -> bool:
	for cmp: ItemComponent in p_to_inv._items[p_to].allowed_components:
		if cmp.is_allowed(_items[p_from].item.get_component(cmp.get_script())):
			return true
	
	# empty list means any item is allowed
	return p_to_inv._items[p_to].allowed_components.size() == 0


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


func _move(p_from: int, p_inv_to: Inventory, p_to: int, p_single: bool) -> bool:
	# TEST
	if p_single:
		p_inv_to._items[p_to].item = _items[p_from].item
		_items[p_from].quantity -= 1
		# IMPORTANT: This is weird, I know. Let me explain:
		# ItemStack class has some guarantees in place to ensure correctness
		# Since the item in p_to can be null (hence the assignment above)
		# after the assignment p_to will have quantity = 1 (and not 0!!!).
		# So without this check when p_to is null (that implies quantity = 0)
		# putting a single item in that slot would dupe it (quantity would be = 2)
		if p_inv_to._items[p_to].quantity > 1:
			p_inv_to._items[p_to].quantity += 1
			
		items_changed.emit()
		p_inv_to.items_changed.emit()
		return not _items[p_from].quantity == 0
	
	return not _swap(p_from, p_inv_to, p_to)


func _swap(p_from: int, p_inv_to: Inventory, p_to: int) -> bool:
	var temp_item: Item = p_inv_to._items[p_to].item
	var temp_quantity: int = p_inv_to._items[p_to].quantity
	
	p_inv_to._items[p_to].item = _items[p_from].item
	p_inv_to._items[p_to].quantity = _items[p_from].quantity
	
	_items[p_from].item = temp_item
	_items[p_from].quantity = temp_quantity
	
	items_changed.emit()
	p_inv_to.items_changed.emit()
	return true


func _stack(p_from: int, p_inv_to: Inventory, p_to: int, p_single: bool) -> bool:
	var total: int = p_inv_to._items[p_to].quantity + _items[p_from].quantity
	
	var take_from_total: int
	if p_single:
		take_from_total = mini(
			p_inv_to._items[p_to].quantity + 1, 
			p_inv_to._items[p_to].item.stack_size
		)
	else:
		take_from_total = mini(total, p_inv_to._items[p_to].item.stack_size)
	p_inv_to._items[p_to].quantity = take_from_total

	var quantity_left: int = maxi(0, total - take_from_total)
	_items[p_from].quantity = quantity_left
	
	items_changed.emit()
	p_inv_to.items_changed.emit()
	return not _items[p_from].quantity == 0


func _get_empty() -> Array[int]:
	var result: Array[int] = []
	
	for index: int in _items.size():
		if _items[index].quantity == 0:
			result.append(index)
	return result

func _is_valid_index(p_index: int) -> bool:
	if p_index >= 0 and p_index < _items.size():
		return true

	Logger.be.debug("Index out of range of inventory buffer. Index={}, size={}", [p_index, get_size()])
	return false


func __set_items_override(p_new_items: Array[ItemStack]) -> void:
	if not _items.size() == p_new_items.size():
		size_changed.emit(p_new_items.size())

	_items = p_new_items
	for i in _items.size():
		if _items[i] == null:
			_items[i] = ItemStack.new()
