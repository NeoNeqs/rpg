class_name Slot
extends Control

signal left_pressed()
signal right_pressed()
signal hovered()
signal unhovered()


@export var _text_holder: Label
@export var _icon_holder: TextureRect
@export var _border_holder: Panel


func _ready() -> void:
	mouse_entered.connect(hovered.emit)
	mouse_exited.connect(unhovered.emit)


func _gui_input(p_event: InputEvent) -> void:
	if p_event is InputEventMouseButton and p_event.is_released():
		match p_event.button_index:
			MOUSE_BUTTON_LEFT:
				get_viewport().set_input_as_handled()
				left_pressed.emit()
			MOUSE_BUTTON_RIGHT:
				get_viewport().set_input_as_handled()
				right_pressed.emit()


func update(p_resource: Resource) -> void:
	match p_resource:
		var item_stack when item_stack is ItemStack and not item_stack.item == null:
			_update_item(item_stack)
		var spell when spell is Spell:
			_update_spell(spell)
		_:
			_clear()


func _update_item(p_item_stack: ItemStack) -> void:
	if not _text_holder == null:
		if p_item_stack.quantity <= 1:
			_text_holder.text = ""
		else:
			_text_holder.text = str(p_item_stack.quantity)
	
	if not _icon_holder == null:
		_icon_holder.texture = p_item_stack.item.icon
	
	if not _border_holder == null:
		_set_border_color(p_item_stack.item.get_rarity_color())


func _update_spell(p_spell: Spell) -> void:
	if not _text_holder == null:
		_text_holder.text = p_spell.name
	
	if not _icon_holder == null:
		_icon_holder.texture = p_spell.icon


func _clear() -> void:
	if not _text_holder == null:
		_text_holder.text = ""
	
	if not _icon_holder == null:
		_icon_holder.texture = null
	
	if not _border_holder == null:
		_set_border_color(Color.BLACK)


func _set_border_color(p_color: Color) -> void:
	if _border_holder == null:
		# TODO: log error
		return
	
	var stylebox := _border_holder.get_theme_stylebox("panel") as StyleBoxFlat
	if stylebox == null:
		# TODO: log error
		return
	
	stylebox.border_color = p_color


func select() -> void:
	modulate = Color(1.0, 1.0, 1.0, 0.5)


func unselect() -> void:
	modulate = Color(1.0, 1.0, 1.0, 1.0)


func _get_view() -> View:
	assert(false, "Do not call this method.")
	return null


func at(p_index: int) -> Slot:
	return _get_view().inventory.at(get_index())
