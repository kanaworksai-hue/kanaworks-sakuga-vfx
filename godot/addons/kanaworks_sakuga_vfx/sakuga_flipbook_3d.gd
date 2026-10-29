@tool
class_name SakugaFlipbook3D
extends MeshInstance3D
## Plays a KanaWorks SAKUGA VFX SpriteFrames (.tres) in 3D on a camera-facing quad (columns stay upright and stand
## on the node's position). Uses the same .tres as SakugaFlipbook2D: the sheet, grid and pivot come from its metadata.
## Credit wherever you use it: VFX: KanaWorks_AI (@KanaWorks_AI)  (code: MIT)

signal finished

enum Billboard { AUTO, NONE, FACE_CAMERA, UPRIGHT }
enum Blend { SHEET, ALPHA, ADDITIVE }
enum Looping { SHEET, ONCE, LOOP }
enum EndAction { HIDE, FREE, HOLD_LAST_FRAME }

const SHADER := preload("sakuga_flipbook_3d.gdshader")

@export var sprite_frames: SpriteFrames:
	set(value):
		sprite_frames = value
		_rebuild()
## Metres per sheet pixel (default: 256 px = 1 m).
@export var pixel_size := 1.0 / 256.0:
	set(value):
		pixel_size = value
		_rebuild()
## Frames per second; 0 = the sheet's fps.
@export var fps := 0.0
@export var looping: Looping = Looping.SHEET
@export var billboard: Billboard = Billboard.AUTO:
	set(value):
		billboard = value
		_rebuild()
@export var blend: Blend = Blend.SHEET:
	set(value):
		blend = value
		_rebuild()
@export var tint := Color.WHITE:
	set(value):
		tint = value
		if _mat:
			_mat.set_shader_parameter("tint", tint)
@export var play_on_ready := true
@export var end_action: EndAction = EndAction.HIDE
## Frame shown in the editor.
@export var preview_frame := 16:
	set(value):
		preview_frame = value
		if Engine.is_editor_hint():
			_show(value)

var _mat: ShaderMaterial
var _info := {}
var _time := 0.0
var _playing := false
var current_frame := -1


## Instance, add and play a flipbook. With FREE (the default) it removes itself when done.
static func spawn(frames: SpriteFrames, parent: Node, at: Vector3, end := EndAction.FREE) -> SakugaFlipbook3D:
	var fx := SakugaFlipbook3D.new()
	fx.sprite_frames = frames
	fx.end_action = end
	parent.add_child(fx)
	fx.global_position = at
	return fx


func _ready() -> void:
	if _mat == null:
		_rebuild()
	if play_on_ready and not Engine.is_editor_hint():
		replay()


func frame_count() -> int:
	return int(_info.get("frames", 1))


func is_looping() -> bool:
	if looping == Looping.SHEET:
		return bool(_info.get("loop", false))
	return looping == Looping.LOOP


func is_playing() -> bool:
	return _playing


func replay() -> void:
	_time = 0.0
	_playing = true
	visible = true
	_show(0)


func stop() -> void:
	_playing = false
	visible = false


func _process(delta: float) -> void:
	if not _playing or Engine.is_editor_hint():
		return
	_time += delta
	var rate: float = fps if fps > 0.0 else float(_info.get("fps", 30.0))
	var f := int(floor(_time * rate + 1e-4))
	var n := frame_count()
	if is_looping():
		f = f % n
	elif f >= n:
		_finish()
		return
	_show(f)


## Show one frame (-1 hides it). For drivers such as SakugaEffect3D: call stop() first so it does not play itself.
func show_frame(f: int) -> void:
	visible = f >= 0
	if f >= 0:
		_show(f)


func _finish() -> void:
	_playing = false
	match end_action:
		EndAction.FREE:
			queue_free()
		EndAction.HIDE:
			visible = false
		_:
			_show(frame_count() - 1)
	finished.emit()


func _show(f: int) -> void:
	current_frame = f
	if _mat:
		_mat.set_shader_parameter("frame", float(clampi(f, 0, max(frame_count() - 1, 0))))


func _rebuild() -> void:
	if sprite_frames == null:
		mesh = null
		material_override = null
		_mat = null
		return
	_info = sprite_frames.get_meta("sakuga", {})
	var tex: Texture2D = null
	if sprite_frames.get_frame_count(&"default") > 0:
		var first := sprite_frames.get_frame_texture(&"default", 0)
		tex = (first as AtlasTexture).atlas if first is AtlasTexture else first
	var cell: Array = _info.get("cell", [256, 256])
	var pivot: Array = _info.get("pivot", [0.5, 0.5])
	var grid: Array = _info.get("grid", [1, 1])
	var size := Vector2(float(cell[0]), float(cell[1])) * pixel_size
	var quad := QuadMesh.new()
	quad.size = size
	quad.center_offset = Vector3((0.5 - float(pivot[0])) * size.x, (float(pivot[1]) - 0.5) * size.y, 0.0)
	mesh = quad
	var reach := size.length()
	custom_aabb = AABB(-Vector3.ONE * reach, Vector3.ONE * reach * 2.0)   # the quad turns around the pivot
	var mode := billboard
	if mode == Billboard.AUTO:
		mode = Billboard.UPRIGHT if str(_info.get("form", "")) in ["column", "strike"] else Billboard.FACE_CAMERA
	var add := blend == Blend.ADDITIVE or (blend == Blend.SHEET and str(_info.get("blend", "alpha")) == "additive")
	_mat = ShaderMaterial.new()
	_mat.shader = SHADER
	_mat.set_shader_parameter("flipbook", tex)
	_mat.set_shader_parameter("grid", Vector3(float(grid[0]), float(grid[1]), float(frame_count())))
	_mat.set_shader_parameter("billboard", {Billboard.NONE: 0, Billboard.FACE_CAMERA: 1, Billboard.UPRIGHT: 2}[mode])
	_mat.set_shader_parameter("additive", 1.0 if add else 0.0)
	_mat.set_shader_parameter("tint", tint)
	material_override = _mat
	cast_shadow = GeometryInstance3D.SHADOW_CASTING_SETTING_OFF
	_show(preview_frame if Engine.is_editor_hint() else 0)
