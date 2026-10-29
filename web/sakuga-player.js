// KanaWorks SAKUGA VFX player - Canvas2D, no dependencies (MIT).
// Effects: VFX: KanaWorks_AI (@KanaWorks_AI) - KanaWorks_AI VFX Asset License.

// Load a <id>.flipbook.json sidecar and the sheet it names. Returns {meta, image}.
export async function load(sidecarUrl) {
  const url = new URL(sidecarUrl, location.href);
  const res = await fetch(url);
  if (!res.ok) throw new Error(`${url}: ${res.status}`);
  const meta = await res.json();
  const image = new Image();
  image.src = new URL(meta.source, url).href;
  await image.decode();
  return { meta, image };
}

// Pure frame clock: -1 before the start, `frames` once a one-shot has finished.
export function frameAt(t, fps, frames, loop) {
  if (frames < 1 || t < 0) return -1;
  const f = Math.floor(t * fps + 1e-4);
  return loop ? f % frames : Math.min(f, frames);
}

export class Flipbook {
  constructor({ meta, image }, opt = {}) {
    this.meta = meta;
    this.image = image;
    this.fps = opt.fps ?? meta.fps ?? 30;
    this.loop = opt.loop ?? !!meta.loop;
    this.additive = (opt.blend ?? meta.blend) === 'additive';
    this.scale = opt.scale ?? 1;
    this.time = 0;
    this.playing = opt.autoplay ?? true;
    this.onend = opt.onend ?? null;
  }
  get frames() { return this.meta.frames; }
  get frame() { return frameAt(this.time, this.fps, this.frames, this.loop); }
  get done() { return !this.loop && this.frame >= this.frames; }
  play() { this.time = 0; this.playing = true; return this; }
  stop() { this.playing = false; return this; }
  update(dt) {
    if (!this.playing) return;
    const was = this.done;
    this.time += dt;
    if (!was && this.done) { this.playing = false; this.onend?.(this); }
  }
  // Draw the current frame with its pivot (centre, or the root of a ground column) at x, y.
  draw(ctx, x, y, scale = this.scale) {
    const f = this.frame;
    if (f < 0 || f >= this.frames) return;
    const { grid: [cols], cell: [w, h], pivot: [px, py] = [0.5, 0.5] } = this.meta;
    const dw = w * scale, dh = h * scale;
    const op = ctx.globalCompositeOperation;
    if (this.additive) ctx.globalCompositeOperation = 'lighter';
    ctx.drawImage(this.image, (f % cols) * w, Math.floor(f / cols) * h, w, h, x - px * dw, y - py * dh, dw, dh);
    ctx.globalCompositeOperation = op;
  }
}

// TexturePacker-style JSON (hash) for PixiJS / Phaser from a sidecar, so you can skip the .atlas.json file.
export function spritesheetData(meta, image = meta.source) {
  const { id = 'fx', grid: [cols], cell: [w, h], frames: n, pivot: [px, py] = [0.5, 0.5] } = meta;
  const frames = {}, names = [];
  for (let i = 0; i < n; i++) {
    const name = `${id}_${String(i).padStart(3, '0')}`;
    names.push(name);
    frames[name] = {
      frame: { x: (i % cols) * w, y: Math.floor(i / cols) * h, w, h }, rotated: false, trimmed: false,
      spriteSourceSize: { x: 0, y: 0, w, h }, sourceSize: { w, h }, anchor: { x: px, y: py },
    };
  }
  return { frames, animations: { [id]: names },
    meta: { image, size: { w: cols * w, h: Math.ceil(n / cols) * h }, scale: '1' } };
}

// Play one flipbook in a canvas with its own animation loop. Returns {flipbook, stop()}.
export function mount(canvas, source, opt = {}) {
  const ctx = canvas.getContext('2d');
  const fb = source instanceof Flipbook ? source : new Flipbook(source, opt);
  const bg = opt.background ?? null;
  const replay = opt.replayAfter ?? 0.4;
  let last = performance.now(), idle = 0, raf = 0;
  const tick = (now) => {
    const dt = Math.min(0.1, (now - last) / 1000);
    last = now;
    fb.update(dt);
    if (fb.done && replay >= 0 && (idle += dt) > replay) { idle = 0; fb.play(); }
    ctx.setTransform(1, 0, 0, 1, 0, 0);
    if (bg) { ctx.fillStyle = bg; ctx.fillRect(0, 0, canvas.width, canvas.height); }
    else ctx.clearRect(0, 0, canvas.width, canvas.height);
    const [w, h] = fb.meta.cell, [px, py] = fb.meta.pivot ?? [0.5, 0.5];
    const s = opt.scale ?? Math.min(canvas.width / w, canvas.height / h) * 0.95;
    fb.draw(ctx, canvas.width / 2 + (px - 0.5) * w * s, canvas.height / 2 + (py - 0.5) * h * s, s);
    raf = requestAnimationFrame(tick);
  };
  raf = requestAnimationFrame(tick);
  return { flipbook: fb, stop: () => cancelAnimationFrame(raf) };
}
