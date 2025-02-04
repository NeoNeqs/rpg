@tool
extends VBoxContainer

var logs_path: String = OS.get_user_data_dir().path_join("logs")

const ptrn := r"\[(\d{4}-\d{2}-\d{2}) (\d{2}:\d{2}:\d{2}.\d{5})\] \[([^]]+)\] \[([^]]+)\] \[([^]]+)\]: (.*)"
var log_regex := RegEx.create_from_string(ptrn)

var logs


# just display file contents
func _ready() -> void:
	_on_logs_pressed()
	_on_log_selected(0)


# this is ~ok~
func _on_log_selected(p_index: int) -> void:
	var file: String = %LogsOption.get_item_text(p_index)
	#%Output.text =
	var log_file := FileAccess.open(logs_path.path_join("test.log"), FileAccess.READ)
	if log_file == null:
		print("file null, error ", FileAccess.get_open_error())
		return

	logs = log_file.get_as_text()
	%Output.text = logs


func filter() -> void:
	%Output.text = ""

	var filters: Array[Query.Filter] = %Query.get_filters()
	for line in logs.split("\n"):
		var m: RegExMatch = log_regex.search(line)
		if m == null:
			if filters.size() == 0:
				%Output.add_text(line + "\n")
			continue

		for filter: Query.Filter in filters:
			if filter.matches(m.strings.slice(1)):
				%Output.add_text(line + "\n")
				break


# TODO: rename to _on_logs_option_pressed()
func _on_logs_pressed() -> void:
	var logs: PackedStringArray = DirAccess.get_files_at(logs_path)

	var selected: int = %LogsOption.get_selected_id()
	%LogsOption.clear()
	%LogsOption.add_item("(No log)")
	for log: String in logs:
		%LogsOption.add_item(log)
	%LogsOption.select(selected)


func _on_query_submitted() -> void:
	filter()
