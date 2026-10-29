using System;
using System.Collections.Generic;
using System.IO;
using KanaWorks.SakugaVFX.Demo;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

namespace KanaWorks.SakugaVFX.EditorTools
{
    /// <summary>
    /// Offline rendering and batch-mode entry points.
    /// <code>
    /// Unity -batchmode -quit -projectPath P -executeMethod KanaWorks.SakugaVFX.EditorTools.SakugaBatch.BuildEverything
    /// Unity -batchmode -quit -projectPath P -executeMethod KanaWorks.SakugaVFX.EditorTools.SakugaBatch.RenderDemoStills -sakugaOut Stills
    /// Unity -batchmode -quit -projectPath P -executeMethod KanaWorks.SakugaVFX.EditorTools.SakugaBatch.RenderSequence -sakugaScene Assets/.../Sakuga_Arena.unity -sakugaFrom 0 -sakugaTo 10 -sakugaFps 30
    /// </code>
    /// Every frame is rebuilt with Simulate(), so frames can be rendered in any order and always match.
    /// </summary>
    public static class SakugaBatch
    {
        public static void BuildEverything()
        {
            SakugaFxBuilder.BuildAll();
            SakugaDemoBuilder.BuildAll();
        }

        static string Arg(string name, string fallback)
        {
            var a = Environment.GetCommandLineArgs();
            for (int i = 0; i < a.Length - 1; i++) if (a[i] == name) return a[i + 1];
            return fallback;
        }

        static float ArgF(string name, float fallback) =>
            float.TryParse(Arg(name, ""), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var v) ? v : fallback;

