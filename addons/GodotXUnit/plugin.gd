tool
extends EditorPlugin

var dock_scene = preload("res://addons/GodotXUnit/editor/XUnitDock.tscn")
var dock_instance = null

func get_plugin_name() -> String:
	return "GodotXUnit"

func _enter_tree() -> void:
	preload("editor/plugin_init.gd").new()
	dock_instance = dock_scene.instance()
	dock_instance.plugin_parent = self
	add_control_to_bottom_panel(dock_instance, get_plugin_name())

func _exit_tree() -> void:
	if dock_instance != null:
		remove_control_from_bottom_panel(dock_instance)
		dock_instance.free()
		dock_instance = null
