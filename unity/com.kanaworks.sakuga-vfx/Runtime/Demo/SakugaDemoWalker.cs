using UnityEngine;

namespace KanaWorks.SakugaVFX.Demo
{
    /// <summary>Walks a closed or open path at a constant speed, as a pure function of time (so offline stills and
    /// aimed shots agree with where the walker is).</summary>
    [AddComponentMenu("KanaWorks/SAKUGA Demo Walker")]
    public class SakugaDemoWalker : MonoBehaviour
    {
        public Vector3[] path;
        public float speed = 1.2f;
        [Tooltip("Seconds of delay before this walker enters the path (it repeats every period).")]
        public float delay;
        public float period = 12f;

        public Vector3 PositionAt(float T) => Evaluate(path, speed, delay, period, T);

        public static Vector3 Evaluate(Vector3[] path, float speed, float delay, float period, float T)
        {
            if (path == null || path.Length == 0) return Vector3.zero;
            float t = T - delay;
            if (period > 0f) t = Mathf.Repeat(t, period);
            float d = Mathf.Max(t, 0f) * speed;
            for (int i = 1; i < path.Length; i++)
            {
                float seg = Vector3.Distance(path[i - 1], path[i]);
                if (d <= seg) return Vector3.Lerp(path[i - 1], path[i], seg > 0f ? d / seg : 1f);
                d -= seg;
            }
            return path[path.Length - 1];
        }

        public void PoseAt(float T)
        {
            var p = PositionAt(T);
            var ahead = PositionAt(T + 0.1f);
            transform.position = p;
            var dir = ahead - p; dir.y = 0f;
            if (dir.sqrMagnitude > 1e-5f) transform.rotation = Quaternion.LookRotation(dir);
        }
    }
}
