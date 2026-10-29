using System;
using UnityEngine;

namespace KanaWorks.SakugaVFX
{
    /// <summary>
    /// Plays one flipbook sheet on a quad through the KanaWorks/SakugaFlipbook shader: once or looping, at a
    /// fixed frame rate. The frame goes to the renderer through a MaterialPropertyBlock, so every instance of a
    /// prefab shares one material and still plays on its own clock.
    /// </summary>
    [ExecuteAlways]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(MeshRenderer))]
    [AddComponentMenu("KanaWorks/SAKUGA Flipbook")]
    public class SakugaFlipbook : MonoBehaviour
    {
        public enum EndAction { Hide, HoldLastFrame, Disable, Destroy }

        [Tooltip("Columns and rows of the sheet (from the .flipbook.json sidecar).")]
        public Vector2Int grid = new Vector2Int(8, 8);
        [Min(1)] public int frames = 64;
        [Min(0.01f)] public float fps = 30f;
        [Tooltip("Projectiles loop; impacts, columns and vortices play once.")]
        public bool loop;
        public bool playOnEnable = true;
        [Min(0f)] public float startDelay;
        [Tooltip("Playback speed multiplier (1 = the sheet's fps).")]
        [Min(0f)] public float speed = 1f;
        public bool useUnscaledTime;
        [Tooltip("Loops only: start at a random frame so many copies do not pulse in sync.")]
        public bool randomStartFrame;
        [Tooltip("What a one-shot does after its last frame.")]
        public EndAction onEnd = EndAction.Hide;
        [Tooltip("Frame shown in the editor while not playing.")]
        [Min(0)] public int previewFrame = 16;

        /// <summary>Set by SakugaEffect: the frame comes from outside (ShowFrame), nothing plays on its own.</summary>
        [HideInInspector] public bool externallyDriven;

        /// <summary>Raised once when a one-shot passes its last frame.</summary>
        public event Action<SakugaFlipbook> Finished;

        static readonly int FrameId = Shader.PropertyToID("_Frame");
        static readonly int InstanceColorId = Shader.PropertyToID("_InstanceColor");
        static readonly int RecolorId = Shader.PropertyToID("_Recolor");
        static readonly int RecolorTintId = Shader.PropertyToID("_RecolorTint");
        MeshRenderer rend;
        MaterialPropertyBlock block;
        float time;
        bool playing;
        int shown = int.MinValue;

        public bool IsPlaying => playing;
        public int CurrentFrame => shown;
        /// <summary>Length of one pass through the sheet in seconds (at speed 1).</summary>
        public float Duration => frames / Mathf.Max(fps, 0.01f);

        /// <summary>Frame index at time t (seconds since the first frame). -1 before the start; for a one-shot,
        /// `frames` once it has finished.</summary>
        public static int FrameAt(float t, float fps, int frames, bool loop)
        {
            if (frames < 1 || t < 0f) return -1;
            int f = Mathf.FloorToInt(t * fps + 1e-4f);
            return loop ? f % frames : Mathf.Min(f, frames);
        }

        /// <summary>Instantiate a flipbook prefab, play it and (by default) destroy it when it ends.</summary>
        public static SakugaFlipbook Spawn(SakugaFlipbook prefab, Vector3 position, Quaternion rotation, Transform parent = null,
                                           EndAction end = EndAction.Destroy)
        {
            var fx = Instantiate(prefab, position, rotation, parent);
            fx.onEnd = end;
            fx.Play();
            return fx;
        }

        public void Play()
        {
            time = -startDelay;
            if (loop && randomStartFrame) time = UnityEngine.Random.Range(0f, Duration);
            playing = true;
            Show(FrameAt(time, fps, frames, loop));
        }

        public void Stop()
        {
            playing = false;
            Show(-1);
        }

        /// <summary>Show one frame (-1 hides the quad). For external drivers such as SakugaEffect.</summary>
        public void ShowFrame(int frame) => Show(frame);

        /// <summary>Per-instance colour multiplier (alpha fades the whole sheet) and recolour toward a tint
        /// (0 = original colours, 1 = the sheet re-ramped to the tint by brightness).</summary>
        public void SetLook(Color multiply, float recolor, Color recolorTint)
        {
            if (!rend) rend = GetComponent<MeshRenderer>();
            if (!rend) return;
            block ??= new MaterialPropertyBlock();
            rend.GetPropertyBlock(block);
            block.SetColor(InstanceColorId, multiply);
            block.SetFloat(RecolorId, recolor);
            block.SetColor(RecolorTintId, recolorTint);
            rend.SetPropertyBlock(block);
        }

        void OnEnable()
        {
            if (externallyDriven) return;
            if (Application.isPlaying && playOnEnable) Play();
            else if (!Application.isPlaying) Show(Mathf.Clamp(previewFrame, 0, frames - 1));
            else Show(-1);
        }

        void OnValidate()
        {
            grid = Vector2Int.Max(grid, Vector2Int.one);
            frames = Mathf.Clamp(frames, 1, grid.x * grid.y);
            if (!Application.isPlaying && isActiveAndEnabled && !externallyDriven) Show(Mathf.Clamp(previewFrame, 0, frames - 1));
        }

        void Update()
        {
            if (!playing || !Application.isPlaying || externallyDriven) return;
            time += (useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime) * speed;
            int f = FrameAt(time, fps, frames, loop);
            if (f >= frames) { Finish(); return; }
            Show(f);
        }

        void Finish()
        {
            playing = false;
            switch (onEnd)
            {
                case EndAction.HoldLastFrame: Show(frames - 1); break;
                case EndAction.Disable: Show(-1); gameObject.SetActive(false); break;
                case EndAction.Destroy: Show(-1); Destroy(gameObject); break;
                default: Show(-1); break;
            }
            Finished?.Invoke(this);
        }

        void Show(int frame)
        {
            if (!rend) rend = GetComponent<MeshRenderer>();
            if (!rend) return;
            bool visible = frame >= 0;
            if (rend.enabled != visible) rend.enabled = visible;
            if (!visible || frame == shown) { shown = frame; return; }
            shown = frame;
            block ??= new MaterialPropertyBlock();
            rend.GetPropertyBlock(block);
            block.SetFloat(FrameId, frame);
            rend.SetPropertyBlock(block);
        }
    }
}
