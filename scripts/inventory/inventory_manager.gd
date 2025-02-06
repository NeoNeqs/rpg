class_name InventoryManager
extends Control

@export
var _tooltip: Tooltip

@export
var _drag_slot: InventorySlot

const s_offset := Vector2(20, 0)

var selected_index: int = -1
var selected_inventory_view: InventoryView


func _init() -> void:
	EventBus.player_inventory_loaded.connect(create_inventory)


func _ready() -> void:
	set_process(false)
	create_inventory(load("res://resources/test_inventory.tres"))


func _process(_delta: float) -> void:
	if _drag_slot.visible:
		_drag_slot.global_position = get_viewport().get_mouse_position()
		_drag_slot.global_position = _drag_slot.global_position.clamp(
			Vector2.ZERO, 
			get_viewport_rect().size - _drag_slot.size * _drag_slot.scale
		)
	
	if _tooltip.visible:
		var mouse_pos: Vector2 = get_viewport().get_mouse_position()
		var pos: Vector2 = mouse_pos + s_offset
		var test_pos: Vector2 = pos + _tooltip.size
		
		if test_pos.x > get_viewport_rect().size.x:
			pos.x = mouse_pos.x - _tooltip.size.x - s_offset.x
		if test_pos.y > get_viewport_rect().size.y:
			pos.y = mouse_pos.y - _tooltip.size.y - s_offset.y
		
		_tooltip.global_position = pos
	
	if not _drag_slot.visible and not _tooltip.visible:
		set_process(false)


func create_inventory(p_inventory: Inventory) -> void:
	var inv_view: InventoryView = AssetDB.InventoryViewScene.instantiate()
	
	inv_view.set_data(p_inventory)
	inv_view.position = Vector2(200, 200)
	inv_view.slot_pressed.connect(_on_slot_pressed)
	inv_view.slot_hovered.connect(_on_slot_hovered)
	inv_view.slot_unhovered.connect(_on_slot_unhovered)
	
	add_child(inv_view)


func _on_slot_pressed(p_view: InventoryView, p_slot: InventorySlot, p_single: bool) -> void:
	if selected_index == -1:
		var item_stack := p_view.inventory.get_item_stack(p_slot.get_index())
		
		if item_stack.quantity == 0:
			return
		
		selected_index = p_slot.get_index()
		selected_inventory_view = p_view
		p_slot.select()
		DisplayServer.mouse_set_mode(DisplayServer.MOUSE_MODE_HIDDEN)
		_drag_slot.update(item_stack)
		_drag_slot.visible = true
		_tooltip.visible = false
		set_process(true)
		return
	
	var items_left: bool = selected_inventory_view.inventory.handle_item_action(
		selected_index,
		p_view.inventory,
		p_slot.get_index(),
		p_single
	)
	
	# TODO: once handle_item_action returns a state instead of bool do this:
	if items_left: # LeftOver
		var selected_item_stack := selected_inventory_view.inventory.get_item_stack(selected_index) as ItemStack
		var pressed_item_stack := p_view.inventory.get_item_stack(p_slot.get_index()) as ItemStack
		
		p_slot.update(pressed_item_stack)
		
		_drag_slot.update(selected_item_stack)
		return
	
	# This will keep the item dragged
	# if NotAllowed: return
	
	# NoLeftOver
	selected_inventory_view.get_slot(selected_index).unselect()
	selected_index = -1
	selected_inventory_view = null
	
	DisplayServer.mouse_set_mode(DisplayServer.MOUSE_MODE_VISIBLE)
	_drag_slot.visible = false
	_tooltip.visible = _tooltip.update(p_slot.get_item_stack())
	set_process(_tooltip.visible)


func _on_slot_hovered(p_slot: InventorySlot) -> void:
	if not selected_index == -1:
		return
	
	_tooltip.visible = _tooltip.update(p_slot.get_item_stack())
	set_process(true)


func _on_slot_unhovered() -> void:
	_tooltip.visible = false
