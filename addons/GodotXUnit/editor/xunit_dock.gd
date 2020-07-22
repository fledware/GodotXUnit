tool
extends MarginContainer

signal on_tests_start
signal on_test_result(details)
signal on_tests_finished

const GODOT_RUN_PLAY_SCENE = 39
const GODOT_SCENE_TAB_CLOSE = 71
const runner_scene_path = "res://addons/GodotXUnit/GodotTestRunnerScene.tscn"
const workdir = "res://addons/GodotXUnit/_runwork/"
onready var workdir_scanner = Directory.new()
onready var run_all_tests_button: Button = $Rows/Buttons/Margin/Buttons/RunAllTests
onready var details_tree: Tree = $Rows/Details/Tree
onready var summary_total_label: SummaryLabel = $Rows/Summary/Margin/Results/TotalRan
onready var summary_passed_label: SummaryLabel = $Rows/Summary/Margin/Results/Passed
onready var summary_failed_label: SummaryLabel = $Rows/Summary/Margin/Results/Failed
onready var summary_time_label: SummaryLabel = $Rows/Summary/Margin/Results/Time
var plugin_parent: EditorPlugin

func _ready():
	run_all_tests_button.connect("pressed", self, "run_all_tests")
	set_process(false)

func run_all_tests():
	plugin_parent.get_editor_interface().open_scene_from_path(runner_scene_path)
	
	var editor_node = plugin_parent.get_editor_interface().get_parent()
	# we have to make the EditorNode::_run method get called, and the only
	# way that i see to do this is to call the _menu_option with 
	# EditorNode::MenuOptions::RUN_PLAY_SCENE... but we also dont have access
	# to that enum value in the script environments.
	editor_node._menu_option(GODOT_RUN_PLAY_SCENE)
	editor_node._menu_option(GODOT_SCENE_TAB_CLOSE)

func _handle_start_tests():
	summary_total_label.summary_text = 0
	summary_passed_label.summary_text = 0
	summary_failed_label.summary_text = 0
	summary_time_label.summary_text = 0
	emit_signal("on_tests_start")
	set_process(true)

func _process(delta):
	if workdir_scanner.open(workdir) != OK:
		return
	workdir_scanner.list_dir_begin(true, true)
	var finished = false
	var file_name = workdir_scanner.get_next()
	while file_name != "":
		if file_name == "finished.json":
			finished = true
		if file_name.ends_with(".json"):
			_pull_file_and_delete(file_name)
		file_name = workdir_scanner.get_next()
	workdir_scanner.list_dir_end()
	if finished:
		set_process(false)
		emit_signal("on_tests_finished")

func _pull_file_and_delete(file_name):
	var file = File.new()
	var result = {}
	if file.open(workdir + file_name, File.READ) == OK:
		result = parse_json(file.get_as_text())
		file.close()
		_handle_test_result(result)
		emit_signal("on_test_result", result)
	workdir_scanner.remove(file_name)

func _handle_test_result(details):
	summary_total_label.summary_value += 1
	summary_time_label.summary_value += details.time
	match details.result:
		"passed":
			summary_passed_label.summary_value += 1
		"failed":
			summary_failed_label.summary_value += 1
	
