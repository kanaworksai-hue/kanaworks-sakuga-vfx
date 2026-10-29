extends Node3D
## Layered effects, one at a time on a small stage: Left / Right for the previous / next effect, Space to replay.
## Projectiles fly across into their paired impact. Contact-frame shakes move the camera.

const INDEX := "res://addons/kanaworks_sakuga_vfx/flipbooks/index.json"

var ids: Array = []
var index := 0
var current: SakugaEffect3D
var label: Label3D
var cam: Camera3D
var shake := 0.0
var shake_left := 0.0
var shake_dur := 0.0
var cam_home := Vector3(0, 2.6, 7.5)
var fly_from := Vector3(-3.2, 1.2, 0)
var fly_to := Vector3(3.2, 1.2, 0)
var flying := 0.0
var wait := -1.0


func _ready() -> void:
	var data = JSON.parse_string(FileAccess.get_file_as_string(INDEX))
	if data == null:
		push_warning("SAKUGA demo: run python tools/sakuga.py install godot <project> first")
		return
	for a in data["assets"]:
		ids.append(str(a["id"]))
	var env := WorldEnvironment.new()
	env.environment = Environment.new()
	env.environment.background_mode = Environment.BG_COLOR
	env.environment.background_color = Color(0.09, 0.09, 0.14)
	env.environment.ambient_light_color = Color(0.35, 0.36, 0.5)
	env.environment.glow_enabled = true
	env.environment.tonemap_mode = Environment.TONE_MAPPER_FILMIC
	add_child(env)
	var sun := DirectionalLight3D.new()
	sun.rotation_degrees = Vector3(-52, -35, 0)
	sun.shadow_enabled = true
	add_child(sun)
	var ground := MeshInstance3D.new()
	var plane := PlaneMesh.new()
	plane.size = Vector2(24, 16)
	ground.mesh = plane
	var gm := StandardMaterial3D.new()
	gm.albedo_color = Color(0.32, 0.32, 0.4)
	ground.material_override = gm
	add_child(ground)
	var dummy := MeshInstance3D.new()
	dummy.mesh = CapsuleMesh.new()
	dummy.position = Vector3(0, 1, -0.7)
	add_child(dummy)
	cam = Camera3D.new()
	cam.position = cam_home
	add_child(cam)
	cam.look_at(Vector3(0, 1.2, 0))
	label = Label3D.new()
	label.pixel_size = 0.005
	label.position = Vector3(0, 3.6, 0)
	add_child(label)
	show_effect(0)


func show_effect(i: int) -> void:
	if ids.is_empty():
		return
	index = posmod(i, ids.size())
	if is_instance_valid(current):
		current.queue_free()
	var id: String = ids[index]
	current = SakugaEffect3D.new()
	current.effect_id = id
	current.lifetime = 3.0
	current.shake_requested.connect(func(a, d): shake = a; shake_dur = d; shake_left = d)
	add_child(current)
	var ground: bool = str(current.recipe.get("anchor", "center")) == "ground"
	current.position = Vector3(0, 0 if ground else 1.1, 0)
	flying = 0.0
	if current.category() == "projectile":
		current.position = fly_from
		flying = 0.001
	label.text = "%d/%d  %s  (%s)" % [index + 1, ids.size(), id, current.category()]
	current.finished.connect(func(): wait = 0.6)


func _process(delta: float) -> void:
	if flying > 0.0 and is_instance_valid(current):
		flying += delta
		current.position = fly_from.lerp(fly_to, clampf(flying * 14.0 / fly_from.distance_to(fly_to), 0, 1))
		if current.position.is_equal_approx(fly_to):
			flying = 0.0
			var impact = current.recipe.get("pairs", {}).get("impact")
			current.stop()
			if impact != null:
				var hit := SakugaEffect3D.spawn(str(impact), self, fly_to)
				hit.shake_requested.connect(func(a, d): shake = a; shake_dur = d; shake_left = d)
	if wait >= 0.0:
		wait -= delta
		if wait < 0.0:
			show_effect(index)
	if shake_left > 0.0:
		shake_left -= delta
		var k := pow(shake_left / maxf(shake_dur, 0.01), 2.0)
		cam.position = cam_home + Vector3(sin(shake_left * 90.0), cos(shake_left * 77.0), 0) * shake * k
	else:
		cam.position = cam_home


func _unhandled_input(event: InputEvent) -> void:
	if event.is_action_pressed("ui_right"):
		show_effect(index + 1)
	elif event.is_action_pressed("ui_left"):
		show_effect(index - 1)
	elif event.is_action_pressed("ui_accept"):
		show_effect(index)
