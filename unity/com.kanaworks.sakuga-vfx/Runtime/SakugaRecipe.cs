using System;
using UnityEngine;

namespace KanaWorks.SakugaVFX
{
    /// <summary>
    /// The layered-effect recipe that comes with every flipbook (generated from the library's catalog and stored in
    /// the .flipbook.json sidecar). Engine-agnostic data: metres, seconds, frames, sRGB hex colours. A layer whose
    /// values are all zero is switched off (JSON null deserializes to an empty layer).
    /// </summary>
    [Serializable]
    public class SakugaRecipe
    {
        public string category = "impact";
        public string style = "classic";
        public string base_id;
        /// <summary>"center" or "ground": where the effect's origin goes.</summary>
        public string anchor = "center";
        /// <summary>"billboard", "upright", "ground" (flat on the floor) or "travel" (faces the camera, rolled along +X).</summary>
        public string orientation = "billboard";
        public float world_size_m = 2f;
        public float duration_s = 2f;
        public int contact_frame;
        /// <summary>[first, last] frames that repeat, or empty when the sheet plays once.</summary>
        public int[] loop_range;
        public SakugaPairs pairs = new SakugaPairs();
        public string[] tags;
        public string[] use_cases;
        public SakugaFxLayers fx = new SakugaFxLayers();

        public bool HasLoopRange => loop_range != null && loop_range.Length == 2 && loop_range[1] >= loop_range[0];
        public bool IsProjectile => category == "projectile";
        public bool IsBeam => category == "beam";
    }

    [Serializable]
    public class SakugaPairs
    {
        public string cast;
        public string projectile;
        public string impact;
    }

    [Serializable]
    public class SakugaFxLayers
    {
        public FlashLayer flash = new FlashLayer();
        public RingLayer ring = new RingLayer();
        public SparkLayer sparks = new SparkLayer();
        public DebrisLayer debris = new DebrisLayer();
        public SmokeLayer smoke = new SmokeLayer();
        public DecalLayer decal = new DecalLayer();
        public LightLayer light = new LightLayer();
        public TrailLayer trail = new TrailLayer();
        public ShakeLayer shake = new ShakeLayer();
        public float hit_stop_s;
        public SfxLayer sfx = new SfxLayer();
    }

    [Serializable] public class FlashLayer { public string color; public float size; public int frames; public bool On => size > 0; }

    [Serializable]
    public class RingLayer
    {
        public string color; public float size; public float duration;
        /// <summary>"view" (faces the camera) or "ground" (flat).</summary>
        public string plane = "view";
        public bool On => size > 0 && duration > 0;
    }

    [Serializable]
    public class SparkLayer
    {
        public int count; public float speed; public string color; public float gravity; public float drag; public float size; public float life;
        /// <summary>"radial", "up", "forward", "swirl" or "rise".</summary>
        public string direction = "radial";
        public bool On => count > 0 && life > 0;
    }

    [Serializable] public class DebrisLayer { public int count; public float speed; public string color; public float gravity; public float size; public float life; public bool On => count > 0 && life > 0; }
    [Serializable] public class SmokeLayer { public int count; public string color; public float size; public float rise; public float life; public float delay; public bool On => count > 0 && life > 0; }
    [Serializable] public class DecalLayer { public string texture; public float size; public float life; public string tint; public bool On => !string.IsNullOrEmpty(texture) && size > 0; }

    [Serializable]
    public class LightLayer
    {
        public string color; public float intensity; public float range;
        /// <summary>Flat pairs: seconds after the contact frame, intensity multiplier.</summary>
        public float[] curve;
        public bool On => intensity > 0 && range > 0;

        /// <summary>Piecewise-linear multiplier at t seconds after contact, clamped at both ends.</summary>
        public float Evaluate(float t)
        {
            if (curve == null || curve.Length < 2) return 1f;
            int n = curve.Length / 2;
            if (t <= curve[0]) return curve[1];
            for (int i = 1; i < n; i++)
            {
                float t0 = curve[(i - 1) * 2], v0 = curve[(i - 1) * 2 + 1], t1 = curve[i * 2], v1 = curve[i * 2 + 1];
                if (t <= t1) return t1 > t0 ? Mathf.Lerp(v0, v1, (t - t0) / (t1 - t0)) : v1;
            }
            return curve[(n - 1) * 2 + 1];
        }

        /// <summary>Seconds after contact when the curve has settled on its last value.</summary>
        public float End => curve != null && curve.Length >= 2 ? curve[curve.Length - 2] : 0f;
    }

    [Serializable] public class TrailLayer { public string color; public float rate; public float size; public float life; public bool On => rate > 0 && life > 0; }
    [Serializable] public class ShakeLayer { public float amplitude; public float duration; public bool On => amplitude > 0 && duration > 0; }
    [Serializable] public class SfxLayer { public string cast; public string loop; public string impact; }

    public static class SakugaColor
    {
        /// <summary>"#rrggbb" to a colour; white when empty or unreadable.</summary>
        public static Color Parse(string hex, Color fallback)
        {
            if (!string.IsNullOrEmpty(hex) && ColorUtility.TryParseHtmlString(hex, out var c)) return c;
            return fallback;
        }

        public static Color Parse(string hex) => Parse(hex, Color.white);
    }
}