        /// <summary>Open a demo scene and render the frame at time t to a PNG.</summary>
        public static void RenderAt(Camera cam, string path, int width, int height, int supersample = 2)
        {
            Physics.SyncTransforms();
            // sRGB target without MSAA (MSAA offscreen targets with post-processing came out washed out); anti-aliasing
            // comes from rendering larger and filtering down
            int ss = Mathf.Clamp(supersample, 1, 4);
            var big = new RenderTexture(width * ss, height * ss, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
            var small = new RenderTexture(width, height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
            var old = cam.targetTexture;
            cam.targetTexture = big;
            cam.Render();
            cam.targetTexture = old;
            big.filterMode = FilterMode.Bilinear;
            Graphics.Blit(big, small);
            var prev = RenderTexture.active;
            RenderTexture.active = small;
            var tex = new Texture2D(width, height, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            tex.Apply();
            RenderTexture.active = prev;
            Object.DestroyImmediate(big);
            Object.DestroyImmediate(small);
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllBytes(path, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
        }

        /// <summary>Pose the open demo scene at time t (director timeline or gallery entry).</summary>
        public static bool PoseScene(float t, int galleryIndex = 0)
        {
            var d = UnityEngine.Object.FindFirstObjectByType<SakugaDemoDirector>();
            if (d) { d.RenderAt(t); return true; }
            var g = UnityEngine.Object.FindFirstObjectByType<SakugaGallery>();
            if (g) { g.RenderAt(galleryIndex, t); return true; }
            return false;
        }

        /// <summary>Stills of the four demo scenes at several times, into -sakugaOut (default: SakugaStills next to Assets).</summary>
        public static void RenderDemoStills()
        {
            string outDir = Arg("-sakugaOut", Path.GetFullPath(Path.Combine(Application.dataPath, "../SakugaStills")));
            int w = (int)ArgF("-sakugaWidth", 1600), h = (int)ArgF("-sakugaHeight", 900);
            var times = new Dictionary<string, float[]>
            {
                { SakugaDemoBuilder.Arena, new[] { 0.62f, 1.75f, 2.42f, 4.45f, 5.95f, 7.0f, 8.15f, 8.9f } },
                { SakugaDemoBuilder.TopDown, new[] { 0.55f, 1.95f, 3.6f, 4.65f, 5.75f, 7.15f } },
                { SakugaDemoBuilder.TowerDefense, new[] { 1.5f, 3.2f, 5.0f, 7.4f, 9.6f } },
            };
            foreach (var kv in times)
            {
                if (!File.Exists(kv.Key)) continue;
                EditorSceneManager.OpenScene(kv.Key, OpenSceneMode.Single);
                var cam = Camera.main;
                string name = Path.GetFileNameWithoutExtension(kv.Key);
                foreach (var t in kv.Value)
                {
                    PoseScene(t);
                    RenderAt(cam, Path.Combine(outDir, $"{name}_t{t:00.00}.png"), w, h);
                }
                SakugaFX.ClearAll();
            }
            if (File.Exists(SakugaDemoBuilder.Gallery))
            {
                EditorSceneManager.OpenScene(SakugaDemoBuilder.Gallery, OpenSceneMode.Single);
                var cam = Camera.main;
                var g = UnityEngine.Object.FindFirstObjectByType<SakugaGallery>();
                if (g)
                {
                    string[] show = { "fire_projectile_01", "ice_impact_01", "rock_column_01", "thunder_strike_01", "wind_slash_01",
                                      "light_shield_01", "dark_aura_01", "poison_impact_02", "fire_impact_03", "thunder_strike_03" };
                    foreach (var id in show)
                    {
                        int i = g.ids.IndexOf(id);
                        if (i < 0) continue;
                        var p = SakugaFX.Library.Find(id);
                        float t = p.recipe.category == "projectile" ? 0.45f : p.LoopsMain ? 0.9f : p.ContactTime + 0.18f;
                        g.RenderAt(i, t);
                        RenderAt(cam, Path.Combine(outDir, $"Sakuga_Gallery_{id}.png"), w, h);
                    }
                }
                SakugaFX.ClearAll();
            }
            Debug.Log("[SAKUGA] stills written to " + outDir);
        }

        /// <summary>Every effect in the gallery at a few moments, one PNG per effect and time (QA sheet input).</summary>
        public static void RenderGalleryAll()
        {
            string outDir = Arg("-sakugaOut", Path.GetFullPath(Path.Combine(Application.dataPath, "../SakugaStills/gallery")));
            int w = (int)ArgF("-sakugaWidth", 640), h = (int)ArgF("-sakugaHeight", 360);
            EditorSceneManager.OpenScene(SakugaDemoBuilder.Gallery, OpenSceneMode.Single);
            var cam = Camera.main;
            var g = UnityEngine.Object.FindFirstObjectByType<SakugaGallery>();
            for (int i = 0; i < g.ids.Count; i++)
            {
                var p = SakugaFX.Library.Find(g.ids[i]);
                if (!p) continue;
                float tc = p.recipe.category == "projectile" ? 0.3f : p.ContactTime;
                foreach (var dt in new[] { 0.06f, 0.35f })
                {
                    g.RenderAt(i, tc + dt);
                    RenderAt(cam, Path.Combine(outDir, $"{g.ids[i]}_{dt:0.00}.png"), w, h);
                }
            }
            SakugaFX.ClearAll();
        }

        /// <summary>A numbered PNG sequence of one demo scene (for a promo edit).</summary>
        public static void RenderSequence()
        {
            string scene = Arg("-sakugaScene", SakugaDemoBuilder.Arena);
            string outDir = Arg("-sakugaOut", Path.GetFullPath(Path.Combine(Application.dataPath, "../SakugaFrames")));
            float from = ArgF("-sakugaFrom", 0f), to = ArgF("-sakugaTo", 10f), fps = ArgF("-sakugaFps", 30f);
            int w = (int)ArgF("-sakugaWidth", 1920), h = (int)ArgF("-sakugaHeight", 1080);
            EditorSceneManager.OpenScene(scene, OpenSceneMode.Single);
            var cam = Camera.main;
            int n = 0;
            for (float t = from; t < to - 1e-4f; t = from + (++n) / fps)
            {
                PoseScene(t);
                RenderAt(cam, Path.Combine(outDir, $"frame_{n:00000}.png"), w, h);
            }
            SakugaFX.ClearAll();
            Debug.Log("[SAKUGA] " + n + " frames written to " + outDir);
        }
    }
}
