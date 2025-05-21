class_name PoorTextLabel
extends VBoxContainer

static var tag_regex := RegEx.create_from_string(r"([^#]+|#[^#]*#)")


#func _ready() -> void:
	#update("Chest#color=red##right#Head")


func update(p_text: String) -> void:
	clear()
	
	var lines: PackedStringArray = p_text.split('\n')
	
	for line: String in lines:
		parse_line(line)
	

func parse_line(p_line: String) -> void:
	var current_label: Label = create_label(self)
	
	for m: RegExMatch in tag_regex.search_all(p_line):
		var groups: PackedStringArray = m.strings.slice(1)
		
		for group: String in groups:
			if group == "#right#":
				current_label = _handle_right_tag(current_label)
			elif group.begins_with("#color="):
				current_label = _handle_color_tag(current_label, group)
			elif group.begins_with("#size="):
				current_label = _handle_size_tag(current_label, group)
			else:
				current_label.text += group


func _handle_right_tag(p_current_label: Label) -> Label:
	if p_current_label.text.is_empty():
		p_current_label.size_flags_horizontal = Control.SIZE_EXPAND_FILL
		p_current_label.horizontal_alignment = HORIZONTAL_ALIGNMENT_RIGHT
		return p_current_label
	
	var hbox: Node = get_hbox(p_current_label)
	p_current_label.size_flags_horizontal = Control.SIZE_SHRINK_BEGIN
	var new_label: Label = create_label(hbox)
	new_label.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	new_label.horizontal_alignment = HORIZONTAL_ALIGNMENT_RIGHT
	
	return new_label


func _handle_color_tag(p_current_label: Label, p_tag: String) -> Label:
	var color_code: String = p_tag.substr(7, p_tag.length() - 8)
	var color := Color.from_string(color_code, Color.WHITE)
	
	if p_current_label.text.is_empty():
		p_current_label.label_settings.font_color = color
		return p_current_label
	
	var hbox: Node = get_hbox(p_current_label)
	var new_label: Label = create_label(hbox)
	new_label.label_settings.font_color = color
	return new_label


func _handle_size_tag(p_current_label: Label, p_tag: String) -> Label:
	var size_str: String = p_tag.substr(6, p_tag.length() - 7)
	var size_num: int = size_str.to_int()
	
	if p_current_label.text.is_empty():
		p_current_label.label_settings.font_size = size_num
		return p_current_label
	
	var hbox: Node = get_hbox(p_current_label)
	var new_label: Label = create_label(hbox)
	new_label.label_settings.font_size = size_num
	
	return new_label

func clear() -> void:
	for child in get_children():
		child.queue_free()
		remove_child(child)

func create_label(p_parent: Node) -> Label:
	var label := Label.new()
	label.size_flags_horizontal = Control.SIZE_SHRINK_BEGIN
	label.horizontal_alignment = HORIZONTAL_ALIGNMENT_LEFT
	label.label_settings = LabelSettings.new()
	#label.label_settings.font_size = 30
	label.label_settings.font_color = Color.WHITE
	if p_parent is HBoxContainer:
		for child in p_parent.get_children():
			child.custom_minimum_size = Vector2(0, 0)
	else:
		label.custom_minimum_size = Vector2(400.0, 0.0)
		label.autowrap_mode = TextServer.AUTOWRAP_WORD_SMART
	p_parent.add_child(label)
	return label


func get_hbox(p_relative: Label) -> HBoxContainer:
	var hbox: Node = p_relative.get_parent()
	
	if not hbox is HBoxContainer:
		hbox = HBoxContainer.new()
		hbox.add_theme_constant_override("separation", 0)
		add_child(hbox)
		p_relative.custom_minimum_size = Vector2(200.0, 0)
		p_relative.reparent(hbox)
		p_relative.autowrap_mode = TextServer.AUTOWRAP_OFF
	
	return hbox
