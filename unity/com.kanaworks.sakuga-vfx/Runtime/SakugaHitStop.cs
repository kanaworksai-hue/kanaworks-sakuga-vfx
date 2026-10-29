using UnityEngine;

namespace KanaWorks.SakugaVFX
{
    /// <summary>
    /// Hit stop: the game slows to 5 % for a few hundredths of a second when a heavy hit lands. Counted in real time,
    /// capped at 0.15 s of freeze in any one second so a burst of hits never locks the game, and it puts back the
    /// time scale it found. Turn SakugaFX.AutoHitStop off if your game manages Time.timeScale itself and listen to
    /// SakugaFX.HitStopRequested instead.
    /// </summary>
    public static class SakugaHitStop
    {
        public static float FrozenScale = 0.05f;
        public static float MaxFreezePerSecond = 0.15f;
        static float left, saved = 1f, written = -1f, windowStart, windowUsed, clock;
        static bool active;

        public static bool Active => active;

        public static void Request(float seconds)
        {
            if (seconds <= 0f) return;
            SakugaFX.Runner.Instance.enabled = true;
            if (clock - windowStart > 1f) { windowStart = clock; windowUsed = 0f; }
            float budget = MaxFreezePerSecond - windowUsed;
            if (budget <= 0f) return;
            left = Mathf.Max(left, Mathf.Min(seconds, budget));
            if (active) return;
            active = true;
            saved = Time.timeScale;
            written = saved * FrozenScale;
            Time.timeScale = written;
        }

        internal static void Tick(float unscaledDt)
        {
            clock += unscaledDt;
            if (!active) return;
            left -= unscaledDt;
            windowUsed += unscaledDt;
            if (left > 0f) return;
            active = false;
            if (Mathf.Approximately(Time.timeScale, written)) Time.timeScale = saved;   // someone else changed it: leave theirs
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics() { left = 0f; active = false; windowUsed = 0f; clock = 0f; windowStart = 0f; }
    }
}
