@tool
class_name SakugaEffect3D
extends Node3D
## A ready-to-use layered KanaWorks SAKUGA VFX effect in 3D: the flipbook plus the secondary layers from its
## recipe (contact flash, shockwave ring, sparks, debris, cel smoke, ground decal, a light with an intensity curve,
## sound) and signals for camera shake and hit stop. The recipe is the same one the Unity and web players use (it is
## in the .flipbook.json sidecar next to every sheet).
##   SakugaEffect3D.spawn("fire_impact_01", self, hit_point)
## Credit wherever you use it: VFX: KanaWorks_AI (@KanaWorks_AI)  (code: MIT)

signal contact
signal shake_requested(amplitude: float, duration: float)
signal hit_stop(seconds: float)
signal finished

enum Size { S, M, L }
const SIZES := [0.6, 1.0, 1.6]
const ROOT := "res://addons/kanaworks_sakuga_vfx/"
const INDEX := ROOT + "flipbooks/index.json"
const SHARED := ROOT + "shared/"
const Flipbook3D := preload("sakuga_flipbook_3d.gd")

@export var effect_id := "":
	set(value):
		effect_id = value
		if is_inside_tree():
			_build()
@export var size: Size = Size.M
@export var scale_multiplier := 1.0
@export var speed := 1.0
@export var use_color := false
@export var color := Color.WHITE
@export var play_on_ready := true
## Looping effects stop by themselves after this many seconds (0 = until stop()).
@export var lifetime := 0.0
@export var lights := true
@export var sound := true
@export var free_when_done := false

var recipe := {}
var meta := {}
var time := 0.0
var playing := false
var _contacted := false
var _stop_time := -1.0
var _main: SakugaFlipbook3D
var _flash: MeshInstance3D
var _glow: MeshInstance3D
var _ring: MeshInstance3D
var _sparks: CPUParticles3D
var _debris: CPUParticles3D
var _smoke: CPUParticles3D
var _light: OmniLight3D
var _decal: Decal
var _audio: AudioStreamPlayer3D
var _clips := {}

static var _index := {}


## Instance an effect by id under parent at a world position and play it (frees itself when done).
static func spawn(id: String, parent: Node, at: Vector3, scale_mult := 1.0) -> SakugaEffect3D:
	var fx := SakugaEffect3D.new()
	fx.effect_id = id
	fx.scale_multiplier = scale_mult
	fx.free_when_done = true
	parent.add_child(fx)
	fx.global_position = at
	return fx


static func sidecar_path(id: String) -> String:
	if _index.is_empty() and FileAccess.file_exists(INDEX):
		var data = JSON.parse_string(FileAccess.get_file_as_string(INDEX))
		if data:
			for a in data["assets"]:
				_index[str(a["id"])] = a
	var a: Dictionary = _index.get(id, {})
	return str(a.get("sidecar", ""))


func _ready() -> void:
	_build()
	if play_on_ready and not Engine.is_editor_hint():
		play()


func category() -> String:
	return str(recipe.get("category", "impact"))


func contact_time() -> float:
	return float(recipe.get("contact_frame", 0)) / float(meta.get("fps", 30.0))


func loops() -> bool:
	return bool(meta.get("loop", false)) or recipe.get("loop_range") != null


func frame_at(t: float) -> int:
	var n := int(meta.get("frames", 1))
	var f := int(floor(t * float(meta.get("fps", 30.0)) + 1e-4))
	if t < 0.0:
		return -1
	if not loops():
		return f if f < n else -1
	var lr = recipe.get("loop_range")
	var a := 0
	var b := n - 1
	if lr != null:
		a = int(lr[0])
		b = int(lr[1])
	return f if f <= b else a + (f - a) % (b - a + 1)


func play() -> void:
	time = 0.0
	_contacted = false
	_stop_time = -1.0
	playing = true
	visible = true
	scale = Vector3.ONE * SIZES[size] * scale_multiplier
	for p in [_sparks, _debris, _smoke]:
		if p:
			p.emitting = false
	if _decal:
		_decal.visible = false
	_sfx("cast")
	var loop_clip := str(recipe.get("fx", {}).get("sfx", {}).get("loop", ""))
	if sound and _audio and _clips.has(loop_clip):
		_audio.stream = _clips[loop_clip]
		_audio.play()
	_apply()


func stop() -> void:
	if _stop_time < 0.0:
		_stop_time = time


func _process(delta: float) -> void:
	if not playing or Engine.is_editor_hint():
		return
	var before := time
	time += delta * speed
	var tc := contact_time()
	if not _contacted and time >= tc and before <= tc + 1e-4:
		_on_contact()
	if lifetime > 0.0 and loops() and _stop_time < 0.0 and time >= lifetime:
		stop()
	_apply()
	if _is_done():
		playing = false
		if _audio:
			_audio.stop()
		finished.emit()
		if free_when_done:
			queue_free()


