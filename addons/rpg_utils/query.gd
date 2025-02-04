@tool
class_name Query
extends LineEdit

signal submitted

var _filter_regex := RegEx.create_from_string(r'(\w+):\s*(?:"([^"]+)"|(\S+))')


func _split_filters(p_str: String) -> PackedStringArray:
	var result := PackedStringArray()

	var i: int = 0
	var from: int = i
	var _search_str := " or "
	var _search_str_len: int = _search_str.length()
	var is_inside_quote: bool = false

	while i < p_str.length():
		if p_str[i] == '"':
			is_inside_quote = not is_inside_quote
		elif not is_inside_quote and p_str.substr(i, _search_str_len) == _search_str:
			result.append(p_str.substr(from, i - from).strip_edges())
			from = i + _search_str.length()
			i = from - 1
		i += 1

	result.append(p_str.substr(from, i - from).strip_edges())
	return result


func _make_filter(p_filter_str: String) -> Filter:
	var filter := Filter.new()
	for m: RegExMatch in _filter_regex.search_all(p_filter_str):
		filter.set(m.strings[1], m.strings[2] + m.strings[3])

	return filter


func get_filters() -> Array[Filter]:
	var filters: Array[Filter] = []
	for filter: String in _split_filters(text):
		filters.append(_make_filter(filter))

	return filters


func _on_text_changed(new_text: String) -> void:
	submitted.emit()


class Filter:
	extends RefCounted

	# Following properties must not share a common beginning of their name
	# in order for aliasing to work
	# E.g: `time` would collide with `thread` when `t` alias is used
	var after: String
	var before: String
	var exact: String
	var level: String
	var category: String
	var thread: String
	var msg: String

	func matches(p_what: Array[String]) -> bool:
		var match_date_after: bool = after.is_empty() or (p_what[0] >= after)
		var match_date_before: bool = before.is_empty() or (p_what[0] >= before)
		var match_date: bool = (match_date_after and match_date_before) or exact == p_what[0]
		var match_level: bool = level.is_empty() or level == p_what[2]
		var match_category: bool = category.is_empty() or category == p_what[3]
		var match_thread: bool = thread.is_empty() or thread == p_what[4]
		var match_message: bool = msg.is_empty() or msg in p_what[5]

		return match_date and match_level and match_category and match_thread and match_message

	# Handles aliases
	func _set(p_prop: StringName, value: Variant) -> bool:
		for prop: Dictionary in get_property_list():
			if not prop["usage"] == PROPERTY_USAGE_SCRIPT_VARIABLE:
				continue
			if prop["name"].begins_with(p_prop):
				set(prop["name"], value)
				return true

		return false
