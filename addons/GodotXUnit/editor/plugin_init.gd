tool
extends Reference

func _init():
	_ensure_property_target_assembly()

func _ensure_property_target_assembly():
	var info = {
		"name": "GodotXUnit/target_assembly",
		"type": TYPE_STRING,
		"hint": PROPERTY_HINT_FILE,
		"hint_string": "set the name of the test assembly, or empty string for main assembly"
	}
	_ensure_project_setting(info, "")

func _ensure_project_setting(prop_info: Dictionary, prop_default):
	print("ensuring setting: %s with default %s" % [str(prop_info), str(prop_default)])
	var name = prop_info.get("name")
	ProjectSettings.add_property_info(prop_info)
	ProjectSettings.set_initial_value(name, prop_default)
	if not ProjectSettings.has_setting(name):
		ProjectSettings.set(name, prop_default)
	ProjectSettings.save()
