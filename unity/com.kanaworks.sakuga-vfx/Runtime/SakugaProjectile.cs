using System;
using UnityEngine;

namespace KanaWorks.SakugaVFX
{
    /// <summary>
    /// Flies a projectile effect from A to B: constant speed, optional arc and homing, the drawing rolled along the
    /// flight for "travel" sheets, a trail emitted by distance, and its paired impact spawned on arrival.
    /// Works live (SakugaFX.Cast) and offline: SakugaEffect.Simulate(t) steps the flight and the impact it spawns.
    /// </summary>
    [RequireComponent(typeof(SakugaEffect))]
    [AddComponentMenu("KanaWorks/SAKUGA Projectile")]
    public class SakugaProjectile : MonoBehaviour, ISakugaStepper
    {
        [Min(0.1f)] public float speed = 14f;
        [Tooltip("Arc height as a fraction of the distance (0 = straight).")]
        [Range(0f, 1f)] public float arc = 0.08f;
        [Tooltip("How hard it steers toward a moving target (0 = aims at where the target was).")]
        [Range(0f, 1f)] public float homing = 0.6f;
        [Tooltip("Impact to spawn on arrival (empty = the recipe's pair).")]
        public string impactId;
        [Min(0.01f)] public float impactScale = 1f;
        [Min(0.1f)] public float maxFlightTime = 6f;

        /// <summary>Raised on arrival with the hit point.</summary>
        public event Action<SakugaProjectile, Vector3> Hit;

        SakugaEffect fx;
        Vector3 from, to, aimOffset, lastDir = Vector3.forward;
        Transform target;
        float flown, flight;
        bool flying, launched;
        SakugaEffect spawnedImpact;
        Action<Vector3> onHit;

        public bool Busy => flying;
        public SakugaEffect Effect => fx ? fx : (fx = GetComponent<SakugaEffect>());
        public SakugaEffect SpawnedImpact => spawnedImpact;
        public Vector3 Origin => from;
        public Vector3 Destination => target ? target.position + aimOffset : to;

        /// <summary>Seconds to fly from a to b at this speed (straight-line distance; arcs add a little).</summary>
        public float FlightTime(Vector3 a, Vector3 b) => Vector3.Distance(a, b) / Mathf.Max(speed, 0.1f);

        /// <summary>Set the path. Call before Play() / BeginManual(). With a target transform the end point follows it
        /// (aimOffset is added to its position).</summary>
        public void Launch(Vector3 start, Vector3 end, Transform follow = null, Action<Vector3> hit = null, Vector3 offset = default)
        {
            from = start; to = end; target = follow; onHit = hit; aimOffset = offset;
            if (follow && offset == default) aimOffset = end - follow.position;
            launched = true;
            ResetStepper();
        }

        public void ResetStepper()
        {
            if (!launched) { from = transform.position; to = from + transform.forward * 10f; }
            flown = 0f;
            flying = true;
            flight = Mathf.Min(FlightTime(from, Destination), maxFlightTime);
            if (spawnedImpact) { SakugaFX.Release(spawnedImpact); spawnedImpact = null; }
            Place(0f);
        }

        Vector3 PointAt(float s)
        {
            var end = Destination;
            var p = Vector3.Lerp(from, end, s);
            p.y += arc * Vector3.Distance(from, end) * 4f * s * (1f - s);
            return p;
        }

        void Place(float s)
        {
            var p = PointAt(s);
            var ahead = PointAt(Mathf.Min(s + 0.02f, 1f));
            var dir = ahead - p;
            if (dir.sqrMagnitude > 1e-8f) lastDir = dir.normalized;
            transform.position = p;
            transform.rotation = Quaternion.LookRotation(lastDir) * Quaternion.Euler(0f, -90f, 0f);   // +X follows the flight
        }

        public void Step(float dt, bool live)
        {
            if (spawnedImpact && !live) spawnedImpact.StepManual(dt);
            if (!flying) return;
            if (target && homing > 0f)
            {
                var want = target.position + aimOffset;
                to = Vector3.Lerp(to, want, 1f - Mathf.Exp(-homing * 12f * dt));
                float left = Vector3.Distance(transform.position, to) / Mathf.Max(speed, 0.1f);
                flight = Mathf.Max(flown + left, flown + dt);
            }
            flown += dt;
            float s = flight > 0f ? Mathf.Clamp01(flown / flight) : 1f;
            Place(s);
            if (s >= 1f || flown >= maxFlightTime) Arrive(live);
        }

        void Arrive(bool live)
        {
            flying = false;
            var p = transform.position;
            var e = Effect;
            string id = string.IsNullOrEmpty(impactId) ? e.recipe.pairs?.impact : impactId;
            if (!string.IsNullOrEmpty(id))
            {
                var rot = Quaternion.identity;
                var imp = SakugaFX.Spawn(id);
                if (imp)
                {
                    imp.transform.SetPositionAndRotation(p, rot);
                    imp.scale = impactScale * e.scale;
                    imp.size = e.size;
                    imp.hitDirection = lastDir;
                    if (e.overrideColor) { imp.overrideColor = true; imp.color = e.color; }
                    if (live) imp.Play(); else { imp.BeginManual(); spawnedImpact = imp; }
                }
            }
            e.Stop();
            Hit?.Invoke(this, p);
            if (live) onHit?.Invoke(p);
        }
    }
}
