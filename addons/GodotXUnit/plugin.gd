tool
extends EditorPlugin

func get_plugin_name() -> String:
	return "GodotXUnit"

func _enter_tree() -> void:
	preload("editor/plugin_init.gd").new()
	#_ControlPanel = ControlPanel.instance()
	#_TestMetadataEditor = TestMetadataEditor.new()
	#add_inspector_plugin(_TestMetadataEditor)
	#_DockController = DockController.new(self, _ControlPanel)
	#add_child(_DockController)

	
func _exit_tree() -> void:
	#_DockController.free()
	#_ControlPanel.free()
	#remove_inspector_plugin(_TestMetadataEditor)
	pass
