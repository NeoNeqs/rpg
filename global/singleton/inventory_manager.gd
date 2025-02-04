class_name InventoryManager
extends Control

@export
var _tooltip: Tooltip

@export
var _drag_slot: InventorySlot


func _init() -> void:
	EventBus.player_inventory_loaded.connect(create_inventory)


func _ready() -> void:
	set_process(false)


func _process(_delta: float) -> void:
	if _drag_slot.visible:
		_drag_slot.global_position = get_viewport().get_mouse_position()
		_drag_slot.global_position = _drag_slot.global_position.clamp(
			Vector2.ZERO, get_viewport_rect().size - _drag_slot.size * _drag_slot.scale
		)
	
	if _tooltip.visible:
		var offset_ := Vector2(_tooltip.size.x + 20, 0)
		var pos := get_viewport().get_mouse_position() - offset_
		if pos.x > 0:
			_tooltip.global_position = pos
		else:
			_tooltip.global_position = get_viewport().get_mouse_position() + Vector2(20, 0)
	
	if not _drag_slot.visible and not _tooltip.visible:
		set_process(false)


func create_inventory(p_inventory: Inventory) -> void:
	var inv_view: InventoryView = AssetDB.InventoryViewScene.instantiate()
	
	inv_view.set_data(p_inventory)
	inv_view.position = Vector2(240, 0)
	inv_view.slot_selected.connect(_on_slot_selected)
	inv_view.slot_unselected.connect(_on_slot_unselected)
	inv_view.slot_hovered.connect(_on_slot_hovered)
	inv_view.slot_unhovered.connect(_on_slot_unhovered)
	inv_view.selected_slot_changed.connect(_on_selected_slot_changed)
	add_child(inv_view)


func _on_slot_selected(p_item_stack: ItemStack) -> void:
	DisplayServer.mouse_set_mode(DisplayServer.MOUSE_MODE_HIDDEN)
	_drag_slot.update(p_item_stack)
	_drag_slot.visible = true
	_tooltip.visible = false
	set_process(true)


func _on_slot_unselected() -> void:
	DisplayServer.mouse_set_mode(DisplayServer.MOUSE_MODE_VISIBLE)
	_drag_slot.visible = false
	_tooltip.visible = true


func _on_slot_hovered(p_slot: InventorySlot) -> void:
	if _tooltip.update(p_slot.get_item_stack()):
		_tooltip.visible = true
	set_process(true)


func _on_slot_unhovered() -> void:
	_tooltip.visible = false


func _on_selected_slot_changed(p_item_stack: ItemStack) -> void:
	_drag_slot.update(p_item_stack)
