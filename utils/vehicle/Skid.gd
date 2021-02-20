extends Spatial

func _ready():
	yield(get_tree().create_timer(10), "timeout")
	queue_free()