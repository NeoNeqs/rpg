extends GutTest


func test_default_init() -> void:
	var _is := ItemStack.new()
	assert_null(_is.item)
	assert_eq(_is.quantity, 0)


func test_set_item() -> void:
	var _is := ItemStack.new()
	_is.item = Item.new()
	assert_not_null(_is.item)
	assert_eq(_is.quantity, 1)
	
func test_set_item_2_quantity() -> void:
	var _is := ItemStack.new()
	_is.item = Item.new()
	_is.item.stack_size = 2
	_is.quantity += 1
	assert_not_null(_is.item)
	assert_eq(_is.quantity, 2)

func test_stack_size() -> void:
	var _is := ItemStack.new()
	_is.item = Item.new()
	_is.item.stack_size = 1
	_is.quantity += 1
	assert_not_null(_is.item)
	assert_eq(_is.quantity, 1)

func test_item_is_set_to_null() -> void:
	var _is := ItemStack.new()
	_is.item = Item.new()
	_is.item = null
	assert_null(_is.item)
	assert_eq(_is.quantity, 0)


func test_quantity_set_to_0() -> void:
	var _is := ItemStack.new()
	_is.item = Item.new()
	_is.quantity = 0
	assert_null(_is.item)
	assert_eq(_is.quantity, 0)


func test_quantity_set_when_item_null() -> void:
	var _is := ItemStack.new()
	_is.item = null
	_is.quantity = 2
	assert_null(_is.item)
	assert_eq(_is.quantity, 0)
