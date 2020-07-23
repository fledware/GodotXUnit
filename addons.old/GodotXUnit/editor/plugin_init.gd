tool
extends Reference

var requires_save = false

func _init():
	_ensure_property_target_assembly()
	_ensure_property_results_summary()
	
	if requires_save:
		print(str(ProjectSettings.save()))

func _ensure_property_target_assembly():
	var info = {
		"name": "GodotXUnit/target_assembly",
		"type": TYPE_STRING,
		"hint": PROPERTY_HINT_FILE,
		"hint_string": "set the name of the test assembly, or empty string for main assembly"
	}
	_ensure_project_setting(info, "")

func _ensure_property_results_summary():
	var info = {
		"name": "GodotXUnit/results_summary",
		"type": TYPE_STRING,
		"hint": PROPERTY_HINT_FILE,
		"hint_string": "sets where you want the test file to be created"
	}
	_ensure_project_setting(info, "res://TestSummary.json")

func _ensure_project_setting(prop_info: Dictionary, prop_default):
	print(prop_info.name + " => " + str(ProjectSettings.get_setting(prop_info.name)) + 
		" => " + str(ProjectSettings.has_setting(prop_info.name)))
	if not ProjectSettings.has_setting(prop_info.name):
		print("ensuring setting: %s with default %s" % [str(prop_info), str(prop_default)])
		# is it set_setting or set? neither seem to work...
		ProjectSettings.set_setting(prop_info.name, prop_default)
		#ProjectSettings.set(prop_info.name, prop_default)
		ProjectSettings.add_property_info(prop_info)
		ProjectSettings.set_initial_value(prop_info.name, prop_default)
		requires_save = true
	print(prop_info.name + " => " + str(ProjectSettings.get_setting(prop_info.name)) + 
		" => " + str(ProjectSettings.has_setting(prop_info.name)))
