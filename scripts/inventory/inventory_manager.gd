class_name InventoryManager
extends Control

@export
var _tooltip: Tooltip

@export
var _drag_slot: InventorySlot

var selected_index: int = -1
var selected_inventory_view: InventoryView


func _init() -> void:
	EventBus.player_inventory_loaded.connect(create_item_inventory)


func _ready() -> void:
	set_process(false)
	create_item_inventory(load("res://resources/test_inventory.tres"))
	create_spell_inventory(load("res://resources/spell_book.tres"))

func _process(_delta: float) -> void:
	if _drag_slot.visible:
		_drag_slot.global_position = get_viewport().get_mouse_position()
		_drag_slot.global_position = _drag_slot.global_position.clamp(
			Vector2.ZERO, 
			get_viewport_rect().size - _drag_slot.size * _drag_slot.scale
		)


func create_item_inventory(p_inventory: Inventory) -> void:
	var item_view: ItemView = AssetDB.ItemViewScene.instantiate()
	
	item_view.set_data(p_inventory)
	item_view.position = Vector2(200, 200)
	item_view.slot_pressed.connect(_on_slot_pressed)
	item_view.slot_hovered.connect(_on_slot_hovered.bind(item_view))
	item_view.slot_unhovered.connect(_on_slot_unhovered)
	
	add_child(item_view)


func create_spell_inventory(p_spell_inventory: Inventory) -> void:
	var spell_book: SpellView = AssetDB.SpellViewScene.instantiate()
	spell_book.position = Vector2(700, 200)
	spell_book.slot_pressed.connect(_on_slot_pressed)
	spell_book.slot_hovered.connect(_on_slot_hovered.bind(spell_book))
	spell_book.slot_unhovered.connect(_on_slot_unhovered)
	spell_book.set_data(p_spell_inventory)
	
	add_child(spell_book)
	
func _on_slot_pressed(p_view: InventoryView, p_slot: InventorySlot, p_single: bool) -> void:
	if selected_index == -1:
		var item_stack := p_view.inventory.get_item_stack(p_slot.get_index())
		
		if item_stack.quantity == 0:
			return
		
		_select(p_slot.get_index(), p_view)
		
		DisplayServer.mouse_set_mode(DisplayServer.MOUSE_MODE_HIDDEN)
		_drag_slot.update(item_stack)
		_drag_slot.visible = true
		_tooltip.update(null)
		set_process(true)
		p_slot.select()
		return
	
	var result: Inventory.ItemActionResult = selected_inventory_view.inventory.handle_item_action(
		selected_index,
		p_view.inventory,
		p_slot.get_index(),
		p_single
	)
	
	match result:
		Inventory.ItemActionResult.LeftOver:
			var selected_item_stack: ItemStack = selected_inventory_view.inventory.get_item_stack(selected_index)
			_drag_slot.update(selected_item_stack)
			
			var pressed_item_stack: ItemStack = p_view.inventory.get_item_stack(p_slot.get_index())
			p_slot.update(pressed_item_stack)
		Inventory.ItemActionResult.NoAction:
#		var rest when rest in [Inventory.ItemActionResult.NoAction, Inventory.ItemActionResult.NoLeftOver]:
			DisplayServer.mouse_set_mode(DisplayServer.MOUSE_MODE_VISIBLE)
			selected_inventory_view.get_slot(selected_index).unselect()
			_tooltip.update(p_view.inventory.get_item_stack(p_slot.get_index()))
			_drag_slot.visible = false
			set_process(false)
			_unselect()


func _on_slot_hovered(p_slot: InventorySlot, p_inv_view: InventoryView) -> void:
	if _is_selected():
		return
	
	_tooltip.update(p_inv_view.inventory.get_item_stack(p_slot.get_index()))
	set_process(true)


func _select(p_index: int, p_view: InventoryView) -> void:
	selected_index = p_index
	selected_inventory_view = p_view


func _unselect() -> void:
	selected_inventory_view = null
	selected_index = -1


func _is_selected() -> bool:
	return not selected_index == -1


func _on_slot_unhovered() -> void:
	_tooltip.update(null)
