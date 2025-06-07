using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(CanvasRenderer))]
public class GeneratedReticleUI : MaskableGraphic
{
    [SerializeField] private List<ReticlePart> reticleParts = new List<ReticlePart>();

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();
        if (reticleParts == null || reticleParts.Count == 0)
            return;

        foreach (var part in reticleParts)
        {
            if (part == null) continue;

            // compute inner radius
            float inner = part.fill
                ? 0f
                : Mathf.Max(0f, part.innerRadius);

            // draw using outerRadius instead of radius, so thickness actually works
            DrawArc(
                vh,
                part.radius,
                inner,
                part.sweepAngle,
                part.startDeg,
                part.CircleColor,
                ReticlePart.DefaultSegments
            );
        }
    }

    private void DrawArc(VertexHelper vh, float outer, float inner, float sweepDeg, float startDeg, Color circleColor, int segments)
    {
        // remember where our verts begin
        int startIndex = vh.currentVertCount;

        float startRad = Mathf.Clamp(startDeg, 0f, 360f) / 360f * Mathf.PI * 2f;
        float sweepRad = Mathf.Clamp01(sweepDeg / 360f) * Mathf.PI * 2f;
        float deltaTheta = sweepRad / segments;

        if (inner <= 0f)
        {
            // filled fan
            vh.AddVert(Vector2.zero, circleColor, Vector2.zero);
            for (int i = 0; i <= segments; i++)
            {
                float theta = startRad + i * deltaTheta;
                Vector2 pos = new Vector2(Mathf.Cos(theta), Mathf.Sin(theta)) * outer;
                vh.AddVert(pos, circleColor, Vector2.zero);

                if (i > 0)
                {
                    // connect center (startIndex) to the two outer verts
                    vh.AddTriangle(
                        startIndex,
                        startIndex + i,
                        startIndex + i + 1
                    );
                }
            }
        }
        else
        {
            // hollow ring
            for (int i = 0; i < segments; i++)
            {
                float thetaA = startRad + i * deltaTheta;
                float thetaB = startRad + (i + 1) * deltaTheta;

                Vector2 oA = new Vector2(Mathf.Cos(thetaA), Mathf.Sin(thetaA)) * outer;
                Vector2 oB = new Vector2(Mathf.Cos(thetaB), Mathf.Sin(thetaB)) * outer;
                Vector2 iA = new Vector2(Mathf.Cos(thetaA), Mathf.Sin(thetaA)) * inner;
                Vector2 iB = new Vector2(Mathf.Cos(thetaB), Mathf.Sin(thetaB)) * inner;

                int idx = vh.currentVertCount;
                vh.AddVert(oA, circleColor, Vector2.zero);
                vh.AddVert(oB, circleColor, Vector2.zero);
                vh.AddVert(iB, circleColor, Vector2.zero);
                vh.AddVert(iA, circleColor, Vector2.zero);

                vh.AddTriangle(idx + 0, idx + 1, idx + 2);
                vh.AddTriangle(idx + 2, idx + 3, idx + 0);
            }
        }
    }

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();
        SetVerticesDirty();
    }
#endif
}
