using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace KanaWorks.SakugaVFX
{
    /// <summary>
    /// One-line API over the layered effects, with pooling.
    /// <code>
    /// SakugaFX.Play("fire_impact_01", hitPoint);                                  // a hit
    /// SakugaFX.Cast("ice_projectile_01", hand.position, enemy, p => enemy.Damage()); // charge, flight, impact
    /// </code>
    /// Effects are looked up by id in a SakugaLibrary (Resources/SakugaLibrary, built by the importer), or in the
    /// library you assign to SakugaFX.Library.
    /// </summary>
    public static class SakugaFX
    {
        static SakugaLibrary library;
        static readonly Dictionary<string, Stack<SakugaEffect>> pools = new Dictionary<string, Stack<SakugaEffect>>();
        static readonly List<SakugaEffect> live = new List<SakugaEffect>();
        static Transform poolRoot;

        /// <summary>The effect library (default: Resources.Load("SakugaLibrary")).</summary>
        public static SakugaLibrary Library
        {
            get { if (!library) library = Resources.Load<SakugaLibrary>("SakugaLibrary"); return library; }
            set => library = value;
        }

        /// <summary>Layers that ground decals and ground rings look for (characters on Ignore Raycast are skipped).</summary>
        public static LayerMask GroundMask = Physics.DefaultRaycastLayers;
        /// <summary>Floor height used when no collider is found below an effect.</summary>
        public static float GroundHeight = 0f;
        /// <summary>How far below an effect to look for the floor (m).</summary>
        public static float GroundSearch = 6f;
        public static bool Sound = true, Lights = true;
        public static float Volume = 1f;
        /// <summary>Forward contact-frame shakes to the built-in SakugaCameraShake (turn off to route ShakeRequested
        /// to your own camera system, for example a Cinemachine impulse source).</summary>
        public static bool AutoShake = true;
        /// <summary>Apply hit-stop requests with SakugaHitStop (turn off if your game owns Time.timeScale).</summary>
        public static bool AutoHitStop = true;
        public static int MaxPoolPerId = 32;
        /// <summary>Use a URP Decal Projector for ground decals when the renderer has the Decal feature (experimental;
        /// off by default: the flat ground quad is used, which looks the same on flat floors).</summary>
        public static bool UseDecalProjector = false;

        /// <summary>Every contact-frame shake: (amplitude m, duration s, world position).</summary>
        public static event Action<float, float, Vector3> ShakeRequested;
        /// <summary>Every hit-stop request (seconds of real time).</summary>
        public static event Action<float> HitStopRequested;

        // ------------------------------------------------------------------ play
        /// <summary>A pooled instance of an effect, active and positioned at the origin but not playing.</summary>
        public static SakugaEffect Spawn(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            SakugaEffect fx = null;
            if (pools.TryGetValue(id, out var stack))
                while (stack.Count > 0 && !fx) fx = stack.Pop();
            if (!fx)
            {
                var prefab = Library ? Library.Find(id) : null;
                if (!prefab) { Debug.LogWarning("[SAKUGA] no effect \"" + id + "\" in the library"); return null; }
                fx = Object.Instantiate(prefab, Root());
                fx.name = id;
                fx.poolKey = id;
                fx.pooled = true;
            }
            fx.transform.SetParent(Root(), false);
            fx.gameObject.SetActive(true);
            fx.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            ResetSettings(fx);
            fx.Clear();
            live.Add(fx);
            return fx;
        }

        /// <summary>Play an effect once at a position. Scale multiplies the size preset. Looping effects run until
        /// you call Stop() on the returned effect (or set its lifetime).</summary>
        public static SakugaEffect Play(string id, Vector3 position, Quaternion rotation = default, float scale = 1f)
        {
            var fx = Spawn(id);
            if (!fx) return null;
            fx.transform.SetPositionAndRotation(position, rotation == default ? Quaternion.identity : rotation);
            fx.scale = scale;
            fx.Play();
            return fx;
        }

        /// <summary>Play an effect attached to a transform (auras, shields, buffs). It follows the parent.</summary>
        public static SakugaEffect Attach(string id, Transform parent, Vector3 localOffset = default, float scale = 1f, float lifetime = 0f)
        {
            var fx = Spawn(id);
            if (!fx) return null;
            fx.transform.SetParent(parent, false);
            fx.transform.localPosition = localOffset;
            fx.scale = scale;
            fx.lifetime = lifetime;
            fx.Play();
            return fx;
        }

        /// <summary>Fire a projectile at a point: its charge (muzzle flash) at `from`, the flight, and its paired impact.
        /// windup &gt; 0 plays the whole charge first and launches when it releases.</summary>
        public static SakugaProjectile Cast(string projectileId, Vector3 from, Vector3 to, Action<Vector3> onHit = null,
                                            float speed = 0f, float arc = -1f, float windup = 0f, float scale = 1f)
            => CastInternal(projectileId, from, to, null, onHit, speed, arc, windup, scale);

        /// <summary>Fire a projectile at a target transform (homes in on it). aimHeight is added to its position.</summary>
        public static SakugaProjectile Cast(string projectileId, Vector3 from, Transform target, Action<Vector3> onHit = null,
                                            float aimHeight = 1f, float speed = 0f, float windup = 0f, float scale = 1f)
            => CastInternal(projectileId, from, target ? target.position + Vector3.up * aimHeight : from, target, onHit, speed, -1f, windup, scale);

        static SakugaProjectile CastInternal(string id, Vector3 from, Vector3 to, Transform target, Action<Vector3> onHit,
                                             float speed, float arc, float windup, float scale)
        {
            var fx = Spawn(id);
            if (!fx) return null;
            var proj = Need<SakugaProjectile>(fx.gameObject);
            if (speed > 0f) proj.speed = speed;
            if (arc >= 0f) proj.arc = arc;
            fx.scale = scale;
            string cast = fx.recipe.pairs?.cast;
            if (!string.IsNullOrEmpty(cast))
            {
                var c = Spawn(cast);
                if (c)
                {
                    c.transform.SetPositionAndRotation(from, Quaternion.identity);
                    c.scale = 0.55f * scale;
                    if (windup > 0f) c.speed = c.ContactTime / windup; else { c.startAt = c.ContactTime; c.speed = 1.6f; }
                    c.shake = false;
                    c.Play();
                }
            }
            proj.Launch(from, to, target, onHit);
            fx.transform.position = from;
            if (windup > 0f) Runner.Instance.Delay(windup, () => { if (fx && fx.gameObject.activeInHierarchy) { proj.Launch(from, to, target, onHit); fx.Play(); } });
            else fx.Play();
            return proj;
        }

        /// <summary>A looping beam from a to b (category "beam"). Stop() it when the beam ends.</summary>
        public static SakugaEffect Beam(string id, Vector3 a, Vector3 b, float scale = 1f)
        {
            var fx = Spawn(id);
            if (!fx) return null;
            fx.scale = scale;
            fx.Play();
            fx.SetBeam(a, b);
            return fx;
        }

        /// <summary>Return an effect to its pool (done automatically when a pooled effect finishes).</summary>
        public static void Release(SakugaEffect fx)
        {
            if (!fx) return;
            live.Remove(fx);
            fx.Clear();
            if (!fx.pooled || string.IsNullOrEmpty(fx.poolKey)) { Kill(fx.gameObject); return; }
            if (!pools.TryGetValue(fx.poolKey, out var stack)) pools[fx.poolKey] = stack = new Stack<SakugaEffect>();
            if (stack.Count >= MaxPoolPerId) { Kill(fx.gameObject); return; }
            fx.transform.SetParent(Root(), false);
            fx.gameObject.SetActive(false);
            stack.Push(fx);
        }

        /// <summary>Destroy every pooled and playing effect (scene changes, offline renders).</summary>
        public static void ClearAll()
        {
            foreach (var fx in live.ToArray()) if (fx) Kill(fx.gameObject);
            live.Clear();
            foreach (var s in pools.Values) foreach (var fx in s) if (fx) Kill(fx.gameObject);
            pools.Clear();
            if (poolRoot) Kill(poolRoot.gameObject);
            poolRoot = null;
        }

        public static IReadOnlyList<SakugaEffect> Playing => live;

        static void ResetSettings(SakugaEffect fx)
        {
            fx.scale = 1f; fx.speed = 1f; fx.startAt = 0f; fx.lifetime = 0f; fx.hitDirection = Vector3.zero;
            fx.overrideColor = false; fx.shake = true; fx.size = SakugaSize.M; fx.seed = 0;
        }

        static void Kill(GameObject go)
        {
            if (Application.isPlaying) Object.Destroy(go); else Object.DestroyImmediate(go);
        }

        static Transform Root()
        {
            if (poolRoot) return poolRoot;
            var go = new GameObject("[SakugaFX]");
            if (Application.isPlaying) Object.DontDestroyOnLoad(go);
            else go.hideFlags = HideFlags.DontSave;
            poolRoot = go.transform;
            return poolRoot;
        }

        // ------------------------------------------------------------------ hit feel
        internal static void RaiseShake(float amplitude, float duration, Vector3 position)
        {
            ShakeRequested?.Invoke(amplitude, duration, position);
            if (AutoShake && Application.isPlaying) SakugaCameraShake.Shake(amplitude, duration);
        }

        internal static void RaiseHitStop(float seconds)
        {
            HitStopRequested?.Invoke(seconds);
            if (AutoHitStop && Application.isPlaying) SakugaHitStop.Request(seconds);
        }

        // ------------------------------------------------------------------ pipeline queries (reflection keeps the
        // runtime assembly free of a compile-time URP reference)
        static Mesh quad;
        /// <summary>A 1 x 1 quad in the XY plane, centred, facing -Z (flat decals rotate it onto the floor).</summary>
        public static Mesh UnitQuad
        {
            get
            {
                if (quad) return quad;
                quad = new Mesh { name = "SakugaUnitQuad", hideFlags = HideFlags.DontSave };
                quad.vertices = new[] { new Vector3(-0.5f, -0.5f), new Vector3(0.5f, -0.5f), new Vector3(-0.5f, 0.5f), new Vector3(0.5f, 0.5f) };
                quad.uv = new[] { new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, 1), new Vector2(1, 1) };
                quad.normals = new[] { Vector3.back, Vector3.back, Vector3.back, Vector3.back };
                quad.triangles = new[] { 0, 2, 1, 2, 3, 1 };
                quad.RecalculateBounds();
                return quad;
            }
        }

        static Type decalType;
        static bool decalTypeLooked;
        public static Type DecalProjectorType
        {
            get
            {
                if (!decalTypeLooked)
                {
                    decalTypeLooked = true;
                    decalType = Type.GetType("UnityEngine.Rendering.Universal.DecalProjector, Unity.RenderPipelines.Universal.Runtime");
                }
                return decalType;
            }
        }

        static RenderPipelineAsset checkedPipeline;
        static bool pipelineHasDecals, pipelineHasDepth;

        /// <summary>True when the active URP renderer has an enabled Decal renderer feature.</summary>
        public static bool RendererHasDecals() { CheckPipeline(); return pipelineHasDecals; }

        /// <summary>True when the active URP asset renders the camera depth texture (needed by soft particles).</summary>
        public static bool PipelineHasDepth() { CheckPipeline(); return pipelineHasDepth; }

        /// <summary>Forget what was learned about the pipeline (after changing renderer features).</summary>
        public static void RefreshPipelineInfo() { checkedPipeline = null; EnsureGlobals(); }

        static void CheckPipeline()
        {
            var rp = GraphicsSettings.currentRenderPipeline;
            if (rp == checkedPipeline && rp) return;
            checkedPipeline = rp;
            pipelineHasDecals = pipelineHasDepth = false;
            if (!rp) return;
            const BindingFlags F = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            try
            {
                var depth = rp.GetType().GetProperty("supportsCameraDepthTexture", F);
                pipelineHasDepth = depth != null && (bool)depth.GetValue(rp);
                var list = rp.GetType().GetField("m_RendererDataList", F)?.GetValue(rp) as Array;
                var idxField = rp.GetType().GetField("m_DefaultRendererIndex", F);
                int idx = idxField != null ? (int)idxField.GetValue(rp) : 0;
                if (list != null && idx >= 0 && idx < list.Length && list.GetValue(idx) is ScriptableObject data)
                {
                    var features = data.GetType().GetProperty("rendererFeatures", F)?.GetValue(data) as System.Collections.IEnumerable;
                    if (features != null)
                        foreach (var f in features)
                            if (f is ScriptableObject so && so && f.GetType().Name == "DecalRendererFeature")
                            {
                                var active = f.GetType().GetProperty("isActive", F);
                                pipelineHasDecals |= active == null || (bool)active.GetValue(f);
                            }
                }
            }
            catch (Exception e) { Debug.LogWarning("[SAKUGA] could not read the render pipeline settings: " + e.Message); }
        }

        static readonly int DepthReadyId = Shader.PropertyToID("_SakugaDepthReady");

        /// <summary>Tell the shaders whether soft particles can read the depth texture.</summary>
        public static void EnsureGlobals() => Shader.SetGlobalFloat(DepthReadyId, PipelineHasDepth() ? 1f : 0f);

        /// <summary>The component of type T on go, added when missing.</summary>
        public static T Need<T>(GameObject go) where T : Component
        {
            if (!go.TryGetComponent(out T c)) c = go.AddComponent<T>();
            return c;
        }

        /// <summary>Set a public property or field by name (used for URP components through reflection).</summary>
        public static void SetMember(object target, string name, object value)
        {
            if (target == null) return;
            var t = target.GetType();
            var p = t.GetProperty(name, BindingFlags.Instance | BindingFlags.Public);
            if (p != null && p.CanWrite) { p.SetValue(target, value); return; }
            t.GetField(name, BindingFlags.Instance | BindingFlags.Public)?.SetValue(target, value);
        }

        /// <summary>Small hidden helper for delayed calls (wind-ups) and hit stop.</summary>
        internal class Runner : MonoBehaviour
        {
            static Runner inst;
            readonly List<(float at, Action a)> jobs = new List<(float, Action)>();
            float clock;

            public static Runner Instance
            {
                get
                {
                    if (inst) return inst;
                    var go = new GameObject("[SakugaFX Runner]") { hideFlags = HideFlags.HideAndDontSave };
                    if (Application.isPlaying) DontDestroyOnLoad(go);
                    inst = go.AddComponent<Runner>();
                    return inst;
                }
            }

            public void Delay(float seconds, Action a) => jobs.Add((clock + seconds, a));

            void Update()
            {
                clock += UnityEngine.Time.deltaTime;
                for (int i = jobs.Count - 1; i >= 0; i--)
                    if (jobs[i].at <= clock) { var a = jobs[i].a; jobs.RemoveAt(i); a?.Invoke(); }
                SakugaHitStop.Tick(UnityEngine.Time.unscaledDeltaTime);
            }
        }
    }
}
