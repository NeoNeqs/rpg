@tool
class_name ItemStack
extends Resource

# (item == null) => (quantity == 0)
# (not item == null) => (quantity > 0)

@export var item: Item = null:
	set = set_item

@export var quantity: int = 0:
	get = get_quantity,
	set = set_quantity



@export var allowed_components: Array[ItemComponent] = []:
	set(v):
		allowed_components = v
		notify_property_list_changed()


func set_item(p_new_item: Item) -> void:
	if quantity == 0:
		quantity = 1
	item = p_new_item


func get_quantity() -> int:
	if item == null:
		return 0
	
	return clampi(quantity, 0, item.stack_size)


func set_quantity(p_new_quantity: int) -> void:
	if p_new_quantity == 0:
		item = null
	quantity = p_new_quantity


func is_empty() -> bool:
	return quantity == 0
