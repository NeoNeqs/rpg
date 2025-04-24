class_name ItemView
extends InventoryView



func _setup_container() -> void:
	if container is GridContainer:
		(container as GridContainer).columns = inventory.columns
