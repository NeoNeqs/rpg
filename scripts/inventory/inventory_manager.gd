class_name InventoryManager
extends Control

@export
var _tooltip: Tooltip

@export
var _drag_slot: Slot

const s_offset := Vector2(20, 0)

var selected_index: int = -1
var selected_view: View


func _init() -> void:
	EventBus.player_inventory_loaded.connect(create_inventory)


func _ready() -> void:
	set_process(false)
	create_inventory(load("res://resources/test_inventory.tres"))

	$SpellBook.slot_pressed.connect(_on_slot_pressed)
	$SpellBook.slot_hovered.connect(_on_spell_hovered)
	$SpellBook.slot_unhovered.connect(_on_slot_unhovered)

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
	var item_view: View = AssetDB.InventoryViewScene.instantiate()
	
	item_view.set_data(p_inventory)
	item_view.position = Vector2(200, 200)
	item_view.slot_pressed.connect(_on_slot_pressed)
	item_view.slot_hovered.connect(_on_slot_hovered)
	item_view.slot_unhovered.connect(_on_slot_unhovered)
	
	add_child(item_view)

func select(p_view: View, p_slot: Slot, p_index: int) -> void:
	selected_index = p_index
	selected_view = p_view
	DisplayServer.mouse_set_mode(DisplayServer.MOUSE_MODE_HIDDEN)


func unselect() -> void:
	selected_index = -1
	selected_view = null
	
	DisplayServer.mouse_set_mode(DisplayServer.MOUSE_MODE_VISIBLE)


func _on_slot_pressed(p_view: View, p_slot: Slot, p_single: bool) -> void:
	var slot_index: int = p_slot.get_index()
	
	var element: Resource = p_view.get_element_at(slot_index)
	if not is_selected():
		
		if p_view.is_empty_at(slot_index):
			return
		
		select(p_view, p_slot, slot_index)
		p_slot.select()
		
		_drag_slot.update(element)
		_drag_slot.visible = true
		_tooltip.visible = false
		set_process(true)
		return
	

	#if selected_view._inventory is ItemInventory:
	var result: Inventory.ActionResult = selected_view._inventory.handle_item_action(
		selected_index,
		p_view._inventory,
		p_slot.get_index(),
		p_single
	)
	
	if result == Inventory.ActionResult.NotAllowed:
		pass
	elif result == Inventory.ActionResult.Allowed:
		var selected_element: Resource = selected_view.get_element_at(selected_index)
		var pressed_element: Resource = p_view.get_element_at(slot_index)
		
		p_slot.update(pressed_element)
		
		_drag_slot.update(selected_element)
	elif result == Inventory.ActionResult.Invalid:
		pass
	elif result == Inventory.ActionResult.NoLeftover:
		selected_view.get_slot(selected_index).unselect()
		unselect()
		_drag_slot.visible = false
		
		if element is ItemStack:
			_tooltip.visible = _tooltip.update(p_view.get_element_at(slot_index).item)
		else:
			_tooltip.visible = _tooltip.update(p_view.get_element_at(slot_index))
		set_process(_tooltip.visible)
	

# TODO: the mess below....

func _on_slot_hovered(p_view: View, p_slot: Slot) -> void:
	if is_selected():
		return
	
	# TODO: move visiblity inside tooltip class
	_tooltip.visible = _tooltip.update(p_view.get_element_at(p_slot.get_index()).item)
	set_process(true)

func is_selected() -> bool:
	return not (selected_index == -1)

func _on_slot_unhovered() -> void:
	_tooltip.visible = false

func _on_spell_hovered(p_view: View, p_spell: Slot) -> void:
	if is_selected():
		return
	_tooltip.visible = _tooltip.update(p_view.get_element_at(p_spell.get_index()))
	set_process(true)
