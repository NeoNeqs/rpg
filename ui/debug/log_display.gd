#extends RichTextLabel
#
#
#const FILE_NAME: String = "godot.log"
#
#var base_dir: String = OS.get_user_data_dir().path_join("logs/")
#var handle := FileAccess.open(base_dir.path_join(FILE_NAME), FileAccess.READ)
#
#var _seek_position: int = -1
#
#func _ready() -> void:
	#update()
#
#
#func update() -> void:
	#_read_main_log()
#
#
#func _read_main_log() -> void:
	#if _seek_position == handle.get_length():
		#return
	#
	#handle.seek(_seek_position)
	#
	#while not handle.eof_reached():
		#append_text(handle.get_line())
		#if not handle.eof_reached():
			#newline()
	#
	#_seek_position = handle.get_position()
#
#
#func _on_timer_timeout() -> void:
	#update()
