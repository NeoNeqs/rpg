#@tool
#class_name AttributeView
#extends UIELement
#
#@export var list: RichTextLabel
#@export var title: String:
	#set(v):
		#if not is_node_ready():
			#await ready
		#
		#var label := $VBoxContainer/Label
		#
		#label.text = v
	#get:
		#if not is_node_ready():
			#await ready
		#var label := $VBoxContainer/Label
		#
		#return label.text
#
#
#
#func update(p_attributes: Attributes) -> void:
	#list.clear()
	#
	#for attr: StringName in Attributes._attributes:
		#var val: Variant = p_attributes.get(attr)
		#if not attr.contains("_"):
			#
			#list.append_text("%s: %d\n" % [attr, val])
