extends Tree

var res := load("res://resources/spells/fire_ball.tres")

func _ready() -> void:
	var root := create_item(null)

	_create_object_property(root, "Spell", res)

func _create_bool_property(p_parent: TreeItem, p_name: StringName, p_value: bool) -> TreeItem:
	var item: TreeItem = create_item(p_parent)
	item.set_text(0, p_name)
	item.collapsed = true
	item.set_cell_mode(1, TreeItem.CELL_MODE_CHECK)
	item.set_text(1, "On")
	item.set_checked(1, p_value)
	
	return item


func _create_int_property(p_parent: TreeItem, p_source: Object, p_name: StringName, p_value: int) -> TreeItem:
	var item: TreeItem = create_item(p_parent)
	item.set_text(0, p_name)
	item.collapsed = true
	item.set_cell_mode(1, TreeItem.CELL_MODE_RANGE)
	item.set_range(1, p_value)
	
	return item


func _create_string_property(p_parent: TreeItem, p_source: Object, p_name: StringName, p_value: String) -> TreeItem:
	var item: TreeItem = create_item(p_parent)
	item.set_text(0, p_name)
	item.collapsed = true
	item.set_cell_mode(1, TreeItem.CELL_MODE_STRING)
	item.set_text(1, p_value)
	return item

func _create_array_property(p_parent: TreeItem, p_source: Array, p_name: StringName) -> TreeItem:
	var item: TreeItem = create_item(p_parent)
	item.set_text(0, p_name)
	item.collapsed = true
	item.set_cell_mode(1, TreeItem.CELL_MODE_STRING)
	item.set_text(1, "Array")
	
	return item

func _create_object_property(p_parent: TreeItem, p_name: StringName, p_value: Object) -> TreeItem:
	var item: TreeItem = create_item(p_parent)
	item.collapsed = true
	
	item.set_text(0, p_name)
	item.set_cell_mode(1, TreeItem.CELL_MODE_STRING)
	
	if p_value == null:
		item.set_text(1, "<null>")
	else:
		#
		var script := p_value.get_script() as Script
		
		if script:
			item.set_text(1, script.get_global_name() + ": " + str(p_value.get_instance_id()))
		else:
			item.set_text(1, str(p_value).substr(1, str(p_value).find("#") - 1)  + ": " + str(p_value.get_instance_id()))
			
	var dummy := create_item(item)
	
	item_collapsed.connect(func (p_tree_item: TreeItem) -> void:
		if not p_tree_item == item:
			return
		
		if not dummy.visible:
			return
		
		if p_value == null:
			return
		
		_create_property(item, p_value)
		
		dummy.visible = false
	,CONNECT_DEFERRED | CONNECT_ONE_SHOT)
	return item

func _create_property(p_parent: TreeItem, p_value: Object) -> void:
	var props := p_value.get_property_list()
	var i := props.size() - 1
	
	while i >= 0:
		var prop: Dictionary = props[i]
		if prop["type"] == TYPE_INT:
			_create_int_property(p_parent, p_value, prop["name"], p_value.get(prop["name"]))
		elif prop["type"] == TYPE_BOOL:
			_create_bool_property(p_parent, prop["name"], p_value.get(prop["name"]))
		elif prop["type"] == TYPE_STRING || prop["type"] == TYPE_STRING_NAME:
			_create_string_property(p_parent, p_value, prop["name"], p_value.get(prop["name"]))
		elif prop["type"] == TYPE_OBJECT:
			_create_object_property(p_parent, prop["name"], p_value.get(prop["name"]))
		elif prop["type"] == TYPE_ARRAY:
			var item := _create_array_property(p_parent, p_value.get(prop["name"]), prop["name"])
			var j := 0
			for ele: Variant in p_value.get(prop["name"]):
				_create_object_property(item, str(j), ele)
				#_create_property(item, ele)
				j += 1
		i -= 1
	return
