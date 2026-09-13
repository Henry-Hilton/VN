using UnityEngine;
using UnityEngine.UI;

namespace YouthRise
{
    /// <summary>Vector rounded rectangle; no generated bitmap or per-button texture.</summary>
    public sealed class RoundedGraphic : Image
    {
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            Rect r = GetPixelAdjustedRect();
            float radius = Mathf.Min(18f, Mathf.Min(r.width, r.height) * 0.25f);
            Vector2[] centers = { new Vector2(r.xMax-radius, r.yMax-radius), new Vector2(r.xMin+radius, r.yMax-radius),
                new Vector2(r.xMin+radius, r.yMin+radius), new Vector2(r.xMax-radius, r.yMin+radius) };
            vh.AddVert(r.center, color, Vector2.zero);
            const int steps = 10;
            for (int corner = 0; corner < 4; corner++)
                for (int step = 0; step <= steps; step++)
                {
                    float angle = (corner * 90f + step * 90f / steps) * Mathf.Deg2Rad;
                    Vector2 p = centers[corner] + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
                    vh.AddVert(p, color, Vector2.zero);
                }
            int count = 4 * (steps + 1);
            for (int i = 1; i <= count; i++) vh.AddTriangle(0, i, i == count ? 1 : i + 1);
        }
    }
}
