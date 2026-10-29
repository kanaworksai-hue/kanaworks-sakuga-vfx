// KanaWorks SAKUGA VFX layered effects - Canvas2D, no dependencies (MIT).
// Plays the flipbook plus the secondary layers of its recipe (the "fx" block in the .flipbook.json sidecar):
// contact flash and glow, shockwave ring, sparks, a ground glow standing in for the point light, the ground decal,
// and camera-shake events. Effects: VFX: KanaWorks_AI (@KanaWorks_AI) - KanaWorks_AI VFX Asset License.
import { Flipbook } from './sakuga-player.js';

const TEXTURES = ['T_Flash', 'T_Ring', 'T_Glow'];

// Shared textures (white, tinted here) and the decal/sound list. `root` is the folder holding shared.json
// (assets/_shared/ in the repository). Missing decals and sounds are simply skipped.
export async function loadShared(root) {
  const base = new URL(root.endsWith('/') ? root : root + '/', location.href);
  let manifest = { textures: {}, decals: {}, sfx: {} };
  try { const r = await fetch(new URL('shared.json', base)); if (r.ok) manifest = await r.json(); } catch {}
  const img = async (rel) => { const i = new Image(); i.src = new URL(rel, base).href; try { await i.decode(); return i; } catch { return null; } };
  const textures = {};
  await Promise.all(TEXTURES.map(async (n) => { textures[n] = await img(manifest.textures?.[n]?.file ?? `textures/${n}.png`); }));
  const decals = {};
  await Promise.all(Object.entries(manifest.decals ?? {}).map(async ([n, rel]) => { decals[n + '.png'] = await img(rel); }));
  return { base, manifest, textures, decals };
}

// Small deterministic random (same id -> same sparks), so stills and recordings repeat exactly.
export function rng(seedText = 'sakuga') {
  let h = 2166136261;
  for (const c of seedText) h = Math.imul(h ^ c.charCodeAt(0), 16777619);
  return () => { h += 0x6d2b79f5; let t = h; t = Math.imul(t ^ (t >>> 15), t | 1); t ^= t + Math.imul(t ^ (t >>> 7), t | 61); return ((t ^ (t >>> 14)) >>> 0) / 4294967296; };
}

// Piecewise-linear light curve: flat pairs [seconds after contact, multiplier, ...], clamped at both ends.
export function curveAt(curve, t) {
  if (!curve || curve.length < 2) return 1;
  if (t <= curve[0]) return curve[1];
  for (let i = 2; i + 1 < curve.length; i += 2) {
    const t0 = curve[i - 2], v0 = curve[i - 1], t1 = curve[i], v1 = curve[i + 1];
    if (t <= t1) return t1 > t0 ? v0 + (v1 - v0) * (t - t0) / (t1 - t0) : v1;
  }
  return curve[curve.length - 1];
}

const tintCache = new WeakMap();
function tinted(image, color) {
  if (!image) return null;
  let m = tintCache.get(image);
  if (!m) tintCache.set(image, (m = new Map()));
  if (m.has(color)) return m.get(color);
  const c = document.createElement('canvas');
  c.width = image.naturalWidth; c.height = image.naturalHeight;
  const g = c.getContext('2d');
  g.drawImage(image, 0, 0);
  g.globalCompositeOperation = 'source-in';
  g.fillStyle = color; g.fillRect(0, 0, c.width, c.height);
  m.set(color, c);
  return c;
}

const rgba = (hex, a) => {
  const n = parseInt((hex || '#ffffff').slice(1), 16);
  return `rgba(${(n >> 16) & 255},${(n >> 8) & 255},${n & 255},${a})`;
};

