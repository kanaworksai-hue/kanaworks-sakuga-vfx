using System;
using System.Collections.Generic;
using UnityEngine;

namespace KanaWorks.SakugaVFX
{
    /// <summary>
    /// One scripted effect on a timeline: a cast (charge, flight, impact), a one-shot at a point, or a looping
    /// effect for a while. Fire() plays it live through SakugaFX; SimulateAll() rebuilds a whole timeline at any time
    /// t offline (deterministic), which is how the demo scenes are rendered to stills and how a promo can be recorded.
    /// </summary>
    [Serializable]
    public class SakugaShot
    {
        public enum Kind { Cast, Play, Hold }

        public Kind kind = Kind.Play;
        public float time;
        public string id;
        public Vector3 from, to;
        public Quaternion rotation = Quaternion.identity;
        [Min(0.01f)] public float scale = 1f;
        public float speed = 0f, arc = -1f, windup = 0f;
        [Tooltip("Hold: how long the looping effect runs.")]
        public float hold = 2f;
        public SakugaSize size = SakugaSize.M;

        public static SakugaShot Cast(float time, string projectileId, Vector3 from, Vector3 to, float speed = 0f, float arc = -1f, float scale = 1f)
            => new SakugaShot { kind = Kind.Cast, time = time, id = projectileId, from = from, to = to, speed = speed, arc = arc, scale = scale };

        public static SakugaShot At(float time, string id, Vector3 at, float scale = 1f, Quaternion rotation = default)
            => new SakugaShot { kind = Kind.Play, time = time, id = id, from = at, to = at, scale = scale, rotation = rotation == default ? Quaternion.identity : rotation };

        public static SakugaShot Loop(float time, string id, Vector3 at, float hold, float scale = 1f)
            => new SakugaShot { kind = Kind.Hold, time = time, id = id, from = at, to = at, hold = hold, scale = scale };

        /// <summary>Seconds from `time` until it is all over (flight + impact, or effect + fade).</summary>
        public float Span(SakugaLibrary lib)
        {
            var p = lib ? lib.Find(id) : null;
            if (!p) return 0f;
            switch (kind)
            {
                case Kind.Cast:
                    var pr = p.GetComponent<SakugaProjectile>();
                    float v = speed > 0f ? speed : pr ? pr.speed : 14f;
                    var imp = lib.Find(p.recipe.pairs?.impact ?? "");
                    return Vector3.Distance(from, to) / v + (imp ? imp.EstimatedDuration : 0.5f) + 0.6f;
                case Kind.Hold: return hold + 0.6f;
                default: return p.EstimatedDuration + 0.2f;
            }
        }

        /// <summary>Seconds from `time` to the moment it hits (casts: arrival; one-shots: contact frame).</summary>
        public float HitDelay(SakugaLibrary lib)
        {
            var p = lib ? lib.Find(id) : null;
            if (!p) return 0f;
            if (kind != Kind.Cast) return p.ContactTime;
            var pr = p.GetComponent<SakugaProjectile>();
            float v = speed > 0f ? speed : pr ? pr.speed : 14f;
            return windup + Vector3.Distance(from, to) / v;
        }

        /// <summary>Play it now on the game clock.</summary>
        public SakugaEffect Fire(Action<Vector3> onHit = null)
        {
            switch (kind)
            {
                case Kind.Cast:
                    var pr = SakugaFX.Cast(id, from, to, onHit, speed, arc, windup, scale);
                    return pr ? pr.Effect : null;
                case Kind.Hold:
                    var h = SakugaFX.Spawn(id);
                    if (!h) return null;
                    h.transform.SetPositionAndRotation(from, rotation);
                    h.scale = scale; h.size = size; h.lifetime = hold;
                    h.Play();
                    return h;
                default:
                    var fx = SakugaFX.Spawn(id);
                    if (!fx) return null;
                    fx.transform.SetPositionAndRotation(from, rotation);
                    fx.scale = scale; fx.size = size;
                    fx.Play();
                    return fx;
            }
        }

        /// <summary>Offline: spawn what this shot shows at `t` seconds after its start and simulate it there.</summary>
        public void SimulateAt(float t, float step = 1f / 60f)
        {
            if (t < 0f) return;
            if (kind == Kind.Cast)
            {
                var fx = SakugaFX.Spawn(id);
                if (!fx) return;
                fx.scale = scale; fx.size = size;
                string castId = fx.recipe.pairs?.cast;
                if (!string.IsNullOrEmpty(castId) && t < 1.5f)
                {
                    var c = SakugaFX.Spawn(castId);
                    if (c)
                    {
                        c.transform.position = from;
                        c.scale = 0.55f * scale; c.startAt = c.ContactTime; c.speed = 1.6f; c.seed = 7;
                        c.Simulate(t, step);
                    }
                }
                var pr = SakugaFX.Need<SakugaProjectile>(fx.gameObject);
                if (speed > 0f) pr.speed = speed;
                if (arc >= 0f) pr.arc = arc;
                pr.Launch(from, to);
                fx.seed = 11;
                fx.Simulate(t, step);
                return;
            }
            var e = SakugaFX.Spawn(id);
            if (!e) return;
            e.transform.SetPositionAndRotation(from, rotation);
            e.scale = scale; e.size = size; e.seed = 5;
            if (kind == Kind.Hold) e.lifetime = hold;
            e.Simulate(t, step);
        }

        /// <summary>Offline: clear every effect and rebuild the whole timeline at time T (timeline time, looping every
        /// `period` seconds when period &gt; 0).</summary>
        public static void SimulateAll(IList<SakugaShot> shots, float T, float period = 0f, float step = 1f / 60f)
        {
            SakugaFX.ClearAll();
            var lib = SakugaFX.Library;
            foreach (var s in shots)
            {
                float span = s.Span(lib);
                float local = T - s.time;
                if (period > 0f && local < 0f) local += period;           // tail of the previous lap
                if (local >= 0f && local < span) s.SimulateAt(local, step);
            }
        }
    }
}
