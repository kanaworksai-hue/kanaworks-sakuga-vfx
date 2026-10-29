using System.Collections.Generic;
using UnityEngine;

namespace KanaWorks.SakugaVFX.Demo
{
    /// <summary>
    /// Browse every effect: Left / Right (or A / D) for the previous / next effect, Up / Down jumps a category,
    /// Space replays, 1 / 2 / 3 picks size S / M / L. Projectiles fly across the stage into their paired impact; beams
    /// stretch across it; shields, auras and buffs sit on the dummy.
    /// </summary>
    [AddComponentMenu("KanaWorks/SAKUGA Gallery")]
    public class SakugaGallery : MonoBehaviour
    {
        public List<string> ids = new List<string>();
        public TextMesh label, help;
        public Transform dummy;
        public Vector3 left = new Vector3(-3.2f, 1.2f, 0f), right = new Vector3(3.2f, 1.2f, 0f), center = new Vector3(0f, 1.1f, 0f);
        public float replayPause = 0.7f;
        public int index;
        public SakugaSize size = SakugaSize.M;

        SakugaEffect current;
        float wait = -1f, loopTime;

        void Start()
        {
            if (ids.Count == 0 && SakugaFX.Library) ids = SakugaFX.Library.Ids();
            Show(index);
        }

        void Update()
        {
            if (SakugaDemoInput.Pressed(SakugaDemoInput.Key.Right)) Show(index + 1);
            else if (SakugaDemoInput.Pressed(SakugaDemoInput.Key.Left)) Show(index - 1);
            else if (SakugaDemoInput.Pressed(SakugaDemoInput.Key.Down)) Show(NextCategory(+1));
            else if (SakugaDemoInput.Pressed(SakugaDemoInput.Key.Up)) Show(NextCategory(-1));
            else if (SakugaDemoInput.Pressed(SakugaDemoInput.Key.Space)) Show(index);
            else if (SakugaDemoInput.Pressed(SakugaDemoInput.Key.One)) { size = SakugaSize.S; Show(index); }
            else if (SakugaDemoInput.Pressed(SakugaDemoInput.Key.Two)) { size = SakugaSize.M; Show(index); }
            else if (SakugaDemoInput.Pressed(SakugaDemoInput.Key.Three)) { size = SakugaSize.L; Show(index); }
            if (current && current.IsPlaying && current.LoopsMain)
            {
                loopTime += Time.deltaTime;
                if (loopTime > 3.5f) current.Stop();
            }
            if (wait < 0f && (!current || !current.isActiveAndEnabled || !current.IsPlaying)) wait = replayPause;
            if (wait >= 0f) { wait -= Time.deltaTime; if (wait < 0f) Show(index); }
        }

        int NextCategory(int dir)
        {
            var lib = SakugaFX.Library;
            if (!lib || ids.Count == 0) return index + dir;
            string cat = lib.Get(ids[Wrap(index)])?.category;
            for (int k = 1; k < ids.Count; k++)
            {
                int i = Wrap(index + dir * k);
                if (lib.Get(ids[i])?.category != cat) return i;
            }
            return index;
        }

        int Wrap(int i) => ids.Count == 0 ? 0 : ((i % ids.Count) + ids.Count) % ids.Count;

        public void Show(int i)
        {
            if (ids.Count == 0) return;
            index = Wrap(i);
            SakugaFX.ClearAll();
            wait = -1f; loopTime = 0f;
            current = Launch(ids[index], live: true, 0f);
            UpdateLabel();
        }

        /// <summary>Offline: effect i at t seconds.</summary>
        public void RenderAt(int i, float t)
        {
            index = Wrap(i);
            SakugaFX.ClearAll();
            Launch(ids[index], live: false, t);
            UpdateLabel();
        }

        void UpdateLabel()
        {
            if (!label) return;
            var e = SakugaFX.Library ? SakugaFX.Library.Get(ids[index]) : null;
            string uses = e != null && e.useCases != null && e.useCases.Length > 0 ? string.Join(", ", e.useCases, 0, Mathf.Min(2, e.useCases.Length)) : "";
            label.text = $"{index + 1}/{ids.Count}  {ids[index]}\n{e?.category}  {e?.style}  {uses}";
        }

        SakugaEffect Launch(string id, bool live, float t)
        {
            var prefab = SakugaFX.Library ? SakugaFX.Library.Find(id) : null;
            if (!prefab) return null;
            var cat = prefab.recipe.category;
            Vector3 ground = new Vector3(center.x, 0f, center.z);
            if (cat == "projectile")
            {
                var shot = SakugaShot.Cast(0f, id, left, right);
                shot.size = size;
                if (live) return shot.Fire();
                shot.SimulateAt(t);
                return null;
            }
            var fx = SakugaFX.Spawn(id);
            if (!fx) return null;
            fx.size = size;
            Vector3 at = prefab.recipe.anchor == "ground" ? ground : center;
            if (cat == "shield" || cat == "buff") at = dummy ? dummy.position + Vector3.up * 1.0f : center;
            if (cat == "aura") at = dummy ? dummy.position : ground;
            fx.transform.position = at;
            fx.hitDirection = cat == "impact" || cat == "slash" ? Vector3.right : Vector3.zero;
            if (fx.LoopsMain) fx.lifetime = 3.5f;
            if (live) fx.Play(); else fx.Simulate(t);
            if (cat == "beam") fx.SetBeam(left, right);
            return fx;
        }
    }
}