func _on_contact() -> void:
	_contacted = true
	var fx: Dictionary = recipe.get("fx", {})
	for p in [_sparks, _debris, _smoke]:
		if p:
			p.restart()
			p.emitting = true
	if _decal:
		_decal.visible = true
	contact.emit()
	if fx.get("shake") != null:
		var sh: Dictionary = fx["shake"]
		shake_requested.emit(float(sh["amplitude"]) * sqrt(SIZES[size] * scale_multiplier), float(sh["duration"]))
	if float(fx.get("hit_stop_s", 0.0)) > 0.0:
		hit_stop.emit(float(fx["hit_stop_s"]))
	_sfx("impact")


func _sfx(kind: String) -> void:
	var name := str(recipe.get("fx", {}).get("sfx", {}).get(kind, ""))
	if not sound or not _clips.has(name) or not is_inside_tree():
		return
	var p := AudioStreamPlayer3D.new()
	p.stream = _clips[name]
	add_child(p)
	p.play()
	p.finished.connect(p.queue_free)


func _alpha() -> float:
	return 1.0 if _stop_time < 0.0 else clampf(1.0 - (time - _stop_time) / 0.25, 0.0, 1.0)


func _apply() -> void:
	var fx: Dictionary = recipe.get("fx", {})
	var age := time - contact_time()
	if _main:
		var f := frame_at(time) if _alpha() > 0.0 else -1
		_main.show_frame(f)
		_main.tint = Color(1, 1, 1, _alpha())
	if _flash:
		var life := float(fx["flash"]["frames"]) / 30.0 + 0.04
		var k := age / life
		_flash.visible = age >= 0.0 and k < 1.0
		_flash.scale = Vector3.ONE * (1.0 - 0.6 * clampf(k, 0, 1))
		_glow.visible = age >= 0.0 and age < 0.28
		(_glow.material_override as StandardMaterial3D).albedo_color.a = 0.7 * (1.0 - clampf(age / 0.28, 0, 1))
	if _ring:
		var flat := str(fx["ring"].get("plane", "view")) == "ground"
		var rl: float = float(fx["ring"]["duration"]) * (1.0 if flat else 0.5)
		var p := age / rl
		_ring.visible = age >= 0.0 and p < 1.0
		var e := 1.0 - pow(1.0 - clampf(p, 0, 1), 3.0)
		_ring.scale = Vector3.ONE * (0.15 + 0.85 * e)
		(_ring.material_override as StandardMaterial3D).albedo_color.a = (0.8 if flat else 0.45) * (1.0 - clampf(p, 0, 1))
	if _light:
		var k := 0.0
		if lights and fx.get("light") != null:
			var curve: Array = fx["light"].get("curve", [0.0, 1.0])
			if loops() or category() == "projectile":
				k = _curve(curve, age) * _alpha()
			elif age >= float(curve[0]):
				k = _curve(curve, age)
		_light.light_energy = float(fx["light"]["intensity"]) * k
		_light.visible = _light.light_energy > 0.01
	if _decal and _decal.visible:
		var life := float(fx["decal"]["life"])
		var fade := 1.0 if loops() and _stop_time < 0.0 else clampf((life - age) / (life * 0.4), 0.0, 1.0)
		_decal.modulate.a = fade
		if fade <= 0.0:
			_decal.visible = false


static func _curve(c: Array, t: float) -> float:
	if c.size() < 2:
		return 1.0
	if t <= float(c[0]):
		return float(c[1])
	var i := 2
	while i + 1 < c.size():
		var t0 := float(c[i - 2])
		var t1 := float(c[i])
		if t <= t1:
			return lerpf(float(c[i - 1]), float(c[i + 1]), (t - t0) / maxf(t1 - t0, 1e-5))
		i += 2
	return float(c[c.size() - 1])


func _is_done() -> bool:
	if loops() and _stop_time < 0.0:
		return false
	var end := float(meta.get("frames", 1)) / float(meta.get("fps", 30.0))
	if _stop_time >= 0.0:
		end = _stop_time + 0.25
	var fx: Dictionary = recipe.get("fx", {})
	var tc := contact_time()
	for k in ["sparks", "debris", "smoke"]:
		if fx.get(k) != null:
			end = maxf(end, tc + float(fx[k]["life"]) + float(fx[k].get("delay", 0.0)))
	if fx.get("decal") != null and _decal:
		end = maxf(end, tc + float(fx["decal"]["life"]))
	return time >= end


