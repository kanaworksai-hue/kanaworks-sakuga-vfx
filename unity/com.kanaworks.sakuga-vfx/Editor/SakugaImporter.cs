using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace KanaWorks.SakugaVFX.EditorTools
{
    /// <summary>
    /// Import rules for any PNG that has a &lt;name&gt;.flipbook.json sidecar next to it: no mipmaps (the 6 px
    /// transparent gutters do not survive low mips), clamped, never rescaled to a power of two (8x5 and 8x7 sheets
    /// would shift every cell), up to 4096 on desktop and 2048 ASTC 6x6 on Android and iOS.
    /// When a sidecar is imported, its material, particle material, quad mesh and prefab are built next to it.
    /// </summary>
    public class SakugaImporter : AssetPostprocessor
    {
        public const string SidecarSuffix = ".flipbook.json";

        public static bool HasSidecar(string assetPath) =>
            assetPath.EndsWith(".png", System.StringComparison.OrdinalIgnoreCase) && File.Exists(Path.ChangeExtension(assetPath, SidecarSuffix));

        void OnPreprocessTexture()
        {
            if (!HasSidecar(assetPath)) return;
            var ti = (TextureImporter)assetImporter;
            ti.textureType = TextureImporterType.Default;
            ti.textureShape = TextureImporterShape.Texture2D;
            ti.sRGBTexture = true;
            ti.alphaSource = TextureImporterAlphaSource.FromInput;
            ti.alphaIsTransparency = true;
            ti.mipmapEnabled = false;
            ti.npotScale = TextureImporterNPOTScale.None;
            ti.wrapMode = TextureWrapMode.Clamp;
            ti.filterMode = FilterMode.Bilinear;
            ti.maxTextureSize = 4096;
            ti.textureCompression = TextureImporterCompression.CompressedHQ;
            foreach (var platform in new[] { "Android", "iPhone" })
                ti.SetPlatformTextureSettings(new TextureImporterPlatformSettings
                {
                    name = platform, overridden = true, maxTextureSize = 2048,
                    format = TextureImporterFormat.ASTC_6x6, compressionQuality = 50,
                });
        }

        static void OnPostprocessAllAssets(string[] imported, string[] deleted, string[] moved, string[] movedFrom)
        {
            var sidecars = new List<string>();
            foreach (var p in imported)
                if (p.EndsWith(SidecarSuffix, System.StringComparison.OrdinalIgnoreCase)) sidecars.Add(p);
            if (sidecars.Count == 0) return;
            // Build after this import pass so the sheet next to the sidecar is imported with the rules above.
            EditorApplication.delayCall += () =>
            {
                foreach (var p in sidecars) SakugaAssetBuilder.Build(p, overwrite: false);
                AssetDatabase.SaveAssets();
                // and the layered <id>_fx.prefab from the recipe in the same sidecar, registered in the library
                var sh = SakugaFxBuilder.Shared();
                var lib = SakugaFxBuilder.LoadOrCreateLibrary();
                foreach (var p in sidecars)
                {
                    if (!p.StartsWith(SakugaFxBuilder.Root + "/Flipbooks/") || p.Contains("_1024.") || p.Contains("_add.")) continue;
                    var fx = SakugaFxBuilder.BuildFx(p, sh);
                    if (!fx) continue;
                    lib.entries.RemoveAll(e => e == null || e.id == fx.id);
                    lib.entries.Add(new SakugaLibrary.Entry
                    {
                        id = fx.id, element = fx.element, category = fx.recipe.category, style = fx.recipe.style, prefab = fx,
                        tags = fx.recipe.tags, useCases = fx.recipe.use_cases,
                    });
                }
                lib.entries.Sort((a, b) => string.CompareOrdinal(a.id, b.id));
                EditorUtility.SetDirty(lib);
                AssetDatabase.SaveAssets();
            };
        }
    }

    /// <summary>Builds M_&lt;id&gt;.mat, M_&lt;id&gt;_Particles.mat, &lt;id&gt;_Quad.asset and &lt;id&gt;.prefab beside a sidecar.</summary>
    public static class SakugaAssetBuilder
    {
        public const string ShaderName = "KanaWorks/SakugaFlipbook";

        public static IEnumerable<string> AllSidecars()
        {
            foreach (var guid in AssetDatabase.FindAssets("t:TextAsset", new[] { "Assets" }))
            {
                var p = AssetDatabase.GUIDToAssetPath(guid);
                if (p.EndsWith(SakugaImporter.SidecarSuffix, System.StringComparison.OrdinalIgnoreCase)) yield return p;
            }
        }

        public static GameObject Build(string sidecarPath, bool overwrite)
        {
            SakugaFlipbookInfo info;
            try { info = SakugaFlipbookInfo.FromJson(File.ReadAllText(sidecarPath)); }
            catch (System.Exception e) { Debug.LogWarning("[SAKUGA] " + sidecarPath + ": " + e.Message); return null; }
            string dir = Path.GetDirectoryName(sidecarPath).Replace('\\', '/');
            string stem = Path.GetFileName(sidecarPath);
            stem = stem.Substring(0, stem.Length - SakugaImporter.SidecarSuffix.Length);
            string id = stem;                                // file stem, so variants (_1024, _add) get their own assets
            string texPath = dir + "/" + (string.IsNullOrEmpty(info.source) ? stem + ".png" : info.source);
            var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(texPath);
            if (!tex) { Debug.LogWarning("[SAKUGA] sheet not found: " + texPath); return null; }
            string prefabPath = dir + "/" + id + ".prefab";
            if (!overwrite && File.Exists(prefabPath)) return AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            var shader = Shader.Find(ShaderName);
            if (!shader) { Debug.LogError("[SAKUGA] shader " + ShaderName + " not found: is the package installed and URP active?"); return null; }

            var recipe = SakugaFlipbookInfo.RecipeFromJson(File.ReadAllText(sidecarPath));
            var mat = Mat(dir + "/M_" + id + ".mat", shader, m =>
            {
                m.SetTexture("_MainTex", tex);
                m.SetVector("_Grid", new Vector4(info.grid[0], info.grid[1], info.frames, 0));
                m.SetFloat("_Frame", 0);
                m.SetFloat("_Additive", info.Additive ? 1 : 0);
                m.SetFloat("_Billboard", string.IsNullOrEmpty(info.orientation) ? (info.GroundRooted ? 2 : 1)
                                                                                 : SakugaFxBuilder.BillboardMode(info.orientation));
                // soft edge where it meets the floor or a body; flat ground sheets lie on the floor and must not fade
                m.SetFloat("_SoftFade", info.orientation == "ground" ? 0f : 0.25f);
                // hits float a little toward the camera so the target's body does not cut them
                m.SetFloat("_CamPush", recipe.category == "impact" || recipe.category == "slash" ? 0.45f
                                     : recipe.category == "shield" || recipe.category == "aura" || recipe.category == "buff" ? 0.6f : 0f);
                // light and white-cored sheets are bright already: a small boost, or Bloom turns them into a white blob
                m.SetFloat("_CoreBoost", info.element == "light" ? 0.15f : info.element == "ice" || info.element == "wind" ? 0.3f : 0.6f);
                m.SetFloat("_CoreThreshold", 0.8f);
            });
            Mat(dir + "/M_" + id + "_Particles.mat", shader, m =>
            {
                m.SetTexture("_MainTex", tex);
                m.SetVector("_Grid", new Vector4(info.grid[0], info.grid[1], info.frames, 0));
                m.SetFloat("_Frame", -1);                    // Particle System Texture Sheet Animation drives the UV
                m.SetFloat("_Additive", info.Additive ? 1 : 0);
                m.SetFloat("_Billboard", 0);                 // particles are billboards already
            });
            var mesh = Quad(dir + "/" + id + "_Quad.asset", info);

            var go = new GameObject(id);
            try
            {
                go.AddComponent<MeshFilter>().sharedMesh = mesh;
                var r = go.AddComponent<MeshRenderer>();
                r.sharedMaterial = mat;
                r.shadowCastingMode = ShadowCastingMode.Off;
                r.receiveShadows = false;
                r.lightProbeUsage = LightProbeUsage.Off;
                r.reflectionProbeUsage = ReflectionProbeUsage.Off;
                r.motionVectorGenerationMode = MotionVectorGenerationMode.ForceNoMotion;
                var fb = go.AddComponent<SakugaFlipbook>();
                fb.grid = new Vector2Int(info.grid[0], info.grid[1]);
                fb.frames = info.frames;
                fb.fps = info.fps;
                fb.loop = info.loop;
                fb.previewFrame = Mathf.Min(info.frames - 1, info.frames / 4);
                return PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
            }
            finally { Object.DestroyImmediate(go); }
        }

        /// <summary>Create or refresh a material. Properties are set before CreateAsset, and an existing asset keeps its GUID.</summary>
        static Material Mat(string path, Shader shader, System.Action<Material> fill)
        {
            var m = AssetDatabase.LoadAssetAtPath<Material>(path);
            bool fresh = !m;
            if (fresh) m = new Material(shader); else m.shader = shader;
            fill(m);
            if (fresh) AssetDatabase.CreateAsset(m, path); else EditorUtility.SetDirty(m);
            return m;
        }

        /// <summary>A quad one cell in size (256 px per unit) with the pivot at the origin. Bounds cover any rotation,
        /// because the shader turns the quad toward the camera around the pivot.</summary>
        static Mesh Quad(string path, SakugaFlipbookInfo info)
        {
            var mesh = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            bool fresh = !mesh;
            if (fresh) mesh = new Mesh();
            mesh.Clear();
            mesh.name = Path.GetFileNameWithoutExtension(path);
            Vector2 size = info.WorldSize, p = info.PivotUV;
            float x0 = -p.x * size.x, x1 = (1 - p.x) * size.x, y0 = -p.y * size.y, y1 = (1 - p.y) * size.y;
            mesh.vertices = new[] { new Vector3(x0, y0, 0), new Vector3(x1, y0, 0), new Vector3(x0, y1, 0), new Vector3(x1, y1, 0) };
            mesh.uv = new[] { new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, 1), new Vector2(1, 1) };
            mesh.normals = new[] { Vector3.back, Vector3.back, Vector3.back, Vector3.back };
            mesh.triangles = new[] { 0, 2, 1, 2, 3, 1 };
            float reach = Mathf.Sqrt(Mathf.Max(x0 * x0, x1 * x1) + Mathf.Max(y0 * y0, y1 * y1));   // farthest corner
            mesh.bounds = new Bounds(Vector3.zero, Vector3.one * (2f * reach));
            if (fresh) AssetDatabase.CreateAsset(mesh, path); else EditorUtility.SetDirty(mesh);
            return mesh;
        }
    }
}
