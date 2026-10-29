using System.Collections.Generic;
using UnityEngine;

namespace KanaWorks.SakugaVFX.Demo
{
    /// <summary>A primitive demo character that reacts to hits: white flash, squash and a small knock back, plus an
    /// idle bob. Live (Hit) or offline (PoseAt with the list of hit times), so stills show the reaction too.</summary>
    [AddComponentMenu("KanaWorks/SAKUGA Demo Target")]
    public class SakugaDemoTarget : MonoBehaviour
    {
        [Tooltip("The visual root that squashes (child of this object).")]
        public Transform body;
        [Range(0f, 0.2f)] public float bob = 0.04f;
        public float bobSpeed = 2.6f;
        public float phase;

        static readonly int FlashId = Shader.PropertyToID("_HitFlash");
        Renderer[] rends;
        MaterialPropertyBlock block;
        Vector3 baseScale = Vector3.one, basePos;
        bool init;
        float liveHit = -100f;
        Vector3 liveDir;
        readonly List<(float t, Vector3 dir)> scheduled = new List<(float, Vector3)>();

        void Init()
        {
            if (init) return;
            init = true;
            if (!body) body = transform;
            baseScale = body.localScale;
            basePos = body.localPosition;
            rends = GetComponentsInChildren<Renderer>();
            block = new MaterialPropertyBlock();
        }

        /// <summary>Live: react now.</summary>
        public void Hit(Vector3 direction) { Init(); liveHit = Time.time; liveDir = direction; }

        public void ClearSchedule() => scheduled.Clear();
        public void Schedule(float time, Vector3 direction) => scheduled.Add((time, direction));
        public IReadOnlyList<(float t, Vector3 dir)> Scheduled => scheduled;

        void Update()
        {
            if (!Application.isPlaying) return;
            Pose(Time.time, Time.time - liveHit, liveDir);
        }

        /// <summary>Offline: pose at timeline time T from the scheduled hits.</summary>
        public void PoseAt(float T)
        {
            float best = -100f; Vector3 dir = Vector3.zero;
            foreach (var h in scheduled) if (h.t <= T && h.t > best) { best = h.t; dir = h.dir; }
            Pose(T, T - best, dir);
        }

        public void Pose(float clock, float age, Vector3 dir)
        {
            Init();
            float s = Mathf.Sin((clock + phase) * bobSpeed) * bob;
            float squash = age >= 0f && age < 0.4f ? Mathf.Sin(age / 0.4f * Mathf.PI) * 0.22f * (1f - age / 0.4f) : 0f;
            body.localScale = Vector3.Scale(baseScale, new Vector3(1f + squash * 0.6f, 1f + s - squash, 1f + squash * 0.6f));
            float kb = age >= 0f && age < 0.5f ? Mathf.Sin(Mathf.Clamp01(age / 0.5f) * Mathf.PI) * 0.22f : 0f;
            var flat = new Vector3(dir.x, 0f, dir.z);
            body.localPosition = basePos + (flat.sqrMagnitude > 1e-4f ? transform.InverseTransformDirection(flat.normalized) * kb : Vector3.zero);
            float flash = age >= 0f && age < 0.14f ? 0.85f * (1f - age / 0.14f) : 0f;
            foreach (var r in rends)
            {
                if (!r) continue;
                r.GetPropertyBlock(block);
                block.SetColor(FlashId, new Color(1f, 1f, 1f, flash));
                r.SetPropertyBlock(block);
            }
        }
    }
}
