@tool
class_name SakugaFlipbook2D
extends AnimatedSprite2D
## Plays a KanaWorks SAKUGA VFX SpriteFrames (.tres) in 2D: once or looping at the sheet's fps, with the sheet's
## pivot (ground-rooted columns stand on the node's position) and optional additive blending.
## Credit wherever you use it: VFX: KanaWorks_AI (@KanaWorks_AI)  (code: MIT)

enum EndAction { HIDE, FREE, HOLD_LAST_FRAME }

## Play from the first frame when the node enters the tree (not in the editor).
@export var play_on_ready := true
## What a one-shot does after its last frame. Loops never end.
@export var end_action: EndAction = EndAction.HIDE
## Put the sheet's pivot (from the .tres metadata) on the node's position.
@export var use_sheet_pivot := true
## Blend additively (adds light). Off = normal alpha blending.
@export var additive := false:
	set(value):
		additive = value
		_apply_blend()


## Instance, add and play a flipbook. With FREE (the default) it removes itself when done.
static func spawn(frames: SpriteFrames, parent: Node, at: Vector2, end := EndAction.FREE) -> SakugaFlipbook2D:
	var fx := SakugaFlipbook2D.new()
	fx.sprite_frames = frames
	fx.end_action = end
	fx.position = at
	parent.add_child(fx)
	return fx


func _ready() -> void:
	apply_sheet()
	if not animation_finished.is_connected(_on_finished):
		animation_finished.connect(_on_finished)
	if play_on_ready and not Engine.is_editor_hint():
		replay()


## Read pivot and blend from the SpriteFrames metadata. Call again after changing sprite_frames.
func apply_sheet() -> void:
	if sprite_frames == null:
		return
	var info: Dictionary = sprite_frames.get_meta("sakuga", {})
	if info.is_empty():
		return
	centered = true
	if use_sheet_pivot:
		var cell: Array = info.get("cell", [256, 256])
		var pivot: Array = info.get("pivot", [0.5, 0.5])
		offset = Vector2((0.5 - float(pivot[0])) * float(cell[0]), (0.5 - float(pivot[1])) * float(cell[1]))
	if str(info.get("blend", "alpha")) == "additive":
		additive = true
	_apply_blend()


func replay() -> void:
	visible = true
	frame = 0
	play(&"default")


func is_looping() -> bool:
	return sprite_frames != null and sprite_frames.get_animation_loop(&"default")


func _apply_blend() -> void:
	if additive:
		var m := CanvasItemMaterial.new()
		m.blend_mode = CanvasItemMaterial.BLEND_MODE_ADD
		material = m
	elif material is CanvasItemMaterial:
		material = null


func _on_finished() -> void:
	match end_action:
		EndAction.FREE:
			queue_free()
		EndAction.HIDE:
			visible = false
		_:
			pass
