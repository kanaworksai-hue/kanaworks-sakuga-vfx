using System;
using System.Collections.Generic;
using UnityEngine;

namespace KanaWorks.SakugaVFX.Demo
{
    /// <summary>
    /// Plays a looping timeline of SakugaShots (casts, hits, loops) with target reactions and walkers. Live in Play
    /// mode through SakugaFX; RenderAt(T) rebuilds the same moment offline for stills and recordings.
    /// </summary>
    [AddComponentMenu("KanaWorks/SAKUGA Demo Director")]
    public class SakugaDemoDirector : MonoBehaviour
    {
        [Serializable]
        public class HitMark
        {
            public SakugaDemoTarget target;
            public float time;
            public Vector3 direction;
        }

        public float period = 10f;
        public bool autoPlay = true;
        public List<SakugaShot> shots = new List<SakugaShot>();
        public List<HitMark> hits = new List<HitMark>();
        [Tooltip("Optional on-screen caption.")]
        public TextMesh caption;
        [TextArea] public string captionText;

        float clock;

        void Start()
        {
            SakugaFX.GroundHeight = 0f;
            if (caption) caption.text = captionText;
            foreach (var h in hits) if (h.target) h.target.ClearSchedule();
        }

        void Update()
        {
            if (!autoPlay) return;
            float prev = clock;
            clock += Time.deltaTime;
            Fire(prev, Mathf.Min(clock, period));
            if (clock >= period)
            {
                clock -= period;
                Fire(-0.0001f, clock);
            }
            foreach (var w in Walkers) w.PoseAt(clock);
        }

        void Fire(float from, float to)
        {
            foreach (var s in shots) if (s.time > from && s.time <= to) s.Fire();
            foreach (var h in hits) if (h.time > from && h.time <= to && h.target) h.target.Hit(h.direction);
        }

        SakugaDemoWalker[] walkers;
        SakugaDemoWalker[] Walkers => walkers ??= FindObjectsByType<SakugaDemoWalker>(FindObjectsSortMode.None);

        /// <summary>Offline: the scene exactly as it is T seconds into the loop.</summary>
        public void RenderAt(float T)
        {
            walkers = null;
            float local = period > 0f ? Mathf.Repeat(T, period) : T;
            foreach (var w in Walkers) w.PoseAt(local);
            var byTarget = new Dictionary<SakugaDemoTarget, List<HitMark>>();
            foreach (var h in hits)
            {
                if (!h.target) continue;
                if (!byTarget.TryGetValue(h.target, out var l)) byTarget[h.target] = l = new List<HitMark>();
                l.Add(h);
            }
            foreach (var kv in byTarget)
            {
                kv.Key.ClearSchedule();
                foreach (var h in kv.Value) { kv.Key.Schedule(h.time, h.direction); kv.Key.Schedule(h.time - period, h.direction); }
                kv.Key.PoseAt(local);
            }
            foreach (var t in FindObjectsByType<SakugaDemoTarget>(FindObjectsSortMode.None))
                if (!byTarget.ContainsKey(t)) t.PoseAt(local);
            SakugaShot.SimulateAll(shots, local, period);
        }

        /// <summary>Register a shot and the hit reaction it causes on a target.</summary>
        public void Add(SakugaShot shot, SakugaDemoTarget target = null)
        {
            shots.Add(shot);
            if (!target) return;
            var lib = SakugaFX.Library;
            var dir = shot.to - shot.from;
            hits.Add(new HitMark { target = target, time = shot.time + shot.HitDelay(lib), direction = dir.sqrMagnitude > 1e-4f ? dir.normalized : Vector3.forward });
        }
    }
}
