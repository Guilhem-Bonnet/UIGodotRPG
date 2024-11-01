extends CheckButton

@export var collapsible_container: CollapsibleContainer 
@export var check_button: CheckButton

func _ready() -> void:
	collapsible_container = $"../CollapsibleContainer"
	collapsible_container.close()
	
func _on_toggled(toggled_on: bool) -> void:
	if toggled_on:
		collapsible_container.open_tween()  # Open the container with a tween
	else:
		collapsible_container.close_tween()  # Close the container with a tween
