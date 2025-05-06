class_name AttributeView
extends UIELement

@onready var list: RichTextLabel = $List


func _ready() -> void:
	EventBus.character_attributes_changed.connect(update)


func update(p_attributes: Attributes) -> void:
	list.clear()
	
	for attr: StringName in Attributes._attributes:
		var val: Variant = p_attributes.get(attr)
		if not attr.contains("_"):
			list.append_text("%s: %d\n" % [attr, val])
		#if typeof(val) == TYPE_INT and not val == 0:
		#	list.append_text("%s: %d\n" % [attr, val])
		#if typeof(val) == TYPE_FLOAT and not is_equal_approx(val, 1.0):
		#	list.append_text("%s: %d\n" % [attr, val])
