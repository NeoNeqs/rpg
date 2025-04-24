@tool
class_name Inventory
extends Resource

# At no point there can be ItemStack = null

# ItemStack must not change, that is the ItemStack at index 0,
# needs to be the same object throughout the lifetime of this Inventory object

# When shrinking inventory you can only delete ItemStacks which have item = null

signal items_changed
signal size_changed

# Todo: Find better names. There are 2 (3?) states:
#       1. After action is done there are still items left in the source slot (Leftover)
#          This should keep the slot selected.
#       2. After action is done there are no more items left in the source (NoLeftover)
#          This should deselect the slot
#       3. ........

enum ItemActionResult {
	NoAction,
	LeftOver,
#	NoLeftOver,
}

@export var _items: Array[ItemStack] = []:
	set = __set_items_override

@export var columns: int = 2
@export var editable: bool = true
@export var owns: bool = true

@export var allowed_components: Array[ItemComponent] = []:
	set(v):
		allowed_components = v
		notify_property_list_changed()


func _init(p_size: int = 1) -> void:
	if _items.is_empty():
		var new: Array[ItemStack] = []
		new.resize(p_size)
		_items = new


func handle_item_action(p_from: int, p_to_inv: Inventory, p_to: int, p_single: bool) -> ItemActionResult:
	if not _is_valid_index(p_from):
		return ItemActionResult.NoAction
	
	if not p_to_inv._is_valid_index(p_to):
		return ItemActionResult.NoAction
	
	if _items[p_from].is_empty():
		return ItemActionResult.NoAction
	
	if self == p_to_inv and p_from == p_to:
		return ItemActionResult.NoAction
	
	if not _is_allowed(p_from, p_to_inv, p_to):
		return ItemActionResult.NoAction
	
	if not editable:
		if p_to_inv.editable:
			return _reference(p_from, p_to_inv, p_to)
		return ItemActionResult.NoAction
	
	if owns and not p_to_inv.owns:
		return _reference(p_from, p_to_inv, p_to)
	
	if not owns and p_to_inv.owns:
		return ItemActionResult.NoAction
	
	if p_to_inv._items[p_to].is_empty():
		return _move(p_from, p_to_inv, p_to, p_single)
	
	# IMPORTANT: This condition (however stupid it may seem) is possible:
	# 1. Have the `p_to` index have an allowed item placed in the that slot
	# 2. Select an item in the same inventory (idk if it matters whether 
	#    it's the same or not) that is not allowed to be place in `p_to` index
	# 3. Right click the `p_to` slot that has the allowed.
	# The expected behavior is to not allow the selected item to be placed there.
	# And that's what this condition prevents.
	if not p_to_inv._is_allowed(p_to, self, p_from):
		return ItemActionResult.NoAction
	

	# NOTE: Special case when destination is full. 
	#		Otherwise it would try to stack, which would do nothing or
	#		it would try to stack, returning false which would be inconsistent
	#		with the proper behavior (Swapping).
	if p_to_inv._items[p_to].quantity == p_to_inv._items[p_to].item.stack_size:
		#if owns and p_to_inv.owns:
		return _swap(p_from, p_to_inv, p_to)

	if _items[p_from].item == p_to_inv._items[p_to].item and owns and p_to_inv.owns:
		return _stack(p_from, p_to_inv, p_to, p_single)
	return _swap(p_from, p_to_inv, p_to)

## Checks whether an item at `p_from` index is allowed to be placed in
## p_to_inv at `p_to` index.
func _is_allowed(p_from: int, p_to_inv: Inventory, p_to: int) -> bool:
	for is_cmp: ItemComponent in p_to_inv._items[p_to].allowed_components:
		var temp := _items[p_from].item.get_component(is_cmp.get_script())
		if is_cmp.is_allowed(temp):
			return true
	
	if p_to_inv._items[p_to].allowed_components.size() == 0:
		for inv_cmp: ItemComponent in p_to_inv.allowed_components:
			var temp := _items[p_from].item.get_component(inv_cmp.get_script())
			if inv_cmp.is_allowed(temp):
				return true
		
	# empty list means any item is allowed
	return false

func delete(p_from: int) -> void:
	_items[p_from].item = null
	items_changed.emit()


func get_item_stack(p_index: int) -> ItemStack:
	if not _is_valid_index(p_index):
		return null
	
	return _items[p_index]


func set_item(p_item: Item, p_index: int, p_quantity: int = 1) -> bool:
	if not _is_valid_index(p_index):
		return false
	
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


func _move(p_from: int, p_inv_to: Inventory, p_to: int, p_single: bool) -> ItemActionResult:
	if p_single and owns:
		p_inv_to._items[p_to].item = _items[p_from].item
		_items[p_from].quantity -= 1
		# IMPORTANT: This is weird, I know. Let me explain:
		# ItemStack class has some guarantees in place to ensure it's correctness
		# Since the item in p_to can be null (hence the assignment above)
		# after the assignment p_to will have quantity = 1 (and not 0!!!).
		# So without this check when p_to is null (that implies quantity = 0 at `p_to` index)
		# putting a single item in that slot would dupe it (it would be `2` instead `1`)
		if p_inv_to._items[p_to].quantity > 1:
			p_inv_to._items[p_to].quantity += 1
			
		items_changed.emit()
		p_inv_to.items_changed.emit()
	else:
		_swap(p_from, p_inv_to, p_to)
	
	if _items[p_from].quantity == 0:
		#return ItemActionResult.NoLeftOver
		return ItemActionResult.NoAction
		
	return ItemActionResult.LeftOver


func _swap(p_from: int, p_inv_to: Inventory, p_to: int) -> ItemActionResult:
	var temp_item: Item = p_inv_to._items[p_to].item
	var temp_quantity: int = p_inv_to._items[p_to].quantity
	
	p_inv_to._items[p_to].item = _items[p_from].item
	p_inv_to._items[p_to].quantity = _items[p_from].quantity
	
	_items[p_from].item = temp_item
	_items[p_from].quantity = temp_quantity
	
	items_changed.emit()
	p_inv_to.items_changed.emit()
	return ItemActionResult.LeftOver


func _stack(p_from: int, p_inv_to: Inventory, p_to: int, p_single: bool) -> ItemActionResult:
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
	
	if _items[p_from].quantity == 0:
		#return ItemActionResult.NoLeftOver
		return ItemActionResult.NoAction
		
	return ItemActionResult.LeftOver


func _reference(p_from: int, p_inv_to: Inventory, p_to: int) -> ItemActionResult:
	# IMPORTANT: Do not reference ItemStacks here. By copying the item directly
	#            we avoid problems with deleting items and moving them around!
	p_inv_to._items[p_to].item = _items[p_from].item
	p_inv_to._items[p_to].quantity = _items[p_from].quantity
	p_inv_to.items_changed.emit()
	return ItemActionResult.NoAction


func _is_valid_index(p_index: int) -> bool:
	if p_index >= 0 and p_index < _items.size():
		return true

	Logger.core.debug("Index out of range of inventory buffer. Index={}, size={}", [p_index, get_size()])
	return false


func __set_items_override(p_new_items: Array[ItemStack]) -> void:
	if not _items.size() == p_new_items.size():
		size_changed.emit(p_new_items.size())

	_items = p_new_items
	for i in _items.size():
		if _items[i] == null:
			_items[i] = ItemStack.new()
