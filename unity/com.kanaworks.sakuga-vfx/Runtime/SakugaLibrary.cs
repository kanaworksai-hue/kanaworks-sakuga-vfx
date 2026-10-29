using System;
using System.Collections.Generic;
using UnityEngine;

namespace KanaWorks.SakugaVFX
{
    /// <summary>Every layered effect by id, with what it is for. Built by the importer as
    /// Assets/KanaWorksSakugaVFX/Resources/SakugaLibrary.asset, so SakugaFX finds it without any setup.</summary>
    [CreateAssetMenu(menuName = "KanaWorks/SAKUGA Library", fileName = "SakugaLibrary")]
    public class SakugaLibrary : ScriptableObject
    {
        [Serializable]
        public class Entry
        {
            public string id;
            public string element;
            public string category;
            public string style;
            public SakugaEffect prefab;
            public string[] tags;
            public string[] useCases;
        }

        public List<Entry> entries = new List<Entry>();
        Dictionary<string, Entry> byId;

        public SakugaEffect Find(string id)
        {
            if (byId == null || byId.Count != entries.Count)
            {
                byId = new Dictionary<string, Entry>();
                foreach (var e in entries) if (e != null && !string.IsNullOrEmpty(e.id)) byId[e.id] = e;
            }
            return byId.TryGetValue(id, out var hit) ? hit.prefab : null;
        }

        public Entry Get(string id) { Find(id); return byId != null && byId.TryGetValue(id, out var e) ? e : null; }

        /// <summary>Ids of one category (and optionally one element), in library order.</summary>
        public List<string> Ids(string category = null, string element = null)
        {
            var list = new List<string>();
            foreach (var e in entries)
                if ((category == null || e.category == category) && (element == null || e.element == element)) list.Add(e.id);
            return list;
        }

        void OnValidate() => byId = null;
    }
}
