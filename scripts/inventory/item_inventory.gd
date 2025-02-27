@tool
class_name ItemInventory
extends Inventory

# At no point there can be ItemStack = null

# ItemStack must not change, that is the ItemStack at index 0,
# needs to be the same object throughout the lifetime of this Inventory object

# When shrinking inventory you can only delete ItemStacks which have item = null

@export var _items: Array[ItemStack] = []:
	set = __set_items_override

@export var columns: int = 2

func _init(p_size: int = 1) -> void:
	if _items.is_empty():
		var new: Array[ItemStack] = []
		new.resize(p_size)
		_items = new


func is_empty_at(p_index: int) -> bool:
	return _items[p_index].quantity == 0


# Returns state of p_from ItemStack after operation is done:
# True: there is items left in p_from
# False: all items in p_from are gone
# TODO: change return value to be an enum State (LeftOver, NoLeftOver, NotAllowed)
func handle_item_action(p_from: int, p_to_inv: Inventory, p_to: int, p_single: bool) -> ActionResult:
	if super.handle_item_action(p_from, p_to_inv, p_to, p_single) == ActionResult.NotAllowed:
		return ActionResult.NotAllowed
	
	if not _is_valid_index(p_from):
		return ActionResult.Invalid
	
	if not p_to_inv._is_valid_index(p_to):
		return ActionResult.Invalid
	
	if _items[p_from].is_empty():
		return ActionResult.Invalid
	
	if self == p_to_inv and p_from == p_to:
		return ActionResult.NoLeftover
	
	if not _is_allowed(p_from, p_to_inv, p_to):
		return ActionResult.NotAllowed
	
	if p_to_inv._items[p_to].is_empty():
		return _move(p_from, p_to_inv, p_to, p_single)
	
	if not p_to_inv._is_allowed(p_to, self, p_from):
		return ActionResult.NotAllowed
	
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

func _is_allowed(p_from: int, p_to_inv: ItemInventory, p_to: int) -> bool:
	# empty list means ny item is allowed
	if p_to_inv._items[p_to].allowed_components.size() == 0:
		return ActionResult.Allowed
	
	for cmp: ItemComponent in p_to_inv._items[p_to].allowed_components:
		if cmp.is_allowed(_items[p_from].item.get_component(cmp.get_script())):
			return ActionResult.Allowed
	
	return ActionResult.NotAllowed


func get_element_at(p_index: int) -> ItemStack:
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


func _move(p_from: int, p_inv_to: ItemInventory, p_to: int, p_single: bool) -> ActionResult:
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
		if _items[p_from].quantity == 0:
			return ActionResult.NoLeftover
		
		return ActionResult.Allowed
	
	#return not _swap(p_from, p_inv_to, p_to)
	_swap(p_from, p_inv_to, p_to)
	
	return ActionResult.NoLeftover

func _swap(p_from: int, p_inv_to: ItemInventory, p_to: int) -> ActionResult:
	var temp_item: Item = p_inv_to._items[p_to].item
	var temp_quantity: int = p_inv_to._items[p_to].quantity
	
	p_inv_to._items[p_to].item = _items[p_from].item
	p_inv_to._items[p_to].quantity = _items[p_from].quantity
	
	_items[p_from].item = temp_item
	_items[p_from].quantity = temp_quantity
	
	items_changed.emit()
	p_inv_to.items_changed.emit()
	return ActionResult.Allowed


func _stack(p_from: int, p_inv_to: ItemInventory, p_to: int, p_single: bool) -> ActionResult:
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
		return ActionResult.NoLeftover
	return ActionResult.Allowed


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
