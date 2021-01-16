tool
extends EditorPlugin

func _enter_tree():
	add_custom_type("TerrainLoader", "Spatial", preload("res://addons/TerrainLoader/tll.gd"), preload("res://addons/TerrainLoader/terrain_loader.png"))

func _exit_tree():
	remove_custom_type("TerrainLoader")