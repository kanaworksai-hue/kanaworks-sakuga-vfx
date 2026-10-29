// KanaWorks SAKUGA VFX - unlit flipbook for Godot 4 (MIT).
// Premultiplied output: additive = 0 is normal alpha blending of the straight-alpha sheet, additive = 1 adds light.
// The quad's local xy is an offset from the pivot (the node origin), so billboarding turns it around the pivot.
shader_type spatial;
render_mode blend_premul_alpha, unshaded, cull_disabled, depth_draw_never, shadows_disabled, skip_vertex_transform, fog_disabled;

uniform sampler2D flipbook : source_color, filter_linear, repeat_disable;
uniform vec3 grid = vec3(8.0, 8.0, 64.0);   // cols, rows, frames
uniform float frame = 0.0;
uniform float additive : hint_range(0.0, 1.0) = 0.0;
uniform int billboard = 1;                  // 0 none, 1 face camera, 2 upright (keeps the node's up axis)
uniform vec4 tint : source_color = vec4(1.0);
uniform float alpha_boost : hint_range(0.5, 3.0) = 1.0;

void vertex() {
	if (billboard == 0) {
		VERTEX = (MODELVIEW_MATRIX * vec4(VERTEX, 1.0)).xyz;
	} else {
		vec3 ax = MODEL_MATRIX[0].xyz;
		vec3 ay = MODEL_MATRIX[1].xyz;
		float sx = length(ax);
		float sy = length(ay);
		vec3 right = INV_VIEW_MATRIX[0].xyz;
		vec3 up = INV_VIEW_MATRIX[1].xyz;
		if (billboard == 2) {
			up = ay / max(sy, 1e-5);
			vec3 r = right - up * dot(right, up);
			right = dot(r, r) > 1e-6 ? normalize(r) : ax / max(sx, 1e-5);
		}
		vec3 world = MODEL_MATRIX[3].xyz + right * (VERTEX.x * sx) + up * (VERTEX.y * sy);
		VERTEX = (VIEW_MATRIX * vec4(world, 1.0)).xyz;
	}
	NORMAL = vec3(0.0, 0.0, 1.0);
	vec2 g = max(grid.xy, vec2(1.0));
	float f = clamp(floor(frame), 0.0, max(grid.z, 1.0) - 1.0);
	UV = (clamp(UV, 0.0, 1.0) + vec2(mod(f, g.x), floor(f / g.x))) / g;   // first frame = top-left cell
}

void fragment() {
	vec4 c = texture(flipbook, UV) * tint * COLOR;
	float a = clamp(c.a * alpha_boost, 0.0, 1.0);
	ALBEDO = c.rgb * a;
	ALPHA = a * (1.0 - additive);
}
