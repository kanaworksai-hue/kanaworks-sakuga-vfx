using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace KanaWorks.SakugaVFX.EditorTools
{
    public static class SakugaMenu
    {
        const string Menu = "Tools/KanaWorks SAKUGA VFX/";
        public const string DemoScene = "Assets/KanaWorksSakugaVFX/Demo/SakugaDemo.unity";
        public const string Homepage = "https://github.com/kanaworksai-hue/kanaworks-sakuga-vfx";

        [MenuItem(Menu + "Build materials and prefabs", priority = 1)]
        public static void BuildAll()
        {
            int n = 0;
            foreach (var p in SakugaAssetBuilder.AllSidecars().ToList())
                if (SakugaAssetBuilder.Build(p, overwrite: true)) n++;
            AssetDatabase.SaveAssets();
            Debug.Log("[SAKUGA] built " + n + " flipbook prefab(s)");
        }

        [MenuItem(Menu + "Build demo scene", priority = 2)]
        public static void BuildDemo()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            var prefabs = new List<GameObject>();
            foreach (var p in SakugaAssetBuilder.AllSidecars().OrderBy(p => p))
            {
                var stem = Path.GetFileName(p);
                if (stem.Contains("_1024.") || stem.Contains("_add.")) continue;   // one copy of each effect
                var prefab = SakugaAssetBuilder.Build(p, overwrite: false);
                if (prefab) prefabs.Add(prefab);
            }
            AssetDatabase.SaveAssets();
            if (prefabs.Count == 0)
            {
                Debug.LogWarning("[SAKUGA] no flipbooks found: copy Assets/KanaWorksSakugaVFX/Flipbooks from the download into this project");
                return;
            }

            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            var cam = Camera.main;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.08f, 0.085f, 0.11f);

            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            float spacing = 1.6f, width = prefabs.Count * spacing;
            ground.transform.localScale = new Vector3(width / 10f + 0.4f, 1, 0.6f);

            var root = new GameObject("SAKUGA VFX demo");
            root.AddComponent<SakugaDemoReplay>();
            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            for (int i = 0; i < prefabs.Count; i++)
            {
                var go = (GameObject)PrefabUtility.InstantiatePrefab(prefabs[i], root.transform);
                var fb = go.GetComponent<SakugaFlipbook>();
                bool grounded = fb.GetComponent<Renderer>().sharedMaterial.GetFloat("_Billboard") > 1.5f;   // upright = ground-rooted
                float x = (i - (prefabs.Count - 1) * 0.5f) * spacing;
                go.transform.position = new Vector3(x, grounded ? 0f : 0.9f, 0f);
                if (font)
                {
                    var label = new GameObject("Label");
                    label.transform.SetParent(root.transform, false);
                    label.transform.position = new Vector3(x, 0.02f, -0.75f);
                    label.transform.rotation = Quaternion.Euler(90, 0, 0);
                    var tm = label.AddComponent<TextMesh>();
                    tm.text = prefabs[i].name;
                    tm.font = font;
                    tm.fontSize = 48;
                    tm.characterSize = 0.035f;
                    tm.anchor = TextAnchor.MiddleCenter;
                    tm.color = new Color(0.8f, 0.82f, 0.9f);
                    label.GetComponent<MeshRenderer>().sharedMaterial = font.material;
                }
            }
            float vfov = cam.fieldOfView * Mathf.Deg2Rad;
            float hfov = 2f * Mathf.Atan(Mathf.Tan(vfov / 2f) * (16f / 9f));
            float dist = Mathf.Max(5f, (width * 0.5f + 0.5f) / Mathf.Tan(hfov / 2f));
            cam.transform.position = new Vector3(0, 1.4f, -dist);
            cam.transform.rotation = Quaternion.Euler(6f, 0, 0);

            Directory.CreateDirectory(Path.GetDirectoryName(DemoScene));
            EditorSceneManager.SaveScene(scene, DemoScene);
            Debug.Log("[SAKUGA] demo scene saved to " + DemoScene + ": press Play");
        }

        [MenuItem(Menu + "Credit and license", priority = 20)]
        public static void OpenHomepage() => Application.OpenURL(Homepage);
    }
}
