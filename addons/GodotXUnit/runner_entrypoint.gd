extends Node

var runner_script = preload("res://addons/GodotXUnit/GodotTestRunner.cs")

func _process(delta):
	if not has_node("/root/GodotTestRunner"):
		var runner = Node.new()
		runner.set_script(runner_script)
		runner.name = "GodotTestRunner"
		get_tree().root.add_child(runner)
	set_process(false)
