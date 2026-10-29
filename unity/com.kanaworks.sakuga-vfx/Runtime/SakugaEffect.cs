using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace KanaWorks.SakugaVFX
{
    public enum SakugaSize { S, M, L }

    /// <summary>Something that moves an effect along while it plays (SakugaProjectile). Called once per step with
    /// the step length in effect seconds, before the layers advance; `live` is false while simulating offline.</summary>
    public interface ISakugaStepper
    {
        void ResetStepper();
        void Step(float dt, bool live);
        bool Busy { get; }
    }

    /// <summary>
    /// A ready-to-use layered effect: the hand-drawn-style flipbook plus the secondary layers from its recipe (contact
    /// flash and glow, shockwave ring, sparks, debris, cel smoke, ground decal, a point light with an intensity
    /// curve, sound, camera shake and hit stop requests). Built from the recipe by the importer as &lt;id&gt;_fx.prefab.
    ///
    /// Live: Play() (or SakugaFX.Play) runs it on the game clock. Offline: Simulate(t) rebuilds the exact state at t
    /// seconds by stepping from zero with a fixed step and a fixed random seed, so frames can be rendered in any order
    /// (camera.Render) and come out the same every time.
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("KanaWorks/SAKUGA Effect")]
    public class SakugaEffect : MonoBehaviour
    {
        public enum EndAction { Nothing, Disable, Destroy }

        [Header("Effect (from the recipe)")]
        public string id;
        public string element;
        public int fps = 30;
        public int frames = 64;
        public bool loop;
        [Tooltip("Cell size in pixels (width, height), for the quad's aspect.")]
        public Vector2 cell = new Vector2(256, 256);
        [Tooltip("Pivot in the cell, 0-1 from the top-left (from the sidecar).")]
        public Vector2 pivot = new Vector2(0.5f, 0.5f);
        public float bottomInset;
        public SakugaRecipe recipe = new SakugaRecipe();

        [Header("Layers (filled by the importer)")]
        public SakugaFlipbook main;
        public Material flashMaterial, glowMaterial, ringMaterial, sparkMaterial, debrisMaterial, smokeMaterial, trailMaterial;
        [Tooltip("Flat-quad ground decal (used when the URP renderer has no Decal feature).")]
        public Material decalMaterial;
        [Tooltip("URP Decal Projector material (used when the renderer has the Decal feature).")]
        public Material decalProjectorMaterial;
        public AudioClip castClip, loopClip, impactClip;

        [Header("Settings")]
        public SakugaSize size = SakugaSize.M;
        [Min(0.01f)] public float scale = 1f;
        [Tooltip("Tint the secondary layers and the light with Color instead of the recipe's colours.")]
        public bool overrideColor;
        public Color color = Color.white;
        [Range(0, 1), Tooltip("With Override Color: how far the drawing itself is re-ramped to Color.")]
        public float recolorMain = 0.85f;
        [Min(0f)] public float speed = 1f;
        public bool useUnscaledTime;
        public bool playOnEnable;
        [Min(0f), Tooltip("Looping effects stop by themselves after this many seconds (0 = until Stop()).")]
        public float lifetime;
        [Tooltip("Start this many seconds into the effect (for example skip a charge's wind-up).")]
        [Min(0f)] public float startAt;
        public bool lights = true, sound = true, shake = true, hitStop = true, decal = true;
        [Range(0, 1)] public float volume = 0.8f;
        [Tooltip("Hits: direction the blow travels (sparks fly that way). Zero = all around.")]
        public Vector3 hitDirection;
        [Tooltip("Fixed random seed for particles (0 = a new seed each live Play; Simulate always uses a fixed one).")]
        public uint seed;
        public EndAction onEnd = EndAction.Nothing;

        /// <summary>Raised once when everything (flipbook, particles, decal, light) has finished.</summary>
        public event Action<SakugaEffect> Finished;
        /// <summary>Raised at the contact frame: (effect, amplitude m, duration s). SakugaFX also forwards it to the camera.</summary>
        public event Action<SakugaEffect, float, float> ShakeRequested;
        /// <summary>Raised at the contact frame (the moment the hit lands).</summary>
        public event Action<SakugaEffect> Contact;

        public static readonly float[] SizePresets = { 0.6f, 1f, 1.6f };

        // runtime state
        [NonSerialized] public bool pooled;
        [NonSerialized] public string poolKey;
        float time, stopTime = -1f;
        bool playing, manual, contacted, firstManualStep, built;
        ISakugaStepper stepper;
        readonly List<ParticleSystem> systems = new List<ParticleSystem>();
        ParticleSystem flashPs, glowPs, ringPs, sparkPs, debrisPs, smokePs, trailPs;
        Light lightComp;
        AudioSource oneShot, loopSource;
        Transform groundPlane, decalTf;
        Renderer decalRenderer;
        Component decalProjector;
        Vector3 groundPoint;
        bool hasGround;
        float decalSpin;
        MaterialPropertyBlock decalBlock;
        static readonly int InstanceColorId = Shader.PropertyToID("_InstanceColor");

        public bool IsPlaying => playing;
        public bool IsStopping => stopTime >= 0f;
        public float Time => time;
        public float ContactTime => recipe.contact_frame / (float)Mathf.Max(fps, 1);
        public float MainDuration => frames / (float)Mathf.Max(fps, 1);
        public float SizeScale => SizePresets[(int)size] * scale;
        public bool LoopsMain => loop || recipe.HasLoopRange;

        /// <summary>Roughly how long a one-shot runs before it is finished (flipbook, layers, decal, light).</summary>
        public float EstimatedDuration
        {
            get
            {
                var fx = recipe.fx;
                float tc = ContactTime, d = MainDuration;
                if (fx.sparks.On) d = Mathf.Max(d, tc + fx.sparks.life);
                if (fx.debris.On) d = Mathf.Max(d, tc + fx.debris.life);
                if (fx.smoke.On) d = Mathf.Max(d, tc + fx.smoke.delay + fx.smoke.life);
                if (fx.decal.On && decal) d = Mathf.Max(d, tc + fx.decal.life);
                if (fx.light.On) d = Mathf.Max(d, tc + fx.light.End);
                return d;
            }
        }

        // ------------------------------------------------------------------ lifecycle
        void Awake() => Build();

        void OnEnable()
        {
            SakugaFX.EnsureGlobals();
            if (Application.isPlaying && playOnEnable && !manual) Play();
        }

        void OnDisable()
        {
            playing = false;
            if (lightComp) lightComp.enabled = false;
        }

        /// <summary>Play from the start (or from startAt) on the game clock.</summary>
        public void Play()
        {
            Build();
            manual = false;
            ResetState();
            uint s = seed != 0 ? seed : (uint)UnityEngine.Random.Range(1, int.MaxValue);
            foreach (var ps in systems)
            {
                ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                ps.randomSeed = s++;
                var m = ps.main;
                m.simulationSpeed = speed;
                m.useUnscaledTime = useUnscaledTime;
                ps.Play(false);
            }
            playing = true;
            if (sound && SakugaFX.Sound && castClip && oneShot) oneShot.PlayOneShot(castClip, volume * SakugaFX.Volume);
            if (sound && SakugaFX.Sound && loopClip && loopSource)
            {
                loopSource.clip = loopClip;
                loopSource.volume = volume * SakugaFX.Volume;
                loopSource.Play();
            }
            if (startAt > 0f) Advance(startAt, true, skipParticles: false);
            Apply();
        }

        /// <summary>Stop a looping effect (or cut a one-shot short): the flipbook fades out, particles already in the
        /// air finish, then Finished is raised.</summary>
        public void Stop()
        {
            if (stopTime < 0f) stopTime = time;
            foreach (var ps in systems) ps.Stop(false, ParticleSystemStopBehavior.StopEmitting);
        }

        /// <summary>Stop at once and hide everything (no Finished event).</summary>
        public void Clear()
        {
            playing = false;
            foreach (var ps in systems) ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            if (main) main.ShowFrame(-1);
            if (lightComp) lightComp.enabled = false;
            if (decalTf) decalTf.gameObject.SetActive(false);
            if (loopSource) loopSource.Stop();
        }

        /// <summary>Deterministic offline state t seconds after Play (speed applies): steps from zero with a fixed step.</summary>
        public void Simulate(float t, float step = 1f / 60f)
        {
            BeginManual();
            float done = 0f;
            step = Mathf.Max(step, 1e-3f);
            while (done < t - 1e-5f)
            {
                float dt = Mathf.Min(step, t - done);
                StepManual(dt);
                done += dt;
            }
            Apply();
        }

        /// <summary>Start manual (offline) mode at time 0. Then call StepManual(dt) as often as you like.</summary>
        public void BeginManual()
        {
            Build();
            SakugaFX.EnsureGlobals();
            manual = true;
            playing = true;
            ResetState();
            uint s = seed != 0 ? seed : StableSeed(id);
            foreach (var ps in systems)
            {
                ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                ps.randomSeed = s++;
                var m = ps.main;
                m.simulationSpeed = 1f;
            }
            firstManualStep = true;
            if (startAt > 0f) StepManual(startAt / Mathf.Max(speed, 1e-3f));
            Apply();
        }

        /// <summary>Advance a manual effect by dt effect-seconds.</summary>
        public void StepManual(float dt)
        {
            if (!manual) BeginManual();
            dt *= speed;
            Advance(dt, false, skipParticles: true);
            foreach (var ps in systems) ps.Simulate(dt, false, firstManualStep, false);
            firstManualStep = false;
            Apply();
        }

        void Update()
        {
            if (!playing || manual) return;
            float dt = (useUnscaledTime ? UnityEngine.Time.unscaledDeltaTime : UnityEngine.Time.deltaTime) * speed;
            foreach (var ps in systems) { var m = ps.main; m.simulationSpeed = speed; }
            Advance(dt, true, skipParticles: false);
            Apply();
            if (IsDone()) Finish();
        }

        // ------------------------------------------------------------------ timeline
        void ResetState()
        {
            time = 0f;
            stopTime = -1f;
            contacted = false;
            transform.localScale = Vector3.one * SizeScale;
            if (stepper == null || (stepper is Object o && !o)) stepper = TryGetComponent(out ISakugaStepper st) ? st : null;
            stepper?.ResetStepper();
            FindGround();
            if (decalTf) decalTf.gameObject.SetActive(false);
            if (main)
            {
                main.externallyDriven = true;
                main.ShowFrame(-1);
            }
            ApplyColors();
        }

        void Advance(float dt, bool live, bool skipParticles)
        {
            stepper?.Step(dt, live);
            float before = time;
            time += dt;
            float tc = ContactTime;
            if (!contacted && time >= tc && before <= tc + 1e-4f)
            {
                contacted = true;
                OnContact(live);
            }
            if (lifetime > 0f && LoopsMain && stopTime < 0f && time >= lifetime) Stop();
            if (live && loopSource && loopSource.isPlaying && stopTime >= 0f)
            {
                loopSource.volume = volume * SakugaFX.Volume * Mathf.Clamp01(1f - (time - stopTime) / 0.25f);
                if (time - stopTime > 0.25f) loopSource.Stop();
            }
        }

        void OnContact(bool live)
        {
            var fx = recipe.fx;
            if (decal && fx.decal.On && hasGround && decalTf) decalTf.gameObject.SetActive(true);
            Contact?.Invoke(this);
            if (!live) return;
            if (sound && SakugaFX.Sound && impactClip && oneShot) oneShot.PlayOneShot(impactClip, volume * SakugaFX.Volume);
            if (shake && fx.shake.On)
            {
                float amp = fx.shake.amplitude * Mathf.Sqrt(SizeScale);
                ShakeRequested?.Invoke(this, amp, fx.shake.duration);
                SakugaFX.RaiseShake(amp, fx.shake.duration, transform.position);
            }
            if (hitStop && fx.hit_stop_s > 0f) SakugaFX.RaiseHitStop(fx.hit_stop_s);
        }

        /// <summary>Frame of the drawing at effect time t (-1 = nothing shown).</summary>
        public int FrameAt(float t)
        {
            if (t < 0f) return -1;
            int f = Mathf.FloorToInt(t * fps + 1e-4f);
            if (LoopsMain)
            {
                int a = 0, b = frames - 1;
                if (recipe.HasLoopRange) { a = Mathf.Clamp(recipe.loop_range[0], 0, frames - 1); b = Mathf.Clamp(recipe.loop_range[1], a, frames - 1); }
                if (f <= b) return f;
                return a + (f - a) % (b - a + 1);
            }
            return f < frames ? f : -1;
        }

        float MainAlpha()
        {
            if (stopTime < 0f) return 1f;
            return Mathf.Clamp01(1f - (time - stopTime) / 0.25f);
        }

        void Apply()
        {
            var fx = recipe.fx;
            if (main)
            {
                bool hidden = stepper != null && !stepper.Busy && recipe.IsProjectile;
                float a = MainAlpha();
                int f = hidden || a <= 0f ? -1 : FrameAt(time);
                main.ShowFrame(f);
                if (f >= 0) main.SetLook(new Color(1, 1, 1, a), overrideColor ? recolorMain : 0f, color);
            }
            if (lightComp)
            {
                float k = 0f;
                if (lights && SakugaFX.Lights && fx.light.On)
                {
                    float rel = time - ContactTime;
                    if (recipe.IsProjectile) k = stepper != null && !stepper.Busy ? Mathf.Clamp01(1f - (time - stopTime) / 0.15f) : 1f;
                    else if (LoopsMain || rel >= fx.light.curve?[0]) k = fx.light.Evaluate(rel);
                    if (LoopsMain || recipe.IsProjectile) k *= MainAlpha();
                    if (!recipe.IsProjectile && !LoopsMain && time < ContactTime + (fx.light.curve != null && fx.light.curve.Length > 0 ? fx.light.curve[0] : 0f)) k = 0f;
                }
                float intensity = fx.light.intensity * k;
                lightComp.enabled = intensity > 0.01f;
                lightComp.intensity = intensity;
                lightComp.range = fx.light.range * SizeScale;
            }
            if (decalTf && decalTf.gameObject.activeSelf)
            {
                float life = Mathf.Max(fx.decal.life, 0.1f), age = time - ContactTime;
                float grow = Mathf.SmoothStep(0.55f, 1f, Mathf.Clamp01(age / 0.12f));
                float fade = LoopsMain && stopTime < 0f ? 1f : Mathf.Clamp01((life - (stopTime >= 0f && LoopsMain ? time - stopTime : age)) / (life * 0.4f));
                float s = fx.decal.size * SizeScale * grow;
                if (decalProjector) SetProjector(s, fade);
                else
                {
                    decalTf.localScale = new Vector3(s, s, s) / SizeScale;
                    decalBlock ??= new MaterialPropertyBlock();
                    decalRenderer.GetPropertyBlock(decalBlock);
                    decalBlock.SetColor(InstanceColorId, new Color(1, 1, 1, fade) * SakugaColor.Parse(fx.decal.tint));
                    decalRenderer.SetPropertyBlock(decalBlock);
                }
                if (fade <= 0f) decalTf.gameObject.SetActive(false);
            }
        }

        bool IsDone()
        {
            if (stepper != null && stepper.Busy) return false;
            if (LoopsMain && stopTime < 0f) return false;
            if (!LoopsMain && time < MainDuration && stopTime < 0f) return false;
            if (stopTime >= 0f && time - stopTime < 0.25f) return false;
            foreach (var ps in systems) if (ps.IsAlive(false)) return false;
            if (decalTf && decalTf.gameObject.activeSelf) return false;
            if (lightComp && lightComp.enabled) return false;
            return true;
        }

        void Finish()
        {
            playing = false;
            if (loopSource) loopSource.Stop();
            if (main) main.ShowFrame(-1);
            Finished?.Invoke(this);
            if (pooled) { SakugaFX.Release(this); return; }
            switch (onEnd)
            {
                case EndAction.Destroy: Destroy(gameObject); break;
                case EndAction.Disable: gameObject.SetActive(false); break;
            }
        }

        static uint StableSeed(string s)
        {
            unchecked
            {
                uint h = 2166136261;
                if (!string.IsNullOrEmpty(s)) foreach (char c in s) h = (h ^ c) * 16777619;
                return h == 0 ? 1 : h;
            }
        }

        // ------------------------------------------------------------------ ground
        void FindGround()
        {
            var p = transform.position;
            hasGround = false;
            if (recipe.anchor == "ground") { groundPoint = p; hasGround = true; }
            else if (Physics.Raycast(p + Vector3.up * 0.05f, Vector3.down, out var hit, SakugaFX.GroundSearch, SakugaFX.GroundMask,
                                     QueryTriggerInteraction.Ignore))
            {
                groundPoint = hit.point; hasGround = true;
            }
            else if (p.y - SakugaFX.GroundHeight >= -0.01f && p.y - SakugaFX.GroundHeight <= SakugaFX.GroundSearch)
            {
                groundPoint = new Vector3(p.x, SakugaFX.GroundHeight, p.z); hasGround = true;
            }
            if (groundPlane) groundPlane.position = hasGround ? groundPoint : p + Vector3.down * 50f;
            if (decalTf && hasGround)
            {
                decalTf.position = groundPoint + Vector3.up * 0.02f;
                decalSpin = (StableSeed(id + (seed + 1)) % 360);
                decalTf.rotation = decalProjector ? Quaternion.Euler(90f, decalSpin, 0f) : Quaternion.Euler(90f, decalSpin, 0f);
            }
            var fx = recipe.fx;
            if (ringPs && fx.ring.plane == "ground") ringPs.transform.position = (hasGround ? groundPoint : p) + Vector3.up * 0.04f;
        }

        // ------------------------------------------------------------------ colours
        Color Col(string hex) => overrideColor ? color : SakugaColor.Parse(hex);
        Color ColLight(string hex, float toWhite) => overrideColor ? Color.Lerp(color, Color.white, toWhite) : SakugaColor.Parse(hex);

        void ApplyColors()
        {
            var fx = recipe.fx;
            if (flashPs) { var m = flashPs.main; m.startColor = ColLight(fx.flash.color, 0.65f); }
            if (glowPs) { var m = glowPs.main; m.startColor = Col(fx.light.color) * new Color(1, 1, 1, 0.8f); }
            if (ringPs) { var m = ringPs.main; m.startColor = ColLight(fx.ring.color, 0.35f); }
            if (sparkPs) { var m = sparkPs.main; m.startColor = ColLight(fx.sparks.color, 0.2f); }
            if (debrisPs) { var m = debrisPs.main; m.startColor = overrideColor ? Color.Lerp(color, Color.white, 0.15f) : SakugaColor.Parse(fx.debris.color); }
            if (smokePs) { var m = smokePs.main; m.startColor = SakugaColor.Parse(fx.smoke.color) * new Color(1, 1, 1, 0.85f); }
            if (trailPs) { var m = trailPs.main; m.startColor = ColLight(fx.trail.color, 0.15f); }
            if (lightComp) lightComp.color = Col(fx.light.color);
            if (sparkPs && hitDirection.sqrMagnitude > 1e-4f && fx.sparks.direction == "radial")
            {
                sparkPs.transform.rotation = Quaternion.LookRotation(hitDirection.normalized);
                var sh = sparkPs.shape; sh.shapeType = ParticleSystemShapeType.Cone; sh.angle = 55f;
            }
        }

        // ------------------------------------------------------------------ build
        /// <summary>Create the layer objects from the recipe (idempotent; the importer bakes them into the prefab).</summary>
        public void Build()
        {
            if (built && systems.Count > 0 || (built && Application.isPlaying)) return;
            built = true;
            systems.Clear();
            var fx = recipe.fx;
            float W = Mathf.Max(recipe.world_size_m, 0.1f);
            if (!main) main = GetComponentInChildren<SakugaFlipbook>(true);
            if (main) main.externallyDriven = true;

            var gp = Child("GroundPlane");
            groundPlane = gp;

            if (fx.flash.On && flashMaterial)
            {
                flashPs = Ps("Flash", flashMaterial, 2, ParticleSystemRenderMode.Billboard);
                float life = Mathf.Max(fx.flash.frames, 2) / 30f + 0.04f;
                Burst(flashPs, 1, 1);
                var m = flashPs.main; m.startLifetime = life; m.startSize = fx.flash.size; m.startRotation = new ParticleSystem.MinMaxCurve(0, Mathf.PI * 2);
                SizeCurve(flashPs, Curve(0, 0.75f, 0.3f, 1f, 1, 0.35f));
                AlphaFade(flashPs, 0.5f);
                glowPs = Ps("Glow", glowMaterial ? glowMaterial : flashMaterial, 2, ParticleSystemRenderMode.Billboard);
                Burst(glowPs, 1, 1);
                var g = glowPs.main; g.startLifetime = 0.28f; g.startSize = fx.flash.size * 1.7f;
                SizeCurve(glowPs, Curve(0, 0.6f, 0.25f, 1f, 1, 1.1f));
                AlphaFade(glowPs, 0f, 0.7f);
            }
            if (fx.ring.On && ringMaterial)
            {
                bool flat = fx.ring.plane == "ground";
                ringPs = Ps("Ring", ringMaterial, 2, flat ? ParticleSystemRenderMode.HorizontalBillboard : ParticleSystemRenderMode.Billboard);
                Burst(ringPs, 1, 1);
                var m = ringPs.main; m.startLifetime = flat ? fx.ring.duration : Mathf.Min(fx.ring.duration * 0.5f, 0.2f); m.startSize = fx.ring.size;
                var sc = new AnimationCurve(new Keyframe(0, 0.15f, 0, 4f), new Keyframe(1, 1f, 0, 0));   // fast out, slow in
                SizeCurve(ringPs, sc);
                AlphaFade(ringPs, 0f, flat ? 0.8f : 0.45f);
            }
            if (fx.sparks.On && sparkMaterial)
            {
                var s = fx.sparks;
                sparkPs = Ps("Sparks", sparkMaterial, Mathf.Max(64, s.count * 2), ParticleSystemRenderMode.Stretch);
                Burst(sparkPs, s.count, s.count);
                var m = sparkPs.main;
                m.startLifetime = new ParticleSystem.MinMaxCurve(s.life * 0.55f, s.life);
                m.startSpeed = new ParticleSystem.MinMaxCurve(s.speed * 0.45f, s.speed);
                m.startSize = new ParticleSystem.MinMaxCurve(s.size * 0.6f, s.size);
                m.gravityModifier = s.gravity;
                var r = sparkPs.GetComponent<ParticleSystemRenderer>();
                r.velocityScale = 0.05f; r.lengthScale = 2.5f;
                Drag(sparkPs, s.drag);
                var sh = sparkPs.shape; sh.enabled = true; sh.radius = W * 0.08f;
                switch (s.direction)
                {
                    case "up": sh.shapeType = ParticleSystemShapeType.Cone; sh.angle = 28f; sh.radius = W * 0.12f; sparkPs.transform.localRotation = Quaternion.Euler(-90, 0, 0); break;
                    case "forward": sh.shapeType = ParticleSystemShapeType.Cone; sh.angle = 35f; break;
                    case "rise": sh.shapeType = ParticleSystemShapeType.Cone; sh.angle = 12f; sh.radius = W * 0.3f; sparkPs.transform.localRotation = Quaternion.Euler(-90, 0, 0);
                        m.startSpeed = new ParticleSystem.MinMaxCurve(s.speed * 0.15f, s.speed * 0.35f); break;
                    case "swirl":
                        sh.shapeType = ParticleSystemShapeType.Circle; sh.radius = W * 0.3f; sparkPs.transform.localRotation = Quaternion.Euler(-90, 0, 0);
                        m.startSpeed = new ParticleSystem.MinMaxCurve(s.speed * 0.1f, s.speed * 0.3f);
                        var vel = sparkPs.velocityOverLifetime; vel.enabled = true; vel.space = ParticleSystemSimulationSpace.Local;
                        vel.orbitalZ = new ParticleSystem.MinMaxCurve(4f, 7f); vel.z = new ParticleSystem.MinMaxCurve(1f, 2.5f);
                        vel.x = 0; vel.y = 0; vel.orbitalX = 0; vel.orbitalY = 0; vel.radial = 0;
                        break;
                    default: sh.shapeType = ParticleSystemShapeType.Sphere; break;
                }
                AlphaFade(sparkPs, 0.55f);
                Collide(sparkPs, 0.15f, 0.6f);
            }
            if (fx.debris.On && debrisMaterial)
            {
                var d = fx.debris;
                debrisPs = Ps("Debris", debrisMaterial, Mathf.Max(32, d.count * 2), ParticleSystemRenderMode.Billboard);
                Burst(debrisPs, d.count, d.count);
                var m = debrisPs.main;
                m.startLifetime = new ParticleSystem.MinMaxCurve(d.life * 0.7f, d.life);
                m.startSpeed = new ParticleSystem.MinMaxCurve(d.speed * 0.5f, d.speed);
                m.startSize = new ParticleSystem.MinMaxCurve(d.size * 0.5f, d.size);
                m.gravityModifier = d.gravity;
                m.startRotation = new ParticleSystem.MinMaxCurve(0, Mathf.PI * 2);
                var sh = debrisPs.shape; sh.enabled = true; sh.shapeType = ParticleSystemShapeType.Cone; sh.angle = 50f; sh.radius = W * 0.12f;
                debrisPs.transform.localRotation = Quaternion.Euler(-90, 0, 0);
                var rot = debrisPs.rotationOverLifetime; rot.enabled = true; rot.z = new ParticleSystem.MinMaxCurve(-8f, 8f);
                Sheet(debrisPs);
                AlphaFade(debrisPs, 0.8f);
                Collide(debrisPs, 0.3f, 0f);
            }
            if (fx.smoke.On && smokeMaterial)
            {
                var k = fx.smoke;
                smokePs = Ps("Smoke", smokeMaterial, Mathf.Max(16, k.count * 2), ParticleSystemRenderMode.Billboard);
                Burst(smokePs, k.count, k.count, k.delay);
                var m = smokePs.main;
                m.startLifetime = new ParticleSystem.MinMaxCurve(k.life * 0.7f, k.life);
                m.startSpeed = new ParticleSystem.MinMaxCurve(0.4f, 1.2f);
                m.startSize = new ParticleSystem.MinMaxCurve(k.size * 0.6f, k.size);
                m.startRotation = new ParticleSystem.MinMaxCurve(0, Mathf.PI * 2);
                var sh = smokePs.shape; sh.enabled = true; sh.shapeType = ParticleSystemShapeType.Hemisphere; sh.radius = W * 0.18f;
                smokePs.transform.localRotation = Quaternion.Euler(-90, 0, 0);
                var vel = smokePs.velocityOverLifetime; vel.enabled = true; vel.space = ParticleSystemSimulationSpace.World;
                vel.x = 0; vel.z = 0; vel.y = new ParticleSystem.MinMaxCurve(k.rise * 0.6f, k.rise);
                var rot = smokePs.rotationOverLifetime; rot.enabled = true; rot.z = new ParticleSystem.MinMaxCurve(-0.8f, 0.8f);
                Drag(smokePs, 2.5f);
                SizeCurve(smokePs, Curve(0, 0.55f, 0.3f, 1f, 1, 1.25f));
                Sheet(smokePs);
                AlphaFade(smokePs, 0.45f);
            }
            if (fx.trail.On && trailMaterial)
            {
                var tr = fx.trail;
                trailPs = Ps("Trail", trailMaterial, 256, ParticleSystemRenderMode.Billboard);
                var em = trailPs.emission; em.rateOverDistance = tr.rate; em.rateOverTime = 0;
                var m = trailPs.main; m.loop = true; m.duration = 1f;
                m.startLifetime = new ParticleSystem.MinMaxCurve(tr.life * 0.7f, tr.life);
                m.startSize = new ParticleSystem.MinMaxCurve(tr.size * 0.7f, tr.size);
                m.startRotation = new ParticleSystem.MinMaxCurve(0, Mathf.PI * 2);
                SizeCurve(trailPs, Curve(0, 1f, 1, 0.1f));
                AlphaFade(trailPs, 0.1f);
            }
            if (fx.light.On)
            {
                var lt = Child("Light");
                lt.localPosition = recipe.anchor == "ground" ? Vector3.up * Mathf.Min(W * 0.35f, 1.2f) : Vector3.zero;
                lightComp = SakugaFX.Need<Light>(lt.gameObject);
                lightComp.type = LightType.Point;
                lightComp.shadows = LightShadows.None;
                lightComp.enabled = false;
            }
            if (fx.decal.On && (decalMaterial || decalProjectorMaterial)) BuildDecal(fx.decal);
            if (castClip || loopClip || impactClip)
            {
                var a = Child("Audio");
                oneShot = Audio(a.gameObject, false);
                if (loopClip) loopSource = Audio(Child("AudioLoop").gameObject, true);
            }
            ApplyColors();
        }

        void BuildDecal(DecalLayer d)
        {
            decalTf = Child("Decal");
            var type = SakugaFX.DecalProjectorType;
            if (SakugaFX.UseDecalProjector && decalProjectorMaterial && type != null && SakugaFX.RendererHasDecals())
            {
                decalProjector = decalTf.GetComponent(type);
                if (!decalProjector) decalProjector = decalTf.gameObject.AddComponent(type);
                SakugaFX.SetMember(decalProjector, "material", decalProjectorMaterial);
                var mr0 = decalTf.GetComponent<MeshRenderer>();
                if (mr0) mr0.enabled = false;
            }
            else
            {
                var mf = SakugaFX.Need<MeshFilter>(decalTf.gameObject);
                mf.sharedMesh = SakugaFX.UnitQuad;
                var mr = SakugaFX.Need<MeshRenderer>(decalTf.gameObject);
                mr.sharedMaterial = decalMaterial;
                mr.shadowCastingMode = ShadowCastingMode.Off;
                mr.receiveShadows = false;
                decalRenderer = mr;
            }
            decalTf.gameObject.SetActive(false);
        }

        void SetProjector(float s, float fade)
        {
            SakugaFX.SetMember(decalProjector, "size", new Vector3(s, s, 1.2f));
            SakugaFX.SetMember(decalProjector, "pivot", Vector3.zero);
            SakugaFX.SetMember(decalProjector, "fadeFactor", fade);
            decalTf.position = groundPoint + Vector3.up * 0.3f;
        }

        Transform Child(string n)
        {
            var t = transform.Find(n);
            if (!t)
            {
                t = new GameObject(n).transform;
                t.SetParent(transform, false);
            }
            return t;
        }

        static AudioSource Audio(GameObject go, bool looping)
        {
            var a = SakugaFX.Need<AudioSource>(go);
            a.playOnAwake = false;
            a.loop = looping;
            a.spatialBlend = 0.6f;
            a.dopplerLevel = 0f;
            a.rolloffMode = AudioRolloffMode.Linear;
            a.minDistance = 4f;
            a.maxDistance = 60f;
            return a;
        }

        ParticleSystem Ps(string n, Material mat, int max, ParticleSystemRenderMode mode)
        {
            var t = Child(n);
            var ps = t.GetComponent<ParticleSystem>();
            bool fresh = !ps;
            if (fresh) ps = t.gameObject.AddComponent<ParticleSystem>();
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            systems.Add(ps);
            if (!fresh) return ps;                               // baked into the prefab already
            ps.useAutoRandomSeed = false;
            var m = ps.main;
            m.playOnAwake = false; m.loop = false; m.duration = Mathf.Max(ContactTime + 0.5f, 1f);
            m.simulationSpace = ParticleSystemSimulationSpace.World;
            m.scalingMode = ParticleSystemScalingMode.Hierarchy;
            m.startSpeed = 0f; m.maxParticles = max;
            var em = ps.emission; em.rateOverTime = 0f;
            var sh = ps.shape; sh.enabled = false;
            var r = t.GetComponent<ParticleSystemRenderer>();
            r.sharedMaterial = mat;
            r.renderMode = mode;
            r.sortMode = ParticleSystemSortMode.Distance;
            r.shadowCastingMode = ShadowCastingMode.Off;
            r.receiveShadows = false;
            r.maxParticleSize = 2f;
            r.alignment = ParticleSystemRenderSpace.View;
            return ps;
        }

        void Burst(ParticleSystem ps, int min, int max, float delay = 0f)
        {
            var em = ps.emission;
            em.enabled = true;
            em.SetBursts(new[] { new ParticleSystem.Burst(ContactTime + delay, (short)min, (short)max) });
        }

        static AnimationCurve Curve(params float[] tv)
        {
            var c = new AnimationCurve();
            for (int i = 0; i + 1 < tv.Length; i += 2) c.AddKey(tv[i], tv[i + 1]);
            return c;
        }

        static void SizeCurve(ParticleSystem ps, AnimationCurve c)
        {
            var s = ps.sizeOverLifetime; s.enabled = true; s.size = new ParticleSystem.MinMaxCurve(1f, c);
        }

        static void AlphaFade(ParticleSystem ps, float holdUntil, float start = 1f)
        {
            var g = new Gradient();
            g.SetKeys(new[] { new GradientColorKey(Color.white, 0), new GradientColorKey(Color.white, 1) },
                      new[] { new GradientAlphaKey(start, 0), new GradientAlphaKey(start, holdUntil), new GradientAlphaKey(0, 1) });
            var col = ps.colorOverLifetime; col.enabled = true; col.color = g;
        }

        static void Drag(ParticleSystem ps, float drag)
        {
            if (drag <= 0) return;
            var l = ps.limitVelocityOverLifetime; l.enabled = true; l.drag = drag; l.multiplyDragByParticleSize = false;
            l.multiplyDragByParticleVelocity = false; l.limit = 1000f;
        }

        static void Sheet(ParticleSystem ps)
        {
            var ts = ps.textureSheetAnimation; ts.enabled = true; ts.numTilesX = 2; ts.numTilesY = 2;
            ts.frameOverTime = new ParticleSystem.MinMaxCurve(0f);
            ts.startFrame = new ParticleSystem.MinMaxCurve(0f, 3.99f);
        }

        void Collide(ParticleSystem ps, float bounce, float lifetimeLoss)
        {
            var c = ps.collision; c.enabled = true; c.type = ParticleSystemCollisionType.Planes;
            c.SetPlane(0, groundPlane); c.bounce = bounce; c.dampen = 0.45f; c.lifetimeLoss = lifetimeLoss; c.radiusScale = 0.2f;
        }

        // ------------------------------------------------------------------ layout
        /// <summary>Place and size the drawing for the recipe's world size, anchor and orientation.</summary>
        public void LayoutMain()
        {
            if (!main) return;
            float hUnits = cell.y / SakugaFlipbookInfo.PixelsPerUnit;
            float s = Mathf.Max(recipe.world_size_m, 0.05f) / Mathf.Max(hUnits, 1e-3f);
            var t = main.transform;
            t.localScale = Vector3.one * s;
            float height = recipe.world_size_m;
            float pivotFromBottom = 1f - pivot.y;
            float lift = 0f;
            if (recipe.orientation == "ground") lift = 0.03f;
            else if (recipe.anchor == "ground") lift = (pivotFromBottom - bottomInset) * height;
            t.localPosition = new Vector3(0f, lift, 0f);
            t.localRotation = recipe.orientation == "ground" ? Quaternion.Euler(90f, 0f, 0f) : Quaternion.identity;
        }

        /// <summary>Beams: stretch the drawing from a to b (the effect's +X follows the beam).</summary>
        public void SetBeam(Vector3 a, Vector3 b)
        {
            var d = b - a;
            transform.position = (a + b) * 0.5f;
            if (d.sqrMagnitude > 1e-6f) transform.rotation = Quaternion.LookRotation(d.normalized) * Quaternion.Euler(0f, -90f, 0f);
            if (!main) return;
            float wUnits = cell.x / SakugaFlipbookInfo.PixelsPerUnit, hUnits = cell.y / SakugaFlipbookInfo.PixelsPerUnit;
            float sy = Mathf.Max(recipe.world_size_m, 0.05f) / Mathf.Max(hUnits, 1e-3f) * 0.5f;
            float sx = d.magnitude / Mathf.Max(wUnits, 1e-3f) / Mathf.Max(SizeScale, 1e-3f);
            main.transform.localScale = new Vector3(sx, sy, 1f);
        }
    }
}
