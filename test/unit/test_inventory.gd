extends GutTest

func before_all() -> void:
	pass


func before_each() -> void:
	pass


func after_all() -> void:
	pass


func after_each() -> void:
	pass


func test_default_init() -> void:
	var inv := Inventory.new()
	assert_eq(inv._items.size(), 1)
	assert_not_null(inv._items[0])


func test_init() -> void:
	var inv := Inventory.new(10)
	assert_eq(inv._items.size(), 10)
	for i: int in inv._items.size():
		assert_not_null(inv._items[i])


func test_set_items_size() -> void:
	var inv := Inventory.new()
	inv._items = [null, null]
	assert_eq(inv._items.size(), 2)


func test_set_items_item_stacks_not_null() -> void:
	var inv := Inventory.new()
	inv._items = [null, null, null]
	for i: int in inv._items.size():
		assert_not_null(inv._items[i])

func test_is_valid_index() -> void:
	var inv := Inventory.new(2)
	assert_false(inv._is_valid_index(-1))
	assert_false(inv._is_valid_index(-2))
	assert_false(inv._is_valid_index(2))
	assert_false(inv._is_valid_index(3))
	
	assert_true(inv._is_valid_index(0))
	assert_true(inv._is_valid_index(1))

func test_handle_item_action_left_over() -> void:
	var inv := Inventory.new(2)
	
	# Branch 1
	assert_false(inv.handle_item_action(-1, inv, 0, false))
	# Branch 2
	assert_false(inv.handle_item_action(0, inv, -1, false))
	# Branch 3
	assert_false(inv.handle_item_action(0, inv, 1, false))
	# Branch 4
	assert_false(inv.handle_item_action(0, inv, 0, false))


	

class TestSelfActions extends GutTest:
	var inv_same_items: Inventory
	var inv_different_items: Inventory
	
	func before_each() -> void:
		inv_same_items = Inventory.new()
		
		# Same items, 1 stack of quantity 2 and 1 stack of 1
		var item_stack1 := ItemStack.new()
		var item_stack2 := ItemStack.new()
		item_stack1.item = Item.new()
		item_stack1.item.stack_size = 2
		item_stack1.quantity = 2
		
		item_stack2.item = item_stack1.item
		var items: Array[ItemStack] = [
			item_stack1,
			item_stack2,
			ItemStack.new(),
			ItemStack.new()
		]
		inv_same_items._items = items
		
		# Different items
		var item_stack3 := ItemStack.new()
		var item_stack4 := ItemStack.new()
		item_stack3.item = Item.new()
		item_stack4.item = Item.new()

		
		items = [
			item_stack3,
			item_stack4,
			ItemStack.new(),
			ItemStack.new()
		]
		
		inv_different_items._items = items
		
	#func test_handle_item_action() -> void:
		#var inv := Inventory.new()

	func test_handle_item_action_left_over_move_single() -> void:
		# Branch 5
		assert_false(inv_same_items.handle_item_action(0, inv_same_items, 2, true))

class TestAcrossActions extends GutTest:
	pass
