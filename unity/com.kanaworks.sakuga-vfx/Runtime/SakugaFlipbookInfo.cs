using System;
using UnityEngine;

namespace KanaWorks.SakugaVFX
{
    /// <summary>
    /// The &lt;id&gt;.flipbook.json sidecar next to every sheet. Field names match the file (and the effekseer-vfx
    /// skill's export: grid, cell, frames, bottom_inset, source); fps, loop, blend and pivot are additions.
    /// </summary>
    [Serializable]
    public class SakugaFlipbookInfo
    {
        public int[] grid;
        public int[] cell;
        public int frames;
        public float bottom_inset;
        public string source;
        public string id;
        public string element;
        public string form;
        public float fps = 30f;
        public bool loop;
        public string blend = "alpha";
        /// <summary>Normalized cell position of the pivot, origin top-left, y down.</summary>
        public float[] pivot;
        public string version;
        public string credit;

        public const float PixelsPerUnit = 256f;

        public static SakugaFlipbookInfo FromJson(string json)
        {
            var info = JsonUtility.FromJson<SakugaFlipbookInfo>(json);
            if (info == null || info.grid == null || info.grid.Length != 2 || info.grid[0] < 1 || info.grid[1] < 1)
                throw new FormatException("flipbook sidecar needs \"grid\": [cols, rows]");
            if (info.cell == null || info.cell.Length != 2 || info.cell[0] < 1 || info.cell[1] < 1)
                throw new FormatException("flipbook sidecar needs \"cell\": [width, height]");
            int max = info.grid[0] * info.grid[1];
            if (info.frames < 1 || info.frames > max) info.frames = max;
            if (info.fps <= 0f) info.fps = 30f;
            return info;
        }

        public bool Additive => string.Equals(blend, "additive", StringComparison.OrdinalIgnoreCase);

        /// <summary>Columns and strikes stand on the ground and turn only around their up axis.</summary>
        public bool GroundRooted => form == "column" || form == "strike";

        /// <summary>Pivot in UV space (origin bottom-left, y up).</summary>
        public Vector2 PivotUV
        {
            get
            {
                if (pivot == null || pivot.Length != 2) return new Vector2(0.5f, 0.5f);
                return new Vector2(pivot[0], 1f - pivot[1]);
            }
        }

        /// <summary>World size of one cell at 256 px per unit.</summary>
        public Vector2 WorldSize => new Vector2(cell[0] / PixelsPerUnit, cell[1] / PixelsPerUnit);
    }
}