# ------------------------------------------------------------------ build
func _col(hex) -> Color:
	if use_color:
		return color
	return Color.html(str(hex)) if hex != null and str(hex) != "" else Color.WHITE


func _tex(rel: String) -> Texture2D:
	var p := SHARED + rel
	return load(p) if ResourceLoader.exists(p) else null


func _quad_mat(tex: Texture2D, add: bool, billboard: bool) -> StandardMaterial3D:
	var m := StandardMaterial3D.new()
	m.shading_mode = BaseMaterial3D.SHADING_MODE_UNSHADED
	m.transparency = BaseMaterial3D.TRANSPARENCY_ALPHA
	if add:
		m.blend_mode = BaseMaterial3D.BLEND_MODE_ADD
	m.cull_mode = BaseMaterial3D.CULL_DISABLED
	m.albedo_texture = tex
	if billboard:
		m.billboard_mode = BaseMaterial3D.BILLBOARD_ENABLED
	m.vertex_color_use_as_albedo = true
	return m


func _quad(n: String, tex: Texture2D, size_m: float, col: Color, add: bool, billboard := true) -> MeshInstance3D:
	var mi := MeshInstance3D.new()
	mi.name = n
	var q := QuadMesh.new()
	q.size = Vector2(size_m, size_m)
	mi.mesh = q
	var m := _quad_mat(tex, add, billboard)
	m.albedo_color = col
	mi.material_override = m
	mi.cast_shadow = GeometryInstance3D.SHADOW_CASTING_SETTING_OFF
	mi.visible = false
	add_child(mi)
	return mi


func _particles(n: String, tex: Texture2D, sheet: bool, add: bool) -> CPUParticles3D:
	var p := CPUParticles3D.new()
	p.name = n
	p.one_shot = true
	p.explosiveness = 1.0
	p.emitting = false
	p.local_coords = false
	var q := QuadMesh.new()
	var m := StandardMaterial3D.new()
	m.shading_mode = BaseMaterial3D.SHADING_MODE_UNSHADED
	m.transparency = BaseMaterial3D.TRANSPARENCY_ALPHA
	if add:
		m.blend_mode = BaseMaterial3D.BLEND_MODE_ADD
	m.vertex_color_use_as_albedo = true
	m.albedo_texture = tex
	m.billboard_mode = BaseMaterial3D.BILLBOARD_PARTICLES
	if sheet:
		m.particles_anim_h_frames = 2
		m.particles_anim_v_frames = 2
		m.particles_anim_loop = false
		p.anim_offset_min = 0.0
		p.anim_offset_max = 1.0
	q.material = m
	p.mesh = q
	var g := Gradient.new()
	g.set_color(0, Color(1, 1, 1, 1))
	g.set_color(1, Color(1, 1, 1, 0))
	p.color_ramp = g
	add_child(p)
	return p


