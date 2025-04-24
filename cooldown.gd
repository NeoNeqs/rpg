extends TextureProgressBar


func start(p_time_usec: int) -> void:
	value = 100
	var tween: Tween = create_tween()
	tween.tween_property(self, "value", 0.0, p_time_usec / 1_000_000.0)
