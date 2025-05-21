class_name Cooldown
extends TextureProgressBar

var _tween: Tween 

func start(p_time_usec: int) -> void:
	if _tween and _tween.is_valid():
		_tween.kill()
	value = 100
	_tween = create_tween()
	_tween.tween_property(self, "value", 0.0, p_time_usec / 1_000_000.0)

func reset() -> void:
	value = 0
	if _tween and _tween.is_valid():
		_tween.kill()