func _build() -> void:
	for c in get_children():
		c.queue_free()
	_main = null; _flash = null; _glow = null; _ring = null; _sparks = null; _debris = null; _smoke = null
	_light = null; _decal = null; _audio = null
	if effect_id == "":
		return
	var path := sidecar_path(effect_id)
	if path == "" or not FileAccess.file_exists(path):
		push_warning("SakugaEffect3D: no effect " + effect_id)
		return
	meta = JSON.parse_string(FileAccess.get_file_as_string(path))
	recipe = meta
	var fx: Dictionary = meta.get("fx", {})
	var W := float(meta.get("world_size_m", 2.0))
	var orient := str(meta.get("orientation", "billboard"))
	var ground := str(meta.get("anchor", "center")) == "ground"
	var tres := path.replace(".flipbook.json", ".tres")
	_main = Flipbook3D.new()
	_main.name = "Main"
	_main.play_on_ready = false
	_main.sprite_frames = load(tres)
	var cell: Array = meta.get("cell", [256, 256])
	_main.pixel_size = W / float(cell[1])
	_main.billboard = {"upright": Flipbook3D.Billboard.UPRIGHT, "ground": Flipbook3D.Billboard.NONE}.get(orient, Flipbook3D.Billboard.FACE_CAMERA)
	if orient == "ground":
		_main.rotation_degrees = Vector3(-90, 0, 0)
		_main.position.y = 0.03
	elif ground:
		var pivot: Array = meta.get("pivot", [0.5, 0.5])
		_main.position.y = ((1.0 - float(pivot[1])) - float(meta.get("bottom_inset", 0.0))) * W
	add_child(_main)
	_main.stop()
	if fx.get("flash") != null:
		_flash = _quad("Flash", _tex("textures/T_Flash.png"), float(fx["flash"]["size"]), _col(fx["flash"]["color"]) * Color(2, 2, 2, 1), true)
		_glow = _quad("Glow", _tex("textures/T_Glow.png"), float(fx["flash"]["size"]) * 1.7, _col(fx["light"]["color"]), true)
	if fx.get("ring") != null:
		var flat := str(fx["ring"].get("plane", "view")) == "ground"
		_ring = _quad("Ring", _tex("textures/T_Ring.png"), float(fx["ring"]["size"]), _col(fx["ring"]["color"]), true, not flat)
		if flat:
			_ring.rotation_degrees = Vector3(-90, 0, 0)
			_ring.position.y = 0.04 if ground else -0.9
	if fx.get("sparks") != null:
		var s: Dictionary = fx["sparks"]
		_sparks = _particles("Sparks", _tex("textures/T_Glow.png"), false, true)
		_sparks.amount = int(s["count"])
		_sparks.lifetime = float(s["life"])
		_sparks.initial_velocity_min = float(s["speed"]) * 0.45
		_sparks.initial_velocity_max = float(s["speed"])
		_sparks.gravity = Vector3(0, -9.8 * float(s["gravity"]), 0)
		_sparks.damping_min = float(s["drag"])
		_sparks.damping_max = float(s["drag"]) * 1.5
		_sparks.scale_amount_min = float(s["size"]) * 2.0
		_sparks.scale_amount_max = float(s["size"]) * 3.0
		_sparks.color = _col(s["color"])
		match str(s.get("direction", "radial")):
			"up", "rise":
				_sparks.direction = Vector3.UP
				_sparks.spread = 30.0
			_:
				_sparks.spread = 180.0
	if fx.get("debris") != null:
		var d: Dictionary = fx["debris"]
		_debris = _particles("Debris", _tex("textures/T_Debris_2x2.png"), true, false)
		_debris.amount = int(d["count"])
		_debris.lifetime = float(d["life"])
		_debris.direction = Vector3.UP
		_debris.spread = 50.0
		_debris.initial_velocity_min = float(d["speed"]) * 0.5
		_debris.initial_velocity_max = float(d["speed"])
		_debris.gravity = Vector3(0, -9.8 * float(d["gravity"]), 0)
		_debris.scale_amount_min = float(d["size"]) * 0.5
		_debris.scale_amount_max = float(d["size"])
		_debris.angle_min = 0.0
		_debris.angle_max = 360.0
		_debris.angular_velocity_min = -400.0
		_debris.angular_velocity_max = 400.0
		_debris.color = _col(d["color"])
	if fx.get("smoke") != null:
		var k: Dictionary = fx["smoke"]
		_smoke = _particles("Smoke", _tex("textures/T_Smoke_2x2.png"), true, false)
		_smoke.amount = int(k["count"])
		_smoke.lifetime = float(k["life"])
		_smoke.direction = Vector3.UP
		_smoke.spread = 60.0
		_smoke.initial_velocity_min = 0.4
		_smoke.initial_velocity_max = 1.2
		_smoke.gravity = Vector3(0, float(k["rise"]), 0)
		_smoke.damping_min = 1.0
		_smoke.damping_max = 2.0
		_smoke.scale_amount_min = float(k["size"]) * 0.6
		_smoke.scale_amount_max = float(k["size"])
		_smoke.color = Color.html(str(k["color"]))
	if fx.get("light") != null:
		_light = OmniLight3D.new()
		_light.name = "Light"
		_light.light_color = _col(fx["light"]["color"])
		_light.omni_range = float(fx["light"]["range"])
		_light.position.y = minf(W * 0.35, 1.2) if ground else 0.0
		_light.visible = false
		add_child(_light)
	if fx.get("decal") != null:
		var dt := _tex("decals/" + str(fx["decal"]["texture"]))
		if dt:
			_decal = Decal.new()
			_decal.name = "Decal"
			_decal.texture_albedo = dt
			var ds := float(fx["decal"]["size"])
			_decal.size = Vector3(ds, 2.0, ds)
			_decal.position.y = 0.0 if ground else -0.9
			_decal.rotation_degrees.y = float(hash(effect_id) % 360)
			_decal.visible = false
			add_child(_decal)
	_clips.clear()
	for kind in ["cast", "loop", "impact"]:
		var n := str(fx.get("sfx", {}).get(kind, ""))
		if n != "" and n != "<null>" and ResourceLoader.exists(SHARED + "sfx/" + n):
			_clips[n] = load(SHARED + "sfx/" + n)
	if _clips.size() > 0:
		_audio = AudioStreamPlayer3D.new()
		_audio.name = "Loop"
		add_child(_audio)
