using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace KanaWorks.SakugaVFX.EditorTools
{
    /// <summary>
    /// Builds &lt;id&gt;_fx.prefab (a SakugaEffect with every layer of the recipe) next to each flipbook, the shared
    /// layer materials from Assets/KanaWorksSakugaVFX/Shared, and the SakugaLibrary that SakugaFX looks ids up in.
    /// </summary>
    public static class SakugaFxBuilder
    {
        public const string Root = "Assets/KanaWorksSakugaVFX";
        public const string SharedDir = Root + "/Shared";
        public const string MaterialsDir = SharedDir + "/Materials";
        public const string LibraryPath = Root + "/Resources/SakugaLibrary.asset";
        public const string DecalShaderName = "Shader Graphs/Decal";

        public class SharedSet
        {
            public Material flash, glow, ring, spark, debris, smoke, trail;
            public readonly Dictionary<string, Material> decalQuad = new Dictionary<string, Material>();
            public readonly Dictionary<string, Material> decalProjector = new Dictionary<string, Material>();
            public readonly Dictionary<string, AudioClip> sfx = new Dictionary<string, AudioClip>();
        }

        static SharedSet cached;

        /// <summary>Shared materials (created or refreshed) and sound clips.</summary>
        public static SharedSet Shared(bool refresh = false)
        {
            if (cached != null && !refresh && cached.flash) return cached;
            var s = new SharedSet();
            var shader = Shader.Find(SakugaAssetBuilder.ShaderName);
            if (!shader) { Debug.LogError("[SAKUGA] shader " + SakugaAssetBuilder.ShaderName + " not found"); return s; }
            Directory.CreateDirectory(MaterialsDir);
            Texture2D Tex(string n) => AssetDatabase.LoadAssetAtPath<Texture2D>(SharedDir + "/textures/" + n + ".png");
            // additive amount: sparks stay mostly alpha-blended so they read on bright floors and white targets
            s.flash = Particle("M_Sakuga_Flash", shader, Tex("T_Flash"), 1f, 2.6f, 0f);
            s.glow = Particle("M_Sakuga_Glow", shader, Tex("T_Glow"), 1f, 1.1f, 0f);
            s.ring = Particle("M_Sakuga_Ring", shader, Tex("T_Ring"), 0.5f, 1.15f, 0f);
            s.spark = Particle("M_Sakuga_Spark", shader, Tex("T_Spark"), 0.35f, 2.2f, 0f);
            s.debris = Particle("M_Sakuga_Debris", shader, Tex("T_Debris_2x2"), 0f, 1f, 0f);
            s.smoke = Particle("M_Sakuga_Smoke", shader, Tex("T_Smoke_2x2"), 0f, 1f, 0.5f);
            s.trail = Particle("M_Sakuga_Trail", shader, Tex("T_Glow"), 0.7f, 1.6f, 0.2f);
            var decalShader = Shader.Find(DecalShaderName);
            foreach (var el in new[] { "water", "fire", "thunder", "wind", "poison", "ice", "rock", "light", "dark" })
            {
                var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(SharedDir + "/decals/decal_" + el + ".png");
                if (!tex) continue;
                s.decalQuad[el] = Mat(MaterialsDir + "/M_Decal_" + el + ".mat", shader, m =>
                {
                    m.SetTexture("_MainTex", tex); m.SetFloat("_Frame", -1); m.SetFloat("_Billboard", 0); m.SetFloat("_Additive", 0);
                    m.SetFloat("_SoftFade", 0); m.SetColor("_Tint", Color.white);
                    m.renderQueue = 2990;                                   // before other transparents: it lies on the floor
                });
                if (decalShader)
                    s.decalProjector[el] = Mat(MaterialsDir + "/M_DecalProjector_" + el + ".mat", decalShader, m =>
                    {
                        if (m.HasProperty("Base_Map")) m.SetTexture("Base_Map", tex);
                        if (m.HasProperty("_BaseMap")) m.SetTexture("_BaseMap", tex);
                    });
            }
            foreach (var guid in AssetDatabase.FindAssets("t:AudioClip", new[] { SharedDir }))
            {
                var p = AssetDatabase.GUIDToAssetPath(guid);
                var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(p);
                if (clip) s.sfx[Path.GetFileName(p)] = clip;
            }
            cached = s;
            return s;
        }

        static Material Particle(string name, Shader shader, Texture2D tex, float additive, float hdr, float softFade)
        {
            return Mat(MaterialsDir + "/" + name + ".mat", shader, m =>
            {
                m.SetTexture("_MainTex", tex);
                m.SetFloat("_Frame", -1);                       // particles: mesh UV (texture sheet animation)
                m.SetFloat("_Billboard", 0);
                m.SetFloat("_Additive", additive);
                m.SetColor("_Tint", new Color(hdr, hdr, hdr, 1));
                m.SetFloat("_SoftFade", softFade);
                m.enableInstancing = false;
            });
        }

        public static Material Mat(string path, Shader shader, System.Action<Material> fill)
        {
            var m = AssetDatabase.LoadAssetAtPath<Material>(path);
            bool fresh = !m;
            if (fresh) m = new Material(shader); else m.shader = shader;
            fill(m);
            if (fresh) AssetDatabase.CreateAsset(m, path); else EditorUtility.SetDirty(m);
            return m;
        }

        /// <summary>Billboard mode for a recipe orientation (shader _Billboard).</summary>
        public static int BillboardMode(string orientation)
        {
            switch (orientation)
            {
                case "ground": return 0;
                case "upright": return 2;
                case "travel": return 3;
                default: return 1;
            }
        }

        /// <summary>Build &lt;id&gt;_fx.prefab next to a sidecar (the simple prefab and material must exist).</summary>
        public static SakugaEffect BuildFx(string sidecarPath, SharedSet sh)
        {
            string json = File.ReadAllText(sidecarPath);
            SakugaFlipbookInfo info;
            try { info = SakugaFlipbookInfo.FromJson(json); }
            catch (System.Exception e) { Debug.LogWarning("[SAKUGA] " + sidecarPath + ": " + e.Message); return null; }
            var recipe = SakugaFlipbookInfo.RecipeFromJson(json);
            string dir = Path.GetDirectoryName(sidecarPath).Replace('\\', '/');
            string stem = Path.GetFileName(sidecarPath);
            stem = stem.Substring(0, stem.Length - SakugaImporter.SidecarSuffix.Length);
            var simple = SakugaAssetBuilder.Build(sidecarPath, overwrite: true);
            if (!simple) return null;
            var mat = AssetDatabase.LoadAssetAtPath<Material>(dir + "/M_" + stem + ".mat");
            var mesh = AssetDatabase.LoadAssetAtPath<Mesh>(dir + "/" + stem + "_Quad.asset");

            var go = new GameObject(stem + "_fx");
            try
            {
                var fx = go.AddComponent<SakugaEffect>();
                fx.id = stem;
                fx.element = info.element;
                fx.fps = Mathf.RoundToInt(info.fps);
                fx.frames = info.frames;
                fx.loop = info.loop;
                fx.cell = new Vector2(info.cell[0], info.cell[1]);
                fx.pivot = info.pivot != null && info.pivot.Length == 2 ? new Vector2(info.pivot[0], info.pivot[1]) : new Vector2(0.5f, 0.5f);
                fx.bottomInset = info.bottom_inset;
                fx.recipe = recipe;

                var mainGo = new GameObject("Main");
                mainGo.transform.SetParent(go.transform, false);
                mainGo.AddComponent<MeshFilter>().sharedMesh = mesh;
                var r = mainGo.AddComponent<MeshRenderer>();
                r.sharedMaterial = mat;
                r.shadowCastingMode = ShadowCastingMode.Off;
                r.receiveShadows = false;
                r.lightProbeUsage = LightProbeUsage.Off;
                r.reflectionProbeUsage = ReflectionProbeUsage.Off;
                var fb = mainGo.AddComponent<SakugaFlipbook>();
                fb.grid = new Vector2Int(info.grid[0], info.grid[1]);
                fb.frames = info.frames;
                fb.fps = info.fps;
                fb.loop = info.loop;
                fb.playOnEnable = false;
                fb.externallyDriven = true;
                fx.main = fb;
                fx.LayoutMain();

                fx.flashMaterial = sh.flash; fx.glowMaterial = sh.glow; fx.ringMaterial = sh.ring; fx.sparkMaterial = sh.spark;
                fx.debrisMaterial = sh.debris; fx.smokeMaterial = sh.smoke; fx.trailMaterial = sh.trail;
                if (info.element != null && sh.decalQuad.TryGetValue(info.element, out var dq)) fx.decalMaterial = dq;
                if (info.element != null && sh.decalProjector.TryGetValue(info.element, out var dp)) fx.decalProjectorMaterial = dp;
                fx.castClip = Clip(sh, recipe.fx.sfx.cast);
                fx.loopClip = Clip(sh, recipe.fx.sfx.loop);
                fx.impactClip = Clip(sh, recipe.fx.sfx.impact);
                if (recipe.IsProjectile) go.AddComponent<SakugaProjectile>();
                fx.Build();
                fb.ShowFrame(Mathf.Min(info.frames - 1, Mathf.Max(recipe.contact_frame, info.frames / 4)));
                var saved = PrefabUtility.SaveAsPrefabAsset(go, dir + "/" + stem + "_fx.prefab");
                return saved ? saved.GetComponent<SakugaEffect>() : null;
            }
            finally { Object.DestroyImmediate(go); }
        }

        static AudioClip Clip(SharedSet sh, string name) => !string.IsNullOrEmpty(name) && sh.sfx.TryGetValue(name, out var c) ? c : null;

        /// <summary>Build every simple and layered prefab, then the library. Returns the number of effects.</summary>
        public static int BuildAll()
        {
            AssetDatabase.Refresh();
            var sh = Shared(refresh: true);
            var lib = LoadOrCreateLibrary();
            lib.entries.Clear();
            int n = 0;
            var sidecars = new List<string>(SakugaAssetBuilder.AllSidecars());
            sidecars.Sort(System.StringComparer.Ordinal);
            try
            {
                for (int i = 0; i < sidecars.Count; i++)
                {
                    var p = sidecars[i];
                    if (!p.StartsWith(Root + "/Flipbooks/")) continue;
                    var stem = Path.GetFileName(p);
                    if (stem.Contains("_1024.") || stem.Contains("_add.")) continue;
                    if (Application.isBatchMode == false)
                        EditorUtility.DisplayProgressBar("KanaWorks SAKUGA VFX", stem, i / (float)sidecars.Count);
                    var fx = BuildFx(p, sh);
                    if (!fx) continue;
                    lib.entries.Add(new SakugaLibrary.Entry
                    {
                        id = fx.id, element = fx.element, category = fx.recipe.category, style = fx.recipe.style, prefab = fx,
                        tags = fx.recipe.tags, useCases = fx.recipe.use_cases,
                    });
                    n++;
                }
            }
            finally { if (!Application.isBatchMode) EditorUtility.ClearProgressBar(); }
            EditorUtility.SetDirty(lib);
            AssetDatabase.SaveAssets();
            SakugaFX.Library = lib;
            Debug.Log("[SAKUGA] built " + n + " layered effect prefab(s) and " + LibraryPath);
            return n;
        }

        public static SakugaLibrary LoadOrCreateLibrary()
        {
            var lib = AssetDatabase.LoadAssetAtPath<SakugaLibrary>(LibraryPath);
            if (lib) return lib;
            Directory.CreateDirectory(Path.GetDirectoryName(LibraryPath));
            lib = ScriptableObject.CreateInstance<SakugaLibrary>();
            AssetDatabase.CreateAsset(lib, LibraryPath);
            return lib;
        }
    }

    /// <summary>Import rules for the shared layer textures, decals and sounds.</summary>
    public class SakugaSharedImporter : AssetPostprocessor
    {
        static bool IsShared(string p) => p.Replace('\\', '/').Contains("/KanaWorksSakugaVFX/Shared/");

        void OnPreprocessTexture()
        {
            if (!IsShared(assetPath)) return;
            var ti = (TextureImporter)assetImporter;
            ti.textureType = TextureImporterType.Default;
            ti.sRGBTexture = true;
            ti.alphaSource = TextureImporterAlphaSource.FromInput;
            ti.alphaIsTransparency = true;
            ti.mipmapEnabled = true;
            ti.wrapMode = TextureWrapMode.Clamp;
            ti.npotScale = TextureImporterNPOTScale.None;
            ti.maxTextureSize = 1024;
            ti.textureCompression = TextureImporterCompression.CompressedHQ;
        }

        void OnPreprocessAudio()
        {
            if (!IsShared(assetPath)) return;
            var ai = (AudioImporter)assetImporter;
            ai.forceToMono = true;
            var s = ai.defaultSampleSettings;
            s.loadType = assetPath.Contains("_loop") ? AudioClipLoadType.CompressedInMemory : AudioClipLoadType.DecompressOnLoad;
            s.compressionFormat = AudioCompressionFormat.Vorbis;
            s.quality = 0.6f;
            ai.defaultSampleSettings = s;
        }
    }
}
