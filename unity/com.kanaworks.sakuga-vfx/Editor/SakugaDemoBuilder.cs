using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using KanaWorks.SakugaVFX.Demo;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace KanaWorks.SakugaVFX.EditorTools
{
    /// <summary>
    /// Builds the four demo scenes from Unity primitives (capsule robots, sphere slimes, cube golems, cylinder towers)
    /// with a small cel shader: Arena (action RPG casts), TopDown (shooter bullet patterns), TowerDefense and Gallery
    /// (every effect, arrow keys). Each scene has a SakugaDemoDirector (or SakugaGallery) that plays live and can be
    /// rendered offline at any time.
    /// </summary>
    public static class SakugaDemoBuilder
    {
        public const string Dir = SakugaFxBuilder.Root + "/Demo";
        public const string Arena = Dir + "/Sakuga_Arena.unity";
        public const string TopDown = Dir + "/Sakuga_TopDown.unity";
        public const string TowerDefense = Dir + "/Sakuga_TowerDefense.unity";
        public const string Gallery = Dir + "/Sakuga_Gallery.unity";
        public static readonly string[] All = { Arena, TopDown, TowerDefense, Gallery };
        const int CharacterLayer = 2;                      // Ignore Raycast: ground decals skip characters

        static readonly Dictionary<string, Material> mats = new Dictionary<string, Material>();

        // ------------------------------------------------------------------ materials and primitives
        static Material Toon(string name, Color baseCol, Color shade, float checker = 0f, float rim = 0.25f)
        {
            if (mats.TryGetValue(name, out var m) && m) return m;
            var shader = Shader.Find("KanaWorks/SakugaToonLit");
            Directory.CreateDirectory(Dir + "/Materials");
            m = SakugaFxBuilder.Mat(Dir + "/Materials/M_Demo_" + name + ".mat", shader, x =>
            {
                x.SetColor("_BaseColor", baseCol);
                x.SetColor("_ShadeColor", shade);
                x.SetColor("_RimColor", new Color(1, 1, 1, rim));
                x.SetFloat("_Checker", checker);
                x.SetFloat("_CheckerSize", 1.5f);
            });
            mats[name] = m;
            return m;
        }

        static GameObject Prim(PrimitiveType type, string name, Transform parent, Vector3 pos, Vector3 scale, Material mat,
                               bool collider = true, int layer = 0)
        {
            var go = GameObject.CreatePrimitive(type);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = pos;
            go.transform.localScale = scale;
            go.GetComponent<Renderer>().sharedMaterial = mat;
            if (!collider) Object.DestroyImmediate(go.GetComponent<Collider>());
            go.layer = layer;
            return go;
        }

        static Material Dark => Toon("Ink", new Color(0.08f, 0.08f, 0.12f), new Color(0.6f, 0.6f, 0.7f), 0f, 0f);

        public static SakugaDemoTarget Robot(string name, Vector3 at, float yaw, Color suit)
        {
            var root = new GameObject(name).transform;
            root.position = at; root.rotation = Quaternion.Euler(0, yaw, 0);
            var body = new GameObject("Body").transform; body.SetParent(root, false);
            var m = Toon("Robot_" + ColorUtility.ToHtmlStringRGB(suit), suit, new Color(0.55f, 0.58f, 0.78f));
            var white = Toon("RobotWhite", new Color(0.92f, 0.93f, 0.97f), new Color(0.6f, 0.62f, 0.8f));
            Prim(PrimitiveType.Capsule, "Torso", body, new Vector3(0, 0.95f, 0), new Vector3(0.75f, 0.62f, 0.6f), m, true, CharacterLayer);
            Prim(PrimitiveType.Sphere, "Head", body, new Vector3(0, 1.82f, 0), Vector3.one * 0.6f, white, false, CharacterLayer);
            Prim(PrimitiveType.Cube, "Visor", body, new Vector3(0, 1.84f, 0.25f), new Vector3(0.42f, 0.14f, 0.12f), Dark, false, CharacterLayer);
            Prim(PrimitiveType.Cylinder, "ArmL", body, new Vector3(-0.5f, 1.05f, 0.05f), new Vector3(0.18f, 0.32f, 0.18f), white, false, CharacterLayer);
            Prim(PrimitiveType.Cylinder, "ArmR", body, new Vector3(0.5f, 1.15f, 0.25f), new Vector3(0.18f, 0.32f, 0.18f), white, false, CharacterLayer)
                .transform.localRotation = Quaternion.Euler(70, 0, 0);
            Prim(PrimitiveType.Cube, "Pack", body, new Vector3(0, 1.1f, -0.36f), new Vector3(0.45f, 0.5f, 0.2f), m, false, CharacterLayer);
            Prim(PrimitiveType.Sphere, "Antenna", body, new Vector3(0.18f, 2.2f, 0), Vector3.one * 0.1f, Toon("Antenna", new Color(1f, 0.4f, 0.6f), new Color(0.8f, 0.5f, 0.6f)), false, CharacterLayer);
            var t = root.gameObject.AddComponent<SakugaDemoTarget>();
            t.body = body; t.bob = 0.02f;
            return t;
        }

        public static SakugaDemoTarget Slime(string name, Vector3 at, Color c, float size = 1f)
        {
            var root = new GameObject(name).transform;
            root.position = at;
            var body = new GameObject("Body").transform; body.SetParent(root, false);
            var m = Toon("Slime_" + ColorUtility.ToHtmlStringRGB(c), c, Color.Lerp(c, new Color(0.3f, 0.3f, 0.6f), 0.5f) * 0.9f + new Color(0.2f, 0.2f, 0.3f), 0f, 0.45f);
            Prim(PrimitiveType.Sphere, "Jelly", body, new Vector3(0, 0.42f * size, 0), new Vector3(1.1f, 0.85f, 1.1f) * size, m, true, CharacterLayer);
            Prim(PrimitiveType.Sphere, "EyeL", body, new Vector3(-0.18f, 0.55f, 0.47f) * size, Vector3.one * 0.13f * size, Dark, false, CharacterLayer);
            Prim(PrimitiveType.Sphere, "EyeR", body, new Vector3(0.18f, 0.55f, 0.47f) * size, Vector3.one * 0.13f * size, Dark, false, CharacterLayer);
            var t = root.gameObject.AddComponent<SakugaDemoTarget>();
            t.body = body; t.bob = 0.06f; t.phase = at.x * 1.7f + at.z;
            return t;
        }

        public static SakugaDemoTarget Golem(string name, Vector3 at, float yaw)
        {
            var root = new GameObject(name).transform;
            root.position = at; root.rotation = Quaternion.Euler(0, yaw, 0);
            var body = new GameObject("Body").transform; body.SetParent(root, false);
            var stone = Toon("Golem", new Color(0.62f, 0.56f, 0.5f), new Color(0.5f, 0.5f, 0.66f));
            var moss = Toon("GolemMoss", new Color(0.45f, 0.62f, 0.4f), new Color(0.5f, 0.55f, 0.66f));
            Prim(PrimitiveType.Cube, "Legs", body, new Vector3(0, 0.45f, 0), new Vector3(1.1f, 0.9f, 0.8f), stone, true, CharacterLayer);
            Prim(PrimitiveType.Cube, "Chest", body, new Vector3(0, 1.45f, 0), new Vector3(1.7f, 1.1f, 1.1f), stone, true, CharacterLayer);
            Prim(PrimitiveType.Cube, "Head", body, new Vector3(0, 2.25f, 0.1f), new Vector3(0.7f, 0.55f, 0.7f), moss, false, CharacterLayer);
            Prim(PrimitiveType.Cube, "Eye", body, new Vector3(0, 2.27f, 0.46f), new Vector3(0.45f, 0.1f, 0.05f), Toon("GolemEye", new Color(1f, 0.85f, 0.3f), new Color(1, 0.9f, 0.6f), 0f, 0.6f), false, CharacterLayer);
            Prim(PrimitiveType.Cube, "ArmL", body, new Vector3(-1.1f, 1.2f, 0), new Vector3(0.5f, 1.4f, 0.55f), stone, false, CharacterLayer);
            Prim(PrimitiveType.Cube, "ArmR", body, new Vector3(1.1f, 1.2f, 0), new Vector3(0.5f, 1.4f, 0.55f), stone, false, CharacterLayer);
            var t = root.gameObject.AddComponent<SakugaDemoTarget>();
            t.body = body; t.bob = 0.015f;
            return t;
        }

        public static Transform Tower(string name, Vector3 at, Color orb)
        {
            var root = new GameObject(name).transform;
            root.position = at;
            var stone = Toon("Tower", new Color(0.82f, 0.8f, 0.76f), new Color(0.55f, 0.56f, 0.72f));
            var roof = Toon("TowerRoof", new Color(0.36f, 0.42f, 0.66f), new Color(0.5f, 0.52f, 0.72f));
            Prim(PrimitiveType.Cylinder, "Base", root, new Vector3(0, 0.25f, 0), new Vector3(1.4f, 0.25f, 1.4f), stone, true, CharacterLayer);
            Prim(PrimitiveType.Cylinder, "Shaft", root, new Vector3(0, 1.2f, 0), new Vector3(0.9f, 0.8f, 0.9f), stone, true, CharacterLayer);
            Prim(PrimitiveType.Cube, "Top", root, new Vector3(0, 2.15f, 0), new Vector3(1.2f, 0.3f, 1.2f), roof, false, CharacterLayer);
            Prim(PrimitiveType.Sphere, "Orb", root, new Vector3(0, 2.65f, 0), Vector3.one * 0.5f,
                 Toon("Orb_" + ColorUtility.ToHtmlStringRGB(orb), orb, Color.Lerp(orb, Color.white, 0.5f), 0f, 0.8f), false, CharacterLayer);
            return root;
        }

        // ------------------------------------------------------------------ scene shell
        static Camera Shell(string title, Vector3 camPos, Vector3 lookAt, float fov, Color sky, Color ground, float floorRadius)
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            mats.Clear();
            var camGo = new GameObject("Main Camera") { tag = "MainCamera" };
            var cam = camGo.AddComponent<Camera>();
            cam.transform.position = camPos;
            cam.transform.LookAt(lookAt);
            cam.fieldOfView = fov;
            cam.nearClipPlane = 0.1f; cam.farClipPlane = 200f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = sky;
            cam.allowHDR = true;
            camGo.AddComponent<AudioListener>();
            camGo.AddComponent<SakugaCameraShake>();
            EnablePostProcessing(cam);

            var sun = new GameObject("Sun").AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.color = new Color(1f, 0.93f, 0.84f);
            sun.intensity = 1.25f;
            sun.shadows = LightShadows.Soft;
            sun.shadowStrength = 0.65f;
            sun.transform.rotation = Quaternion.Euler(52f, -35f, 0f);

            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = Color.Lerp(sky, Color.white, 0.2f) * 0.85f;
            RenderSettings.ambientEquatorColor = Color.Lerp(sky, ground, 0.5f) * 0.7f;
            RenderSettings.ambientGroundColor = ground * 0.35f;
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogColor = sky;
            RenderSettings.fogStartDistance = 28f; RenderSettings.fogEndDistance = 70f;

            var floor = Prim(PrimitiveType.Cylinder, "Floor", null, new Vector3(0, -0.05f, 0), new Vector3(floorRadius * 2, 0.05f, floorRadius * 2),
                             Toon("Floor_" + title, ground, new Color(0.5f, 0.52f, 0.72f), 0.08f, 0f));
            floor.isStatic = true;
            AddVolume(title);
            return cam;
        }

        static TextMesh Caption(Camera cam, string text, float size = 0.03f)
        {
            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var go = new GameObject("Caption");
            go.transform.SetParent(cam.transform, false);
            go.transform.localPosition = new Vector3(0f, -0.52f, 1.6f);
            var tm = go.AddComponent<TextMesh>();
            tm.text = text; tm.font = font; tm.fontSize = 48; tm.characterSize = size * 0.5f;
            tm.anchor = TextAnchor.LowerCenter; tm.alignment = TextAlignment.Center;
            tm.color = new Color(1f, 1f, 1f, 0.85f);
            go.GetComponent<MeshRenderer>().sharedMaterial = font.material;
            return tm;
        }

        // URP camera data and a Bloom / tonemapping volume, through reflection so the editor assembly has no
        // compile-time URP reference.
        static void EnablePostProcessing(Camera cam)
        {
            var t = Type.GetType("UnityEngine.Rendering.Universal.UniversalAdditionalCameraData, Unity.RenderPipelines.Universal.Runtime");
            if (t == null) return;
            var data = cam.GetComponent(t);
            if (!data) data = cam.gameObject.AddComponent(t);
            SakugaFX.SetMember(data, "renderPostProcessing", true);
            var aa = t.GetProperty("antialiasing");
            if (aa != null) SakugaFX.SetMember(data, "antialiasing", Enum.ToObject(aa.PropertyType, 1));   // FXAA
        }

        static void AddVolume(string title)
        {
            var volT = Type.GetType("UnityEngine.Rendering.Volume, Unity.RenderPipelines.Core.Runtime");
            var profT = Type.GetType("UnityEngine.Rendering.VolumeProfile, Unity.RenderPipelines.Core.Runtime");
            var bloomT = Type.GetType("UnityEngine.Rendering.Universal.Bloom, Unity.RenderPipelines.Universal.Runtime");
            var toneT = Type.GetType("UnityEngine.Rendering.Universal.Tonemapping, Unity.RenderPipelines.Universal.Runtime");
            var vigT = Type.GetType("UnityEngine.Rendering.Universal.Vignette, Unity.RenderPipelines.Universal.Runtime");
            if (volT == null || profT == null || bloomT == null) return;
            string path = Dir + "/Materials/VP_Demo.asset";
            var profile = AssetDatabase.LoadAssetAtPath(path, profT) as ScriptableObject;
            if (!profile)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                profile = ScriptableObject.CreateInstance(profT);
                AssetDatabase.CreateAsset(profile, path);
                var add = profT.GetMethod("Add", new[] { typeof(Type), typeof(bool) });
                foreach (var (type, set) in new (Type, Action<object>)[]
                {
                    (bloomT, c => { Param(c, "intensity", 0.9f); Param(c, "threshold", 1.0f); Param(c, "scatter", 0.6f); }),
                    (toneT, c => { if (toneT != null) Param(c, "mode", Enum.ToObject(toneT.Assembly.GetType("UnityEngine.Rendering.Universal.TonemappingMode"), 1)); }),
                    (vigT, c => { Param(c, "intensity", 0.22f); }),
                })
                {
                    if (type == null) continue;
                    var comp = add.Invoke(profile, new object[] { type, true }) as ScriptableObject;
                    if (!comp) continue;
                    comp.name = type.Name;
                    comp.hideFlags = HideFlags.HideInInspector | HideFlags.HideInHierarchy;
                    set(comp);
                    AssetDatabase.AddObjectToAsset(comp, profile);
                }
                EditorUtility.SetDirty(profile);
                AssetDatabase.SaveAssets();
            }
            var go = new GameObject("Post Processing");
            var vol = go.AddComponent(volT);
            SakugaFX.SetMember(vol, "isGlobal", true);
            SakugaFX.SetMember(vol, "sharedProfile", profile);
        }

        static void Param(object comp, string field, object value)
        {
            var f = comp.GetType().GetField(field);
            var p = f?.GetValue(comp);
            if (p == null) return;
            p.GetType().GetProperty("overrideState")?.SetValue(p, true);
            p.GetType().GetProperty("value")?.SetValue(p, value);
        }

        static void Props(Transform parent, int count, float radius, Material mat, float h = 2.4f)
        {
            for (int i = 0; i < count; i++)
            {
                float a = i / (float)count * Mathf.PI * 2f + 0.3f;
                var p = new Vector3(Mathf.Cos(a), 0, Mathf.Sin(a)) * radius;
                Prim(PrimitiveType.Cylinder, "Pillar", parent, p + Vector3.up * h * 0.5f, new Vector3(0.9f, h * 0.5f, 0.9f), mat).isStatic = true;
                Prim(PrimitiveType.Cube, "Cap", parent, p + Vector3.up * (h + 0.12f), new Vector3(1.2f, 0.24f, 1.2f), mat).isStatic = true;
            }
        }

        static SakugaDemoDirector Director(float period, string caption, TextMesh tm)
        {
            var d = new GameObject("SAKUGA Demo Director").AddComponent<SakugaDemoDirector>();
            d.period = period; d.captionText = caption; d.caption = tm;
            return d;
        }

        static void Save(string path)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), path);
            Debug.Log("[SAKUGA] demo scene saved: " + path);
        }

        static bool Has(string id) => SakugaFX.Library && SakugaFX.Library.Find(id);
        static string Pick(params string[] ids) => ids.FirstOrDefault(Has) ?? ids[0];

        // ------------------------------------------------------------------ (a) Arena: action RPG
        public static void BuildArena()
        {
            var cam = Shell("Arena", new Vector3(4.4f, 4.4f, -9.0f), new Vector3(0.3f, 0.9f, 3.0f), 42f,
                            new Color(0.16f, 0.18f, 0.32f), new Color(0.44f, 0.41f, 0.5f), 14f);
            var props = new GameObject("Props").transform;
            Props(props, 8, 11f, Toon("Pillar", new Color(0.78f, 0.74f, 0.7f), new Color(0.55f, 0.55f, 0.72f)), 3.2f);
            var hero = Robot("Hero", new Vector3(0, 0, -3.4f), 0f, new Color(0.35f, 0.55f, 0.95f));
            var s1 = Slime("Slime A", new Vector3(-3.2f, 0, 3.2f), new Color(0.45f, 0.85f, 0.55f));
            var s2 = Slime("Slime B", new Vector3(0.2f, 0, 4.2f), new Color(0.95f, 0.55f, 0.7f));
            var s3 = Slime("Slime C", new Vector3(3.4f, 0, 3.0f), new Color(0.6f, 0.7f, 1f));
            var golem = Golem("Golem", new Vector3(0.6f, 0, 8.5f), 180f);
            var d = Director(10f, "ARENA  action RPG: charge > projectile > impact, AoE, aura", Caption(cam, ""));
            Vector3 hand = new Vector3(0.5f, 1.45f, -2.8f);
            Vector3 Chest(SakugaDemoTarget t, float h = 0.55f) => t.transform.position + Vector3.up * h;
            d.Add(SakugaShot.Cast(0.2f, "fire_projectile_01", hand, Chest(s2)), s2);
            d.Add(SakugaShot.Cast(1.3f, Pick("ice_projectile_02", "ice_projectile_01"), hand, Chest(s1)), s1);
            d.Add(SakugaShot.At(2.3f, "thunder_strike_01", s3.transform.position), s3);
            d.Add(SakugaShot.Cast(3.3f, "dark_projectile_01", hand, Chest(s2)), s2);
            d.Add(SakugaShot.At(4.3f, "rock_column_01", golem.transform.position + Vector3.back * 1.2f), golem);
            d.Add(SakugaShot.Cast(5.3f, Pick("poison_projectile_02", "poison_projectile_01"), hand, Chest(s3), arc: 0.25f), s3);
            d.Add(SakugaShot.At(6.4f, Pick("wind_vortex_02", "wind_vortex_01"), new Vector3(-2.2f, 0, 4.6f)), s1);
            d.shots.Add(SakugaShot.Loop(7.0f, Pick("fire_aura_01", "light_column_01"), hero.transform.position, 2.4f));
            d.Add(SakugaShot.Cast(7.6f, Pick("fire_projectile_03", "fire_projectile_01"), hand + Vector3.up * 0.4f, Chest(golem, 1.4f), arc: 0.14f), golem);
            d.Add(SakugaShot.At(8.6f, Pick("water_column_02", "water_column_01"), s1.transform.position), s1);
            Save(Arena);
        }

        // ------------------------------------------------------------------ (b) TopDown shooter
        public static void BuildTopDown()
        {
            var cam = Shell("TopDown", new Vector3(0f, 16f, -9f), new Vector3(0f, 0f, 1.2f), 44f,
                            new Color(0.07f, 0.08f, 0.15f), new Color(0.24f, 0.27f, 0.38f), 16f);
            var player = Robot("Player", new Vector3(0, 0, -5f), 0f, new Color(1f, 0.6f, 0.3f));
            var boss = Golem("Boss", new Vector3(0, 0, 6.5f), 180f);
            var mobs = new List<SakugaDemoTarget>();
            for (int i = 0; i < 5; i++)
            {
                float x = -6f + i * 3f;
                mobs.Add(Slime("Mob " + i, new Vector3(x, 0, 2.5f + Mathf.Abs(x) * 0.25f), Color.HSVToRGB(0.08f + i * 0.17f, 0.45f, 0.95f), 0.8f));
            }
            var d = Director(9f, "TOP-DOWN SHOOTER  fans, rings and spirals of pooled projectiles", Caption(cam, ""));
            Vector3 gun = player.transform.position + new Vector3(0, 1.1f, 0.6f);
            // player fan volleys
            for (int v = 0; v < 2; v++)
                for (int i = 0; i < mobs.Count; i++)
                    d.Add(SakugaShot.Cast(0.2f + v * 4.2f + i * 0.05f, v == 0 ? "fire_projectile_01" : Pick("poison_projectile_03", "poison_projectile_01"),
                                          gun, mobs[i].transform.position + Vector3.up * 0.45f, speed: 16f, arc: 0f, scale: 0.75f), mobs[i]);
            // boss radial ring
            Vector3 bc = boss.transform.position + Vector3.up * 1.2f;
            for (int i = 0; i < 10; i++)
            {
                float a = (i / 10f) * Mathf.PI * 2f;
                var dir = new Vector3(Mathf.Sin(a), 0, Mathf.Cos(a));
                d.Add(SakugaShot.Cast(1.6f, Pick("dark_projectile_02", "dark_projectile_01"), bc, new Vector3(bc.x, 0.3f, bc.z) + dir * 9f, speed: 7f, arc: 0f, scale: 0.7f));
            }
            // spiral
            for (int i = 0; i < 14; i++)
            {
                float a = i * 0.55f;
                var dir = new Vector3(Mathf.Sin(a), 0, Mathf.Cos(a));
                d.Add(SakugaShot.Cast(3.0f + i * 0.08f, Pick("ice_projectile_03", "ice_projectile_01"), bc, new Vector3(bc.x, 0.3f, bc.z) + dir * 8f, speed: 8f, arc: 0f, scale: 0.6f));
            }
            d.shots.Add(SakugaShot.Loop(2.6f, Pick("water_shield_01", "water_impact_01"), player.transform.position + Vector3.up * 1.0f, 2.6f, 0.8f));
            d.Add(SakugaShot.At(5.6f, Pick("thunder_strike_02", "thunder_strike_01"), boss.transform.position + Vector3.back * 0.8f), boss);
            d.Add(SakugaShot.At(7.0f, Pick("fire_impact_03", "fire_impact_01"), mobs[2].transform.position + Vector3.up * 0.5f, 1.2f), mobs[2]);
            Save(TopDown);
        }

        // ------------------------------------------------------------------ (c) Tower defense
        public static readonly Vector3[] TdPath =
        {
            new Vector3(-11f, 0, 6f), new Vector3(-3f, 0, 6f), new Vector3(-3f, 0, 0f), new Vector3(4f, 0, 0f),
            new Vector3(4f, 0, -6f), new Vector3(12f, 0, -6f),
        };

        public static void BuildTowerDefense()
        {
            var cam = Shell("TowerDefense", new Vector3(8f, 12.5f, -13.5f), new Vector3(0.5f, 0f, 0f), 40f,
                            new Color(0.3f, 0.46f, 0.66f), new Color(0.38f, 0.56f, 0.34f), 18f);
            var road = Toon("Road", new Color(0.86f, 0.78f, 0.6f), new Color(0.62f, 0.6f, 0.72f));
            var roads = new GameObject("Road").transform;
            for (int i = 1; i < TdPath.Length; i++)
            {
                var a = TdPath[i - 1]; var b = TdPath[i];
                var mid = (a + b) * 0.5f; var len = Vector3.Distance(a, b) + 2f;
                var seg = Prim(PrimitiveType.Cube, "Road", roads, mid + Vector3.up * 0.01f, Vector3.one, road);
                seg.transform.localScale = Mathf.Abs(b.x - a.x) > 0.1f ? new Vector3(len, 0.04f, 2f) : new Vector3(2f, 0.04f, len);
                seg.isStatic = true;
            }
            const float period = 12f, speed = 2.0f;
            var walkers = new List<(SakugaDemoTarget t, float delay)>();
            for (int i = 0; i < 6; i++)
            {
                var t = Slime("Creep " + i, TdPath[0], Color.HSVToRGB(0.3f + i * 0.09f, 0.5f, 0.92f), 0.75f);
                var w = t.gameObject.AddComponent<SakugaDemoWalker>();
                w.path = TdPath; w.speed = speed; w.delay = i * 1.6f; w.period = period;
                walkers.Add((t, w.delay));
            }
            var towers = new (string proj, Vector3 at, Color c)[]
            {
                ("fire_projectile_01", new Vector3(-6f, 0, 2.6f), new Color(1f, 0.5f, 0.2f)),
                (Pick("ice_projectile_02", "ice_projectile_01"), new Vector3(0.6f, 0, 3.2f), new Color(0.5f, 0.85f, 1f)),
                ("poison_projectile_01", new Vector3(7.6f, 0, -2.6f), new Color(0.6f, 0.9f, 0.3f)),
                ("thunder_strike_01", new Vector3(0.4f, 0, -3.4f), new Color(0.75f, 0.55f, 1f)),
            };
            var d = Director(period, "TOWER DEFENSE  element towers, ground traps, sky strikes", Caption(cam, ""));
            var tws = towers.Select((tw, i) => Tower("Tower " + i, tw.at, tw.c)).ToArray();
            for (int ti = 0; ti < towers.Length; ti++)
            {
                var (proj, at, _) = towers[ti];
                var top = at + Vector3.up * 2.7f;
                for (float t = 0.3f + ti * 0.4f; t < period - 0.6f; t += 1.5f)
                {
                    // aim at the creep that will be nearest when the shot lands
                    SakugaDemoTarget best = null; float bestD = 9f; Vector3 aim = Vector3.zero;
                    foreach (var (wt, delay) in walkers)
                    {
                        float flight = proj.Contains("strike") ? 0.2f : Vector3.Distance(top, at) / 14f + 0.45f;
                        var p = SakugaDemoWalker.Evaluate(TdPath, speed, delay, period, t + flight);
                        float dist = Vector3.Distance(new Vector3(p.x, 0, p.z), at);
                        if (dist < bestD && p != TdPath[0] && p != TdPath[TdPath.Length - 1]) { bestD = dist; best = wt; aim = p; }
                    }
                    if (!best) continue;
                    if (proj.Contains("strike")) d.Add(SakugaShot.At(t, proj, aim, 0.9f), best);
                    else
                    {
                        // re-aim once with the real flight time
                        float flight = Vector3.Distance(top, aim + Vector3.up * 0.4f) / 14f;
                        var wk = best.GetComponent<SakugaDemoWalker>();
                        aim = SakugaDemoWalker.Evaluate(TdPath, speed, wk.delay, period, t + flight);
                        d.Add(SakugaShot.Cast(t, proj, top, aim + Vector3.up * 0.4f, speed: 14f, arc: 0.12f, scale: 1f), best);
                    }
                }
            }
            d.shots.Add(SakugaShot.Loop(0.0f, Pick("ice_impact_03", "ice_impact_01"), new Vector3(-3f, 0, 3f), 11f, 0.9f));
            d.shots.Add(SakugaShot.Loop(1.0f, Pick("poison_impact_02", "poison_impact_01"), new Vector3(8f, 0, -6f), 10f, 0.9f));
            Save(TowerDefense);
        }

        // ------------------------------------------------------------------ (d) Gallery
        public static void BuildGallery()
        {
            var cam = Shell("Gallery", new Vector3(0f, 3.0f, -8.4f), new Vector3(0f, 1.2f, 0f), 40f,
                            new Color(0.09f, 0.09f, 0.14f), new Color(0.3f, 0.3f, 0.38f), 9f);
            var back = Toon("Backdrop", new Color(0.26f, 0.27f, 0.36f), new Color(0.7f, 0.7f, 0.85f));
            Prim(PrimitiveType.Cube, "Backdrop", null, new Vector3(0, 3f, 4.5f), new Vector3(20f, 6f, 0.2f), back).isStatic = true;
            var dummy = Robot("Dummy", new Vector3(0, 0, 0.6f), 180f, new Color(0.85f, 0.85f, 0.9f));
            var g = new GameObject("SAKUGA Gallery").AddComponent<SakugaGallery>();
            g.dummy = dummy.transform;
            g.center = new Vector3(0, 1.1f, -0.4f);
            g.ids = SakugaFX.Library ? SakugaFX.Library.entries.OrderBy(e => Array.IndexOf(CategoryOrder, e.category)).ThenBy(e => e.id).Select(e => e.id).ToList() : new List<string>();
            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            TextMesh Label(string n, Vector3 local, TextAnchor anchor, float size, string text)
            {
                var go = new GameObject(n);
                go.transform.SetParent(cam.transform, false);
                go.transform.localPosition = local;
                var tm = go.AddComponent<TextMesh>();
                tm.font = font; tm.fontSize = 48; tm.characterSize = size; tm.anchor = anchor; tm.text = text;
                tm.color = new Color(0.95f, 0.95f, 1f, 0.9f);
                go.GetComponent<MeshRenderer>().sharedMaterial = font.material;
                return tm;
            }
            g.label = Label("Label", new Vector3(-1.2f, 0.66f, 2f), TextAnchor.UpperLeft, 0.0125f, "");
            g.help = Label("Help", new Vector3(0f, -0.68f, 2f), TextAnchor.LowerCenter, 0.009f,
                           "Left / Right: effect    Up / Down: category    Space: replay    1 2 3: size S M L");
            Save(Gallery);
        }

        static readonly string[] CategoryOrder = { "projectile", "impact", "slash", "cast", "column", "strike", "area", "vortex", "beam", "buff", "shield", "aura" };

        public static void BuildAll()
        {
            if (!SakugaFX.Library || SakugaFX.Library.entries.Count == 0) SakugaFxBuilder.BuildAll();
            BuildArena();
            BuildTopDown();
            BuildTowerDefense();
            BuildGallery();
            var scenes = EditorBuildSettings.scenes.ToList();
            foreach (var p in All)
                if (!scenes.Any(s => s.path == p)) scenes.Add(new EditorBuildSettingsScene(p, true));
            EditorBuildSettings.scenes = scenes.ToArray();
            AssetDatabase.SaveAssets();
        }
    }
}
