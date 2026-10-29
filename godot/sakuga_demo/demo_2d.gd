extends Node2D
## Every flipbook in the addon on a grid, one-shots replaying. Labels show the asset ids.

const INDEX := "res://addons/kanaworks_sakuga_vfx/flipbooks/index.json"
const Flipbook2D := preload("res://addons/kanaworks_sakuga_vfx/sakuga_flipbook_2d.gd")
const COLUMNS := 4
const CELL := Vector2(300, 330)
const PAUSE := 0.5

var _waiting := {}


func _ready() -> void:
	RenderingServer.set_default_clear_color(Color(0.08, 0.085, 0.11))
	var data = JSON.parse_string(FileAccess.get_file_as_string(INDEX))
	if data == null:
		push_warning("SAKUGA demo: no flipbooks found at " + INDEX)
		return
	var assets: Array = data["assets"]
	for i in assets.size():
		var a: Dictionary = assets[i]
		var centre := Vector2((i % COLUMNS) * CELL.x + CELL.x * 0.5, floori(i / float(COLUMNS)) * CELL.y + CELL.y * 0.5)
		var fx := Flipbook2D.new()
		fx.sprite_frames = load(a["frames"])
		fx.end_action = Flipbook2D.EndAction.HIDE
		var rooted := str(a["form"]) in ["column", "strike"]
		fx.position = centre + (Vector2(0, 130) if rooted else Vector2.ZERO)
		fx.name = str(a["id"])
		add_child(fx)
		var label := Label.new()
		label.text = str(a["id"])
		label.position = centre + Vector2(-110, 140)
		label.size = Vector2(220, 24)
		label.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
		label.modulate = Color(0.8, 0.82, 0.9)
		add_child(label)
	var cam := Camera2D.new()
	var rows := ceili(assets.size() / float(COLUMNS))
	cam.position = Vector2(min(assets.size(), COLUMNS) * CELL.x, rows * CELL.y) * 0.5
	add_child(cam)


func _process(delta: float) -> void:
	for fx in get_children():
		if not fx is AnimatedSprite2D or fx.is_looping() or fx.is_playing():
			continue
		_waiting[fx] = _waiting.get(fx, 0.0) + delta
		if _waiting[fx] >= PAUSE:
			_waiting[fx] = 0.0
			fx.replay()
