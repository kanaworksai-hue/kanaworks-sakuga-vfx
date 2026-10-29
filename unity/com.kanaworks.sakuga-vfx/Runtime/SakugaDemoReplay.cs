using System.Collections.Generic;
using UnityEngine;

namespace KanaWorks.SakugaVFX
{
    /// <summary>Demo helper: replays every one-shot flipbook under this object after a short pause.</summary>
    [AddComponentMenu("KanaWorks/SAKUGA Demo Replay")]
    public class SakugaDemoReplay : MonoBehaviour
    {
        [Min(0f)] public float pause = 0.6f;
        readonly Dictionary<SakugaFlipbook, float> waiting = new Dictionary<SakugaFlipbook, float>();

        void Update()
        {
            foreach (var fx in GetComponentsInChildren<SakugaFlipbook>())
            {
                if (fx.loop || fx.IsPlaying) { waiting.Remove(fx); continue; }
                waiting.TryGetValue(fx, out float t);
                t += Time.deltaTime;
                if (t >= pause) { fx.Play(); waiting.Remove(fx); }
                else waiting[fx] = t;
            }
        }
    }
}
