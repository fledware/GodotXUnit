tool
extends Label
class_name SummaryLabel

onready var text_format = text
var summary_value = 0 setget set_summary_value

func set_summary_value(value):
	summary_value = value
	text = text_format % str(summary_value)