export class SakugaFx {
  // source: {meta, image} from load(); meta is the sidecar with its recipe. shared: from loadShared().
  constructor(source, shared = null, opt = {}) {
    this.meta = source.meta;
    this.fx = source.meta.fx ?? {};
    this.shared = shared;
    this.book = new Flipbook(source, { autoplay: false });
    this.size = opt.size ?? 1;                 // S 0.6, M 1, L 1.6
    this.speed = opt.speed ?? 1;
    this.color = opt.color ?? null;            // override every layer colour
    this.direction = opt.direction ?? 0;       // hit direction for sparks, radians (0 = right)
    this.flip = opt.flip ?? false;             // mirror the drawing (travel sheets fly toward +x)
    this.sound = opt.sound ?? false;
    this.onshake = opt.onshake ?? null;        // (amplitude m, duration s) at the contact frame
    this.oncontact = opt.oncontact ?? null;
    this.onend = opt.onend ?? null;
    this.time = 0; this.playing = false; this.contacted = false;
    this.sparks = [];
  }
  get fps() { return this.meta.fps ?? 30; }
  get contactTime() { return (this.meta.contact_frame ?? 0) / this.fps; }
  get loops() { return !!this.meta.loop || !!this.meta.loop_range; }
  frameAt(t) {
    const n = this.meta.frames, f = Math.floor(t * this.fps + 1e-4);
    if (t < 0) return -1;
    if (!this.loops) return f < n ? f : -1;
    const [a, b] = this.meta.loop_range ?? [0, n - 1];
    return f <= b ? f : a + (f - a) % (b - a + 1);
  }
  get duration() {
    const fx = this.fx, tc = this.contactTime;
    let d = this.meta.frames / this.fps;
    if (fx.sparks) d = Math.max(d, tc + fx.sparks.life);
    if (fx.decal) d = Math.max(d, tc + fx.decal.life);
    return d;
  }
  get done() { return !this.loops && this.time >= this.duration; }
  play() { this.time = 0; this.playing = true; this.contacted = false; this.sparks = []; this.#sfx('cast'); return this; }
  // Jump to time t deterministically (offline rendering).
  seek(t, step = 1 / 60) { this.play(); this.playing = true; const q = this.sound; this.sound = false; while (this.time < t - 1e-6) this.update(Math.min(step, t - this.time) / this.speed); this.sound = q; return this; }
  update(dt) {
    if (!this.playing) return;
    dt *= this.speed;
    const before = this.time;
    this.time += dt;
    const tc = this.contactTime;
    if (!this.contacted && this.time >= tc && before <= tc + 1e-4) this.#contact();
    for (const s of this.sparks) {
      s.vx *= Math.exp(-s.drag * dt); s.vy *= Math.exp(-s.drag * dt);
      s.vy += s.g * 9.81 * dt; s.x += s.vx * dt; s.y += s.vy * dt; s.age += dt;
    }
    this.sparks = this.sparks.filter((s) => s.age < s.life);
    if (this.done) { this.playing = false; this.onend?.(this); }
  }
  #col(c) { return this.color ?? c ?? '#ffffff'; }
  #sfx(kind) {
    const name = this.fx.sfx?.[kind];
    if (!this.sound || !name || !this.shared || !this.shared.manifest.sfx?.[name]) return;
    try { const a = new Audio(new URL(this.shared.manifest.sfx[name].file, this.shared.base).href); a.volume = 0.7; a.play().catch(() => {}); } catch {}
  }
  #contact() {
    this.contacted = true;
    const s = this.fx.sparks, r = rng(this.meta.id ?? 'fx');
    if (s) {
      const dir = s.direction, spread = this.direction ? 1.1 : Math.PI;
      for (let i = 0; i < s.count; i++) {
        let a = dir === 'up' || dir === 'rise' ? -Math.PI / 2 + (r() - 0.5) * 0.9 : this.direction + (r() * 2 - 1) * spread;
        const v = s.speed * (dir === 'rise' ? 0.3 : 0.45 + 0.55 * r());
        this.sparks.push({ x: 0, y: 0, vx: Math.cos(a) * v, vy: Math.sin(a) * v, g: s.gravity, drag: s.drag,
                           life: s.life * (0.55 + 0.45 * r()), age: 0, w: s.size * (0.6 + 0.4 * r()) });
      }
    }
    if (this.fx.shake) this.onshake?.(this.fx.shake.amplitude * Math.sqrt(this.size), this.fx.shake.duration);
    this.oncontact?.(this);
    this.#sfx('impact');
  }
  // Draw with the effect's origin at (x, y): the root for ground effects, the centre otherwise. ppm = pixels per metre.
  draw(ctx, x, y, ppm = 100) {
    const m = this.meta, fx = this.fx, tc = this.contactTime, age = this.time - tc, k = ppm * this.size;
    const tex = this.shared?.textures ?? {};
    const op = ctx.globalCompositeOperation;
    // ground glow (stands in for the point light) and decal, under everything
    if (fx.light && age >= (fx.light.curve?.[0] ?? 0)) {
      const li = Math.min(1, curveAt(fx.light.curve, age) * fx.light.intensity / 4);
      if (li > 0.01) {
        const R = fx.light.range * k * 0.5, gy = m.anchor === 'ground' ? y : y + 0.9 * k;
        const g = ctx.createRadialGradient(x, gy, 0, x, gy, R);
        g.addColorStop(0, rgba(this.#col(fx.light.color), 0.35 * li)); g.addColorStop(1, rgba(this.#col(fx.light.color), 0));
        ctx.globalCompositeOperation = 'lighter'; ctx.fillStyle = g;
        ctx.beginPath(); ctx.ellipse(x, gy, R, R * 0.3, 0, 0, Math.PI * 2); ctx.fill();
      }
    }
    const decal = fx.decal && this.shared?.decals?.[fx.decal.texture];
    if (decal && age >= 0) {
      const fade = Math.max(0, Math.min(1, (fx.decal.life - age) / (fx.decal.life * 0.4)));
      const D = fx.decal.size * k * Math.min(1, 0.55 + age / 0.12 * 0.45), gy = m.anchor === 'ground' ? y : y + 0.9 * k;
      ctx.globalCompositeOperation = 'source-over'; ctx.globalAlpha = fade;
      ctx.drawImage(decal, x - D / 2, gy - D * 0.15, D, D * 0.3);
      ctx.globalAlpha = 1;
    }
    // the drawing
    const f = this.frameAt(this.time);
    if (f >= 0) {
      this.book.time = f / this.fps + 1e-4; this.book.loop = false;
      const H = (m.world_size_m ?? 2) * k, scale = H / m.cell[1];
      const lift = m.anchor === 'ground' && m.orientation !== 'ground' ? ((1 - (m.pivot?.[1] ?? 0.5)) - (m.bottom_inset ?? 0)) * H : 0;
      ctx.save();
      ctx.translate(x, y - lift);
      if (m.orientation === 'ground') ctx.scale(1, 0.32);               // lying on the floor, seen from the side
      if (this.flip) ctx.scale(-1, 1);
      this.book.draw(ctx, 0, 0, scale);
      ctx.restore();
    }
    // ring, sparks, flash: additive
    ctx.globalCompositeOperation = 'lighter';
    if (fx.ring && age >= 0 && age < fx.ring.duration * 0.5) {
      const p = age / (fx.ring.duration * 0.5), R = fx.ring.size * k * (0.15 + 0.85 * (1 - (1 - p) ** 3));
      const img = tinted(tex.T_Ring, this.#col(fx.ring.color));
      ctx.globalAlpha = 0.5 * (1 - p);
      if (img) ctx.drawImage(img, x - R / 2, y - (fx.ring.plane === 'ground' ? R * 0.15 : R / 2), R, fx.ring.plane === 'ground' ? R * 0.3 : R);
      ctx.globalAlpha = 1;
    }
    if (this.sparks.length) {
      ctx.lineCap = 'round';
      for (const s of this.sparks) {
        const a = 1 - s.age / s.life;
        ctx.strokeStyle = rgba(this.#col(fx.sparks.color), Math.min(1, a * 1.5));
        ctx.lineWidth = Math.max(1, s.w * k * 0.5);
        ctx.beginPath(); ctx.moveTo(x + s.x * k, y + s.y * k); ctx.lineTo(x + (s.x - s.vx * 0.05) * k, y + (s.y - s.vy * 0.05) * k); ctx.stroke();
      }
    }
    if (fx.flash && age >= 0 && age < fx.flash.frames / 30 + 0.04) {
      const p = age / (fx.flash.frames / 30 + 0.04), F = fx.flash.size * k * (1 - 0.6 * p);
      const glow = tinted(tex.T_Glow, this.#col(fx.light?.color)), star = tinted(tex.T_Flash, this.#col(fx.flash.color));
      ctx.globalAlpha = 0.8 * (1 - p);
      if (glow) ctx.drawImage(glow, x - F, y - F, F * 2, F * 2);
      ctx.globalAlpha = 1 - p * 0.5;
      if (star) ctx.drawImage(star, x - F / 2, y - F / 2, F, F);
      ctx.globalAlpha = 1;
    }
    ctx.globalCompositeOperation = op;
  }
}

// Camera shake for a canvas: add() on the effect's onshake, then ctx.translate(...shaker.offset(dt)) each frame.
export class Shaker {
  constructor(pxPerMetre = 100) { this.ppm = pxPerMetre; this.amp = 0; this.dur = 0; this.t = 0; }
  add(amplitude, duration) {
    const left = this.dur > 0 ? this.amp * (1 - Math.min(1, this.t / this.dur)) : 0;
    if (amplitude >= left) { this.amp = amplitude; this.dur = duration; this.t = 0; }
  }
  offset(dt) {
    if (this.t >= this.dur) return [0, 0];
    this.t += dt;
    const k = Math.max(0, 1 - this.t / this.dur) ** 2, a = this.amp * this.ppm * k;
    return [Math.sin(this.t * 90) * a, Math.cos(this.t * 77) * a];
  }
}
