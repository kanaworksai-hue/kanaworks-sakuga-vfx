extends SceneTree
## Headless check of a staged Godot package: every SpriteFrames loads with the frame count, loop flag and sheet
## size its metadata promises, and both demo scenes run and advance their animations.
## godot --headless --path <project> --script res://check_addon.gd

const INDEX := "res://addons/kanaworks_sakuga_vfx/flipbooks/index.json"
var fails := 0
var frames_run := 0
var demos: Array = []


func check(ok: bool, what: String) -> void:
	if not ok:
		fails += 1
		printerr("FAIL: " + what)


func _initialize() -> void:
	var data = JSON.parse_string(FileAccess.get_file_as_string(INDEX))
	check(data != null, "index.json readable")
	if data == null:
		quit(1)
		return
	for a in data["assets"]:
		var sf: SpriteFrames = load(a["frames"])
		check(sf != null, "load " + a["frames"])
		if sf == null:
			continue
		var meta: Dictionary = sf.get_meta("sakuga", {})
		check(not meta.is_empty(), a["id"] + " has sakuga metadata")
		check(sf.get_frame_count(&"default") == int(meta["frames"]), a["id"] + " frame count")
		check(sf.get_animation_loop(&"default") == bool(meta["loop"]), a["id"] + " loop flag")
		check(is_equal_approx(sf.get_animation_speed(&"default"), float(meta["fps"])), a["id"] + " fps")
		var tex = sf.get_frame_texture(&"default", 0)
		check(tex is AtlasTexture, a["id"] + " frames are AtlasTextures")
		if tex is AtlasTexture:
			var sheet: Texture2D = tex.atlas
			check(sheet != null and sheet.get_width() == int(meta["grid"][0]) * int(meta["cell"][0])
				and sheet.get_height() == int(meta["grid"][1]) * int(meta["cell"][1]), a["id"] + " sheet size")
	for path in ["res://sakuga_demo/demo_2d.tscn", "res://sakuga_demo/demo_3d.tscn"]:
		var scene: PackedScene = load(path)
		check(scene != null, "load " + path)
		if scene:
			var node := scene.instantiate()
			root.add_child(node)
			demos.append(node)


func _process(_delta: float) -> bool:
	frames_run += 1
	if frames_run < 20:
		return false
	var advanced2d := 0
	var advanced3d := 0
	for d in demos:
		for c in d.get_children():
			if c is AnimatedSprite2D and c.frame > 0:
				advanced2d += 1
			elif c is MeshInstance3D and c.get("current_frame") != null and int(c.get("current_frame")) > 0:
				advanced3d += 1
	check(advanced2d > 0, "2D demo animations advance")
	check(advanced3d > 0, "3D demo animations advance")
	print("SAKUGA_GODOT_CHECK " + ("OK" if fails == 0 else "FAILED %d" % fails))
	quit(1 if fails else 0)
	return true
