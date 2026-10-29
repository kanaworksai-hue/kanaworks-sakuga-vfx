extends Node3D
## Every flipbook in a row on a ground plane: columns stand on the ground, the rest float at chest height.

const INDEX := "res://addons/kanaworks_sakuga_vfx/flipbooks/index.json"
const Flipbook3D := preload("res://addons/kanaworks_sakuga_vfx/sakuga_flipbook_3d.gd")
const SPACING := 1.6
const PAUSE := 0.5

var _fx: Array = []
var _waiting := {}


func _ready() -> void:
	var data = JSON.parse_string(FileAccess.get_file_as_string(INDEX))
	if data == null:
		push_warning("SAKUGA demo: no flipbooks found at " + INDEX)
		return
	var assets: Array = data["assets"]
	var width := assets.size() * SPACING
	var env := WorldEnvironment.new()
	env.environment = Environment.new()
	env.environment.background_mode = Environment.BG_COLOR
	env.environment.background_color = Color(0.08, 0.085, 0.11)
	add_child(env)
	var sun := DirectionalLight3D.new()
	sun.rotation_degrees = Vector3(-50, 30, 0)
	add_child(sun)
	var ground := MeshInstance3D.new()
	var plane := PlaneMesh.new()
	plane.size = Vector2(width + 2.0, 4.0)
	ground.mesh = plane
	var gm := StandardMaterial3D.new()
	gm.albedo_color = Color(0.22, 0.23, 0.27)
	ground.material_override = gm
	add_child(ground)
	for i in assets.size():
		var a: Dictionary = assets[i]
		var fx := Flipbook3D.new()
		fx.sprite_frames = load(a["frames"])
		fx.name = str(a["id"])
		var x := (i - (assets.size() - 1) * 0.5) * SPACING
		var rooted := str(a["form"]) in ["column", "strike"]
		fx.position = Vector3(x, 0.0 if rooted else 0.9, 0.0)
		add_child(fx)
		_fx.append(fx)
		var label := Label3D.new()
		label.text = str(a["id"])
		label.font_size = 24
		label.pixel_size = 0.004
		label.position = Vector3(x, 0.02, 0.8)
		label.rotation_degrees = Vector3(-90, 0, 0)
		label.modulate = Color(0.8, 0.82, 0.9)
		add_child(label)
	var cam := Camera3D.new()
	var dist: float = max(5.0, (width * 0.5 + 0.5) / tan(deg_to_rad(cam.fov) * 0.5) / 1.6)
	cam.position = Vector3(0, 1.4, dist)
	cam.rotation_degrees = Vector3(-6, 0, 0)
	add_child(cam)


func _process(delta: float) -> void:
	for fx in _fx:
		if fx.is_looping() or fx.is_playing():
			continue
		_waiting[fx] = _waiting.get(fx, 0.0) + delta
		if _waiting[fx] >= PAUSE:
			_waiting[fx] = 0.0
			fx.replay()
