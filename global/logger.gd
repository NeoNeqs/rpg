#@tool
#class_name Logger
#extends RefCounted
#
#enum LogLevel {
	#Debug,
	#Info,
	#Warn,
	#Error,
	#Critical,
#}
#
#static var core: Logger = Logger.new("Core", LogLevel.Debug)
#static var combat: Logger = Logger.new("Combat", LogLevel.Debug)
#static var ui: Logger = Logger.new("UI", LogLevel.Debug)
#static var physics: Logger = Logger.new("Physics", LogLevel.Debug)
#static var input: Logger = Logger.new("Input", LogLevel.Debug)
#
#const message_format: String = "[{}] [{}] [{}] [{}]: {}"
#const placeholder: String = "{}"
#
#var _level: LogLevel = LogLevel.Debug
#var _category: String
#
#
#static func _static_init() -> void:
	#_print_info()
#
#
#func _init(p_category: String, p_level: LogLevel) -> void:
	#_category = p_category
	#_level = p_level
#
#
#func debug(p_message: String, p_values: Array = []) -> void:
	#if not OS.is_debug_build():
		#return
#
	#_log(LogLevel.Debug, p_message, p_values)
	#_log_stacktrace()
#
#
#func info(p_message: String, p_values: Array = []) -> void:
	#_log(LogLevel.Info, p_message, p_values)
#
#
#func warn(p_message: String, p_values: Array = []) -> void:
	#_log(LogLevel.Warn, p_message, p_values)
#
#
#func error(p_message: String, p_values: Array = []) -> void:
	#_log(LogLevel.Error, p_message, p_values)
#
#
#func critical(p_message: String, p_values: Array = []) -> void:
	#_log(LogLevel.Critical, p_message, p_values)
	#assert(false, p_message.format(p_values, placeholder))
#
#
#func _log(p_level: LogLevel, p_message: String, p_values: Array) -> void:
	#if int(p_level) < int(_level):
		#return
#
	#var datetime: String = _get_datetime_now()
	#var level_str: String = _level_to_string(p_level)
	#var thread_str: String = _get_thread_string()
#
	#p_message = p_message.format(p_values, placeholder)
#
	#p_message = message_format.format([datetime, level_str, _category, thread_str, p_message], placeholder)
#
	#if OS.has_feature("editor"):
		#print_rich(_format_color(p_message, p_level))
	#else:
		#print(p_message)
#
#
#func _log_stacktrace() -> void:
	#var format_str: StringName = &"\tat: {source} -> {function}:{line}"
#
	## Slice off first 2 entries (call to `_log_stacktrace()` and `debug()`)
	#for ele: Dictionary in get_stack().slice(2):
		#var trace: String = format_str.format(ele)
		#print_rich(_format_color(trace, _level))
#
#
#func _get_datetime_now() -> String:
	#var unix_time: float = Time.get_unix_time_from_system()
	#var unix_time_sec: int = int(unix_time)
	#var milliseconds: float = unix_time - unix_time_sec
#
	#var formatted: String = Time.get_datetime_string_from_unix_time(unix_time_sec, true)
#
	#return formatted + str(milliseconds).substr(1, 6)
#
#
#func _level_to_string(p_level: LogLevel) -> String:
	#return LogLevel.keys()[p_level].to_upper()
#
#
#func _format_color(p_message: String, p_level: LogLevel) -> String:
	#var format_str: StringName = &"[color={}]{}[/color]"
#
	#var color: StringName
	#match p_level:
		#LogLevel.Debug:
			#color = &"cyan"
		#LogLevel.Info:
			#color = &"green"
		#LogLevel.Warn:
			#color = &"yellow"
		#LogLevel.Error:
			#color = &"orangered"
		#LogLevel.Critical:
			#color = &"red"
		#_:
			#color = &"white"
#
	#return format_str.format([color, p_message], placeholder)
#
#
#func _get_thread_string() -> String:
	#if OS.get_main_thread_id() == OS.get_thread_caller_id():
		#return &"Main"
	#
	#return &"Thread/" + str(OS.get_thread_caller_id())
#
#
## TODO: Move this function to main scene and call it there instead
## or have a LogManager class that will do it instaed
#static func _print_info() -> void:
	#if not OS.has_feature("template"):
		#return
	#core.info("--------------------System Information--------------------")
	#core.info("OS: {} {}, Locale: {}", [OS.get_distribution_name(), OS.get_version(), OS.get_locale()])
	#core.info("CPU: {}, ({}-core)", [OS.get_processor_name(), OS.get_processor_count()])
	#core.info(
		#"GPU: {}, API: {} ({})",
		#[
			#RenderingServer.get_rendering_device().get_device_name(),
			#ProjectSettings.get_setting("rendering/rendering_device/driver").capitalize(),
			#RenderingServer.get_video_adapter_api_version()
		#]
	#)
	#var mem_info: Dictionary = OS.get_memory_info()
	#core.info("Memory: {} + {} Swap", [String.humanize_size(mem_info["physical"]), String.humanize_size(mem_info["available"] - mem_info["physical"])])
	#core.info("Stack size: {}", [String.humanize_size(mem_info["stack"])])
#
	#core.info("--------------------Process Information--------------------")
	#core.info("Executable path: {}", [OS.get_executable_path()])
	#core.info("Engine arguments: {}", [OS.get_cmdline_args()])
	#core.info("User arguments: {}", [OS.get_cmdline_user_args()])
#
	#core.info("--------------------Storage Information--------------------")
	#core.info("Is 'user://' persistent: {}", [OS.is_userfs_persistent()])
	#core.info("User data dir: {}", [OS.get_user_data_dir()])
	#core.info("Config dir: {}", [OS.get_config_dir()])
	#core.info("Cache dir: {}", [OS.get_cache_dir()])
	#core.info("Data dir: {}", [OS.get_data_dir()])
#
	#core.info("--------------------Misc Information--------------------")
	#core.info("Is sandboxed: {}", [OS.is_sandboxed()])
	## TODO:
	##core.info("VRAM usage: {}", [RenderingServer.get_rendering_device().get_memory_usage()])
