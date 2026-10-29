using UnityEngine;
using UnityEngine.Rendering;

namespace KanaWorks.SakugaVFX
{
    /// <summary>
    /// Camera shake that never fights a camera controller (Cinemachine or your own): the offset is added just
    /// before the camera renders and removed right after, so the camera's Transform is untouched between frames.
    /// Put it on a camera to shake that camera; otherwise SakugaFX shakes Camera.main through a hidden instance.
    /// Uses unscaled time, so hit stop does not freeze the shake.
    /// </summary>
    [AddComponentMenu("KanaWorks/SAKUGA Camera Shake")]
    public class SakugaCameraShake : MonoBehaviour
    {
        [Tooltip("Multiplier on every shake (0 turns shakes off for this camera).")]
        [Range(0f, 3f)] public float strength = 1f;
        [Tooltip("Also roll the camera a little (degrees per metre of amplitude).")]
        public float rollPerMetre = 12f;
        [Range(5f, 60f)] public float frequency = 22f;

        static SakugaCameraShake global;
        float amp, dur, t;
        Vector3 posOffset;
        Quaternion rotOffset = Quaternion.identity;
        Camera shaken;

        /// <summary>Start a shake (amplitude in metres, duration in seconds). A weaker shake never cuts a stronger one.</summary>
        public static void Shake(float amplitude, float duration)
        {
            var s = FindTarget();
            if (s) s.Add(amplitude, duration);
        }

        static SakugaCameraShake FindTarget()
        {
            var cam = Camera.main;
            if (cam && cam.TryGetComponent(out SakugaCameraShake own)) return own;
            if (global) return global;
            var go = new GameObject("[SakugaCameraShake]") { hideFlags = HideFlags.HideAndDontSave };
            if (Application.isPlaying) DontDestroyOnLoad(go);
            global = go.AddComponent<SakugaCameraShake>();
            return global;
        }

        public void Add(float amplitude, float duration)
        {
            float left = dur > 0 ? amp * (1f - Mathf.Clamp01(t / dur)) : 0f;
            if (amplitude * strength < left) return;
            amp = amplitude * strength; dur = Mathf.Max(duration, 0.01f); t = 0f;
        }

        /// <summary>Offset at time t into a shake of this amplitude and duration (also usable offline).</summary>
        public static Vector3 Offset(float amplitude, float duration, float t, float frequency = 22f, int seed = 0)
        {
            if (t < 0f || t >= duration) return Vector3.zero;
            float k = 1f - t / duration; k *= k;
            float x = Mathf.PerlinNoise(seed * 0.37f + t * frequency, 0.1f) * 2f - 1f;
            float y = Mathf.PerlinNoise(0.7f, seed * 0.53f + t * frequency) * 2f - 1f;
            return new Vector3(x, y, 0f) * amplitude * k;
        }

        void Update() { if (t < dur) t += Time.unscaledDeltaTime; }

        void OnEnable()
        {
            RenderPipelineManager.beginCameraRendering += Begin;
            RenderPipelineManager.endCameraRendering += End;
        }

        void OnDisable()
        {
            RenderPipelineManager.beginCameraRendering -= Begin;
            RenderPipelineManager.endCameraRendering -= End;
        }

        Camera Target()
        {
            if (this != global && TryGetComponent(out Camera own)) return own;
            return Camera.main;
        }

        void Begin(ScriptableRenderContext ctx, Camera cam)
        {
            if (t >= dur || cam != Target() || cam.cameraType != CameraType.Game) return;
            var o = Offset(amp, dur, t, frequency);
            posOffset = cam.transform.right * o.x + cam.transform.up * o.y;
            rotOffset = Quaternion.AngleAxis(o.x * rollPerMetre, cam.transform.forward);
            cam.transform.position += posOffset;
            cam.transform.rotation = rotOffset * cam.transform.rotation;
            shaken = cam;
        }

        void End(ScriptableRenderContext ctx, Camera cam)
        {
            if (cam != shaken) return;
            cam.transform.rotation = Quaternion.Inverse(rotOffset) * cam.transform.rotation;
            cam.transform.position -= posOffset;
            shaken = null;
        }
    }
}
