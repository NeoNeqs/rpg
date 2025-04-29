class_name ItemView
extends InventoryView



func _setup_container() -> void:
	var grid_container := container as GridContainer
	if not grid_container == null:
		grid_container.columns = inventory.columns
